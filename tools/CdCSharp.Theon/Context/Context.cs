using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tools;
using CdCSharp.Theon.Tools.Commands;
using CdCSharp.Theon.Tracing;
using System.Diagnostics;
using System.Text.Json;

namespace CdCSharp.Theon.Context;

public interface IContext
{
    string Name { get; }
    bool IsStateful { get; }
    ContextState State { get; }
    ContextConfiguration Configuration { get; }

    Task<TResponse> AskAsync<TResponse>(ContextQuery query, CancellationToken ct = default) where TResponse : class, new();
    Task<TResponse> ContinueAsync<TResponse>(string followUpQuestion, CancellationToken ct = default) where TResponse : class, new();
    void Reset();
}

public sealed class Context : IContext
{
    private readonly ContextConfiguration _config;
    private readonly IAIClient _aiClient;
    private readonly IProjectContext _projectContext;
    private readonly IFileSystem _fileSystem;
    private readonly ITheonLogger _logger;
    private readonly ITracer _tracer;
    private readonly IContextFactory _factory;
    private readonly SharedProjectKnowledge _sharedKnowledge;
    private readonly ContextRegistry _registry;
    private readonly PromptFormatter _promptFormatter;
    private readonly ContextBudgetManager _budgetManager;
    private readonly ToolDispatcher _toolDispatcher;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TheonOptions _options;
    private readonly int _cloneDepth;

    public string Name => _config.Name;
    public bool IsStateful => _config.IsStateful;
    public ContextState State { get; } = new();
    public ContextConfiguration Configuration => _config;

    public Context(
        ContextConfiguration config,
        IAIClient aiClient,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer,
        IContextFactory factory,
        SharedProjectKnowledge sharedKnowledge,
        ContextRegistry registry,
        PromptFormatter promptFormatter,
        ContextBudgetManager budgetManager,
        TheonOptions options,
        int cloneDepth = 0)
    {
        _config = config;
        _aiClient = aiClient;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _factory = factory;
        _sharedKnowledge = sharedKnowledge;
        _registry = registry;
        _promptFormatter = promptFormatter;
        _budgetManager = budgetManager;
        _options = options;
        _cloneDepth = cloneDepth;

        _toolDispatcher = ToolDispatcher.CreateForContext();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _registry.RegisterContext(config.Name, config.ContextType, config.Speciality, config.MaxTokenBudget);
        _budgetManager.AllocateBudget(config.Name, config.MaxTokenBudget);
    }

    public Task<TResponse> AskAsync<TResponse>(ContextQuery query, CancellationToken ct = default)
        where TResponse : class, new()
        => AskAsync<TResponse>(query, null, ct);

    internal async Task<TResponse> AskAsync<TResponse>(
        ContextQuery query,
        ITracerScope? tracerScope,
        CancellationToken ct = default) where TResponse : class, new()
    {
        if (!IsStateful)
            State.Clear();

        await LoadInitialScope(query, tracerScope, ct);
        State.AddMessage(new Message { Role = "user", Content = query.Question });

        return await ExecuteWithToolLoop<TResponse>(tracerScope, ct);
    }

    public Task<TResponse> ContinueAsync<TResponse>(string followUpQuestion, CancellationToken ct = default)
        where TResponse : class, new()
        => ContinueAsync<TResponse>(followUpQuestion, null, ct);

    internal async Task<TResponse> ContinueAsync<TResponse>(
        string followUpQuestion,
        ITracerScope? tracerScope,
        CancellationToken ct = default) where TResponse : class, new()
    {
        if (!IsStateful)
            throw new InvalidOperationException("Cannot continue a stateless context.");

        State.AddMessage(new Message { Role = "user", Content = followUpQuestion });
        return await ExecuteWithToolLoop<TResponse>(tracerScope, ct);
    }

    public void Reset()
    {
        State.Clear();
        BudgetAllocation? allocation = _budgetManager.GetAllocation(_config.Name);
        allocation?.Reset();
    }

    private async Task LoadInitialScope(ContextQuery query, ITracerScope? tracerScope, CancellationToken ct)
    {
        if (query.InitialFiles == null) return;

        CommandContext commandContext = CreateCommandContext(tracerScope);

        foreach (string file in query.InitialFiles)
        {
            if (string.IsNullOrWhiteSpace(file)) continue;

            LoadFileCommand command = new() { Path = file };
            Result<LoadedFile> result = await _toolDispatcher.ExecuteCommandAsync(command, commandContext, ct);

            if (!result.IsSuccess)
            {
                _logger.Warning($"Failed to load initial file {file}: {result.Error.Message}");
            }
        }
    }

    private async Task<TResponse> ExecuteWithToolLoop<TResponse>(ITracerScope? tracerScope, CancellationToken ct)
        where TResponse : class, new()
    {
        int maxIterations = 15;

        for (int i = 0; i < maxIterations; i++)
        {
            ct.ThrowIfCancellationRequested();

            BudgetAllocation? allocation = _budgetManager.GetAllocation(_config.Name);
            if (allocation != null && allocation.Status == BudgetStatus.Exhausted)
            {
                _logger.Warning($"[{Name}] Budget exhausted");
                break;
            }

            ChatCompletionRequest request = await BuildRequest();
            tracerScope?.RecordLlmRequest(request);

            Stopwatch sw = Stopwatch.StartNew();
            ChatCompletionResponse response = await _aiClient.SendAsync(request, ct);
            sw.Stop();
            tracerScope?.RecordLlmResponse(response, sw.Elapsed);

            Choice choice = response.Choices[0];

            if (choice.FinishReason == "tool_calls" && choice.Message.ToolCalls?.Count > 0)
            {
                State.AddMessage(choice.Message);

                foreach (ToolCall toolCall in choice.Message.ToolCalls)
                {
                    Stopwatch toolSw = Stopwatch.StartNew();
                    string result = await ExecuteTool(toolCall, tracerScope, ct);
                    toolSw.Stop();

                    tracerScope?.RecordToolExecution(toolCall, result, toolSw.Elapsed, result.Contains("\"error\""));
                    State.AddMessage(new Message { Role = "tool", Content = result, ToolCallId = toolCall.Id });
                }

                continue;
            }

            State.AddMessage(choice.Message);
            return ParseFinalResponse<TResponse>(choice.Message.Content);
        }

        _logger.Warning($"[{Name}] Max iterations reached");
        return new TResponse();
    }

