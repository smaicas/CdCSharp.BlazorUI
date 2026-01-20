using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context.Planning;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Tools;
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

    public string Name => Configuration.Name;
    public bool IsStateful => Configuration.IsStateful;
    public ContextState State { get; } = new();
    public ContextConfiguration Configuration { get; }

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
        Configuration = config;
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
            PropertyNameCaseInsensitive = true,
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

        // Note: We don't load InitialFiles here anymore
        // Contexts should use peek_file or read_file explicitly
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
        BudgetAllocation? allocation = _budgetManager.GetAllocation(Configuration.Name);
        allocation?.Reset();
    }

    private async Task<TResponse> ExecuteWithToolLoop<TResponse>(ITracerScope? tracerScope, CancellationToken ct)
        where TResponse : class, new()
    {
        int maxIterations = 15;
        Dictionary<string, string> ephemeralFiles = []; // Peeked files for next request only

        for (int i = 0; i < maxIterations; i++)
        {
            ct.ThrowIfCancellationRequested();

            BudgetAllocation? allocation = _budgetManager.GetAllocation(Configuration.Name);
            if (allocation != null && allocation.Status == BudgetStatus.Exhausted)
            {
                _logger.Warning($"[{Name}] Budget exhausted");
                break;
            }

            ChatCompletionRequest request = await BuildRequest<TResponse>(ephemeralFiles);
            tracerScope?.RecordLlmRequest(request);

            Stopwatch sw = Stopwatch.StartNew();
            ChatCompletionResponse response = await _aiClient.SendAsync(request, ct);
            sw.Stop();
            tracerScope?.RecordLlmResponse(response, sw.Elapsed);

            Choice choice = response.Choices[0];

            // Clear ephemeral files after each request
            ephemeralFiles.Clear();

            if (choice.FinishReason == "tool_calls" && choice.Message.ToolCalls?.Count > 0)
            {
                State.AddMessage(choice.Message);

                foreach (ToolCall toolCall in choice.Message.ToolCalls)
                {
                    Stopwatch toolSw = Stopwatch.StartNew();
                    string result = await ExecuteTool(toolCall, tracerScope, ephemeralFiles, ct);
                    toolSw.Stop();

                    tracerScope?.RecordToolExecution(toolCall, result, toolSw.Elapsed, result.Contains("\"error\""));
                    State.AddMessage(new Message { Role = "tool", Content = result, ToolCallId = toolCall.Id });
                }

                continue;
            }

            // Final response - use structured output
            State.AddMessage(choice.Message);
            return ParseStructuredResponse<TResponse>(choice.Message.Content);
        }

        _logger.Warning($"[{Name}] Max iterations reached");
        return new TResponse();
    }

    private TResponse ParseStructuredResponse<TResponse>(string? content) where TResponse : class, new()
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.Warning($"[{Name}] Empty response from LLM");
            return new TResponse();
        }

        try
        {
            TResponse? parsed = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
            if (parsed != null)
                return parsed;

            _logger.Warning($"[{Name}] Failed to deserialize response");
            return new TResponse();
        }
        catch (JsonException ex)
        {
            _logger.Warning($"[{Name}] JSON parsing error: {ex.Message}");
            _logger.Debug($"Content: {content}");
            return new TResponse();
        }
    }

    private async Task<ChatCompletionRequest> BuildRequest<TResponse>(Dictionary<string, string> ephemeralFiles)
    {
        await _sharedKnowledge.GetProjectAsync();

        string systemPrompt = BuildSystemPrompt(ephemeralFiles);

        List<Message> messages = [
            new() { Role = "system", Content = systemPrompt },
        .. State.History.ToArray()
        ];

        ChatCompletionRequest request = new()
        {
            Model = Configuration.Model,
            Messages = messages,
            Tools = ContextToolDefinitions.GetTools(Configuration),
            Temperature = 0.3
        };

        // CRITICAL: Add structured output based on response type
        object? schema = GetResponseSchema<TResponse>();
        if (schema != null)
        {
            request.ResponseFormat = new ResponseFormat
            {
                Type = "json_schema",
                JsonSchema = new JsonSchema
                {
                    Name = GetSchemaName<TResponse>(),
                    Strict = "true",
                    Schema = schema
                }
            };
        }

        return request;
    }

    private object? GetResponseSchema<TResponse>()
    {
        // For Planner, use the ExecutionPlan schema
        if (Configuration.ContextType == "Planner")
        {
            return PlannerContextConfiguration.GetResponseSchema(
                _registry.GetAllContexts());
        }

        // For standard contexts, use ContextInfoResponse
        if (typeof(TResponse) == typeof(ContextInfoResponse))
        {
            return ResponseSchemas.GetSchemaFor<TResponse>();
        }

        return null;
    }

    private bool ShouldUseStructuredOutput<TResponse>()
    {
        // Use structured output for known response types
        return typeof(TResponse) == typeof(ContextInfoResponse);
    }

    private string GetSchemaName<TResponse>()
    {
        if (Configuration.ContextType == "Planner")
            return "execution_plan";

        if (typeof(TResponse) == typeof(ContextInfoResponse))
            return "context_info_response";

        return "generic_response";
    }

    private string BuildSystemPrompt(Dictionary<string, string> ephemeralFiles)
    {
        string fileIndex = _promptFormatter.FormatFileIndex();
        string contextsOverview = _promptFormatter.FormatContextsOverview(Configuration.Name);

        BudgetAllocation? allocation = _budgetManager.GetAllocation(Configuration.Name);
        int usedTokens = allocation?.UsedTokens ?? 0;

        string status = _promptFormatter.FormatContextStatus(
            Configuration.Name,
            Configuration.ContextType,
            usedTokens,
            Configuration.MaxTokenBudget,
            _cloneDepth,
            Configuration.MaxCloneDepth);

        string loadedFiles = _promptFormatter.FormatLoadedFiles(State.FileContents);
        string peekedFiles = ephemeralFiles.Count > 0
            ? _promptFormatter.FormatPeekedFiles(ephemeralFiles)
            : "";

        return $"""
            {Configuration.SystemPrompt}

            {fileIndex}

            {contextsOverview}

            {status}

            ## Files in YOUR Context (Permanent)
            {loadedFiles}

            {peekedFiles}
            """;
    }

    private async Task<string> ExecuteTool(
        ToolCall toolCall,
        ITracerScope? tracerScope,
        Dictionary<string, string> ephemeralFiles,
        CancellationToken ct)
    {
        _logger.Debug($"[{Name}] Tool: {toolCall.Function.Name}");

        try
        {
            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments, _jsonOptions);

            ToolContext toolContext = CreateToolContext(tracerScope);
            object result = await _toolDispatcher.DispatchAsync(toolCall.Function.Name, args!, toolContext, ct);

            // Handle ephemeral files from peek_file
            if (result is FileContent fc && fc.IsEphemeral)
            {
                ephemeralFiles[fc.Path] = fc.Content;
                _logger.Debug($"[{Name}] Added ephemeral file: {fc.Path}");
            }

            return toolContext.Infrastructure.Serialize(result);
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

    private ToolContext CreateToolContext(ITracerScope? tracerScope)
    {
        return new ToolContext
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
                Config = Configuration,
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