    private TResponse ParseFinalResponse<TResponse>(string? content) where TResponse : class, new()
    {
        if (string.IsNullOrWhiteSpace(content))
            return CreateDefaultResponse<TResponse>(string.Empty);

        try
        {
            string trimmed = content.Trim();
            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
            {
                TResponse? parsed = JsonSerializer.Deserialize<TResponse>(trimmed, _jsonOptions);
                if (parsed != null) return parsed;
            }
            return CreateDefaultResponse<TResponse>(content);
        }
        catch (JsonException)
        {
            return CreateDefaultResponse<TResponse>(content);
        }
    }

    private TResponse CreateDefaultResponse<TResponse>(string content) where TResponse : class, new()
    {
        if (typeof(TResponse) == typeof(ContextInfoResponse))
        {
            return (new ContextInfoResponse
            {
                Answer = content,
                FilesExamined = State.LoadedFiles.ToList(),
                Confidence = string.IsNullOrEmpty(content) ? 0.5f : 0.8f
            } as TResponse)!;
        }
        return new TResponse();
    }

    private async Task<ChatCompletionRequest> BuildRequest()
    {
        await _sharedKnowledge.GetProjectAsync();

        string systemPrompt = BuildSystemPrompt();

        List<Message> messages = [new() { Role = "system", Content = systemPrompt }, .. State.History];

        return new ChatCompletionRequest
        {
            Model = _config.Model,
            Messages = messages,
            Tools = ContextToolDefinitions.GetTools(_config),
            Temperature = 0.3
        };
    }

    private string BuildSystemPrompt()
    {
        string fileIndex = _promptFormatter.FormatFileIndex();
        string contextsOverview = _promptFormatter.FormatContextsOverview(_config.Name);

        BudgetAllocation? allocation = _budgetManager.GetAllocation(_config.Name);
        int usedTokens = allocation?.UsedTokens ?? 0;

        string status = _promptFormatter.FormatContextStatus(
            _config.Name,
            _config.ContextType,
            usedTokens,
            _config.MaxTokenBudget,
            _cloneDepth,
            _config.MaxCloneDepth);

        string loadedFiles = _promptFormatter.FormatLoadedFiles(State.FileContents);

        return $"""
            {_config.SystemPrompt}

            {fileIndex}

            {contextsOverview}

            {status}

            ## Files in YOUR Context
            {loadedFiles}
            """;
    }

    private async Task<string> ExecuteTool(ToolCall toolCall, ITracerScope? tracerScope, CancellationToken ct)
    {
        _logger.Debug($"[{Name}] Tool: {toolCall.Function.Name}");

        try
        {
            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments, _jsonOptions);

            InfrastructureServices infrastructure = new()
            {
                FileSystem = _fileSystem,
                Logger = _logger,
                Options = _options
            };

            QueryContext queryContext = new()
            {
                Infrastructure = infrastructure,
                Knowledge = new ProjectKnowledge
                {
                    Metadata = _sharedKnowledge,
                    Context = _projectContext
                },
                Execution = new ExecutionScope
                {
                    Tracer = tracerScope,
                    State = State,
                    Config = _config,
                    CloneDepth = _cloneDepth
                },
                Orchestration = new OrchestrationCapabilities
                {
                    Registry = _registry,
                    Factory = _factory,
                    BudgetManager = _budgetManager,
                    State = null
                }
            };

            CommandContext commandContext = CreateCommandContext(tracerScope);

            object result = await _toolDispatcher.DispatchAsync(toolCall.Function.Name, args!, queryContext, commandContext, ct);

            return infrastructure.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.Warning($"[{Name}] Tool failed: {ex.Message}");
            InfrastructureServices infra = new()
            {
                FileSystem = _fileSystem,
                Logger = _logger,
                Options = _options
            };
            return infra.Serialize(new { error = ex.Message });
        }
    }

    private CommandContext CreateCommandContext(ITracerScope? tracerScope)
    {
        return new CommandContext
        {
            Infrastructure = new InfrastructureServices
            {
                FileSystem = _fileSystem,
                Logger = _logger,
                Options = _options
            },
            Knowledge = new ProjectKnowledge
            {
                Metadata = _sharedKnowledge,
                Context = _projectContext
            },
            Execution = new ExecutionScope
            {
                Tracer = tracerScope,
                State = State,
                Config = _config,
                CloneDepth = _cloneDepth
            },
            Orchestration = new OrchestrationCapabilities
            {
                Registry = _registry,
                Factory = _factory,
                BudgetManager = _budgetManager,
                State = null
            }
        };
    }
}

public sealed class ContextInfoResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("answer")]
    public string Answer { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("files_examined")]
    public List<string> FilesExamined { get; init; } = [];

    [System.Text.Json.Serialization.JsonPropertyName("confidence")]
    public float Confidence { get; init; }
}