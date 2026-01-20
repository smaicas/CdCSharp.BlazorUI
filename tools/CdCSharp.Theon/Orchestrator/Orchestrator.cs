using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestrator.Models;
using CdCSharp.Theon.Tools;
using CdCSharp.Theon.Tools.Commands;
using CdCSharp.Theon.Tracing;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace CdCSharp.Theon.Orchestrator;

public interface IOrchestrator
{
    OrchestratorState State { get; }
    Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default);
    Task<OrchestratorResponse> ConfirmChangesAsync(bool confirm, string? changeIds = null, CancellationToken ct = default);
    void Reset();
}

public sealed class Orchestrator : IOrchestrator
{
    private readonly IAIClient _aiClient;
    private readonly IContextFactory _factory;
    private readonly IProjectContext _projectContext;
    private readonly IFileSystem _fileSystem;
    private readonly ITheonLogger _logger;
    private readonly ITracer _tracer;
    private readonly SharedProjectKnowledge _sharedKnowledge;
    private readonly ContextRegistry _registry;
    private readonly ContextBudgetManager _budgetManager;
    private readonly PromptFormatter _promptFormatter;
    private readonly ToolDispatcher _toolDispatcher;
    private readonly TheonOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrchestratorState State { get; } = new();

    private const string SystemPrompt = """
        You are an orchestrator coordinating specialized contexts to analyze C# codebases.

        ## Your Role
        You COORDINATE experts, you don't analyze code yourself. Delegate technical questions.

        ## How to Work
        1. Identify what expertise is needed
        2. Use query_context to ask specialists
        3. Specialists read files and provide analysis
        4. Synthesize their responses
        5. Use generate_output_file for documentation

        ## Tools Available
        - query_context: Ask CodeExplorer, ArchitectureAnalyzer, or DependencyAnalyzer
        - propose_file_change: Propose modifications (requires confirmation)
        - create_project_file: Create new files immediately
        - generate_output_file: Generate documentation/reports

        ## Important
        - ALWAYS delegate technical questions to contexts
        - Be specific about what you need from each context
        - Contexts can see each other's loaded files
        """;

    public Orchestrator(
        IAIClient aiClient,
        IContextFactory factory,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer,
        SharedProjectKnowledge sharedKnowledge,
        ContextRegistry registry,
        ContextBudgetManager budgetManager,
        PromptFormatter promptFormatter,
        IOptions<TheonOptions> options)
    {
        _aiClient = aiClient;
        _factory = factory;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _sharedKnowledge = sharedKnowledge;
        _registry = registry;
        _budgetManager = budgetManager;
        _promptFormatter = promptFormatter;
        _options = options.Value;
        _toolDispatcher = ToolDispatcher.CreateForOrchestrator();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        InitializePredefinedContexts();
    }

    public async Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default)
    {
        _logger.Debug($"Processing: {userInput[..Math.Min(50, userInput.Length)]}...");

        using ITracerScope tracerScope = _tracer.BeginOrchestration(userInput);

        State.AddUserMessage(userInput);

        try
        {
            OrchestratorResponse response = await ExecuteWithToolLoop(tracerScope, ct);

            tracerScope.SetResult(new ExecutionResult
            {
                Success = true,
                MessagePreview = response.Message.Length > 200 ? response.Message[..200] + "..." : response.Message,
                CreatedFiles = response.CreatedFiles,
                GeneratedOutputs = response.GeneratedOutputs,
                ProposedChanges = response.ProposedChanges.Select(c => new ProposedChangeTrace
                {
                    Id = c.Id,
                    Path = c.Path,
                    ChangeType = c.ChangeType.ToString(),
                    Description = c.Description
                }).ToList()
            });

            return response;
        }
        catch (Exception ex)
        {
            tracerScope.SetResult(new ExecutionResult { Success = false, Error = ex.Message });
            throw;
        }
    }

    public async Task<OrchestratorResponse> ConfirmChangesAsync(bool confirm, string? changeIds = null, CancellationToken ct = default)
    {
        IEnumerable<ProposedChange> changesToProcess = string.IsNullOrEmpty(changeIds) || changeIds == "all"
            ? State.GetPendingChanges()
            : changeIds.Split(',').Select(id => State.GetPendingChange(id.Trim())).Where(c => c != null)!;

        List<string> applied = [];
        List<string> rejected = [];

        foreach (ProposedChange change in changesToProcess)
        {
            if (confirm)
            {
                bool success = await _fileSystem.WriteProjectFileAsync(change.Path, change.NewContent, ct);
                if (success)
                {
                    State.MarkChangeApplied(change.Id);
                    applied.Add(change.Path);
                }
            }
            else
            {
                State.MarkChangeRejected(change.Id);
                rejected.Add(change.Path);
            }
        }

        string message = confirm
            ? $"Applied changes to: {string.Join(", ", applied)}"
            : $"Rejected changes to: {string.Join(", ", rejected)}";

        return new OrchestratorResponse
        {
            Message = message,
            ModifiedFiles = applied,
            Confidence = 1.0f,
            NeedsConfirmation = false
        };
    }

    public void Reset()
    {
        State.Clear();
        _registry.Clear();
        _budgetManager.Clear();
        InitializePredefinedContexts();
    }

    private void InitializePredefinedContexts()
    {
        IContextScope codeExplorer = _factory.CreateDelegate("CodeExplorer", "Code analysis");
        IContextScope architectureAnalyzer = _factory.CreateDelegate("ArchitectureAnalyzer", "Architecture analysis");
        IContextScope dependencyAnalyzer = _factory.CreateDelegate("DependencyAnalyzer", "Dependency analysis");

        State.RegisterContext("CodeExplorer", codeExplorer);
        State.RegisterContext("ArchitectureAnalyzer", architectureAnalyzer);
        State.RegisterContext("DependencyAnalyzer", dependencyAnalyzer);
    }

    private async Task<OrchestratorResponse> ExecuteWithToolLoop(ITracerScope tracerScope, CancellationToken ct)
    {
        List<string> createdFiles = [];
        List<string> generatedOutputs = [];
        List<ProposedChange> proposedChanges = [];

        int maxIterations = 20;
        int iteration = 0;

        while (iteration < maxIterations)
        {
            ct.ThrowIfCancellationRequested();
            iteration++;

            ChatCompletionRequest request = await BuildRequest();
            tracerScope.RecordLlmRequest(request);

            Stopwatch sw = Stopwatch.StartNew();
            ChatCompletionResponse response = await _aiClient.SendAsync(request, ct);

            sw.Stop();

            tracerScope.RecordLlmResponse(response, sw.Elapsed);

            Choice choice = response.Choices[0];

            if (choice.FinishReason == "tool_calls" && choice.Message.ToolCalls?.Count > 0)
            {
                State.AddAssistantMessage(choice.Message);

                foreach (ToolCall toolCall in choice.Message.ToolCalls)
                {
                    Stopwatch toolSw = Stopwatch.StartNew();
                    string result = await ExecuteTool(toolCall, tracerScope, ct, createdFiles, generatedOutputs, proposedChanges);
                    toolSw.Stop();

                    tracerScope.RecordToolExecution(toolCall, result, toolSw.Elapsed, result.Contains("\"error\""));
                    State.AddToolResult(toolCall.Id, result);
                }

                continue;
            }

            State.AddAssistantMessage(choice.Message);

            return new OrchestratorResponse
            {
                Message = choice.Message.Content ?? string.Empty,
                ProposedChanges = proposedChanges,
                CreatedFiles = createdFiles,
                GeneratedOutputs = generatedOutputs,
                ModifiedFiles = [],
                Confidence = 0.9f,
                NeedsConfirmation = proposedChanges.Any(c => c.Status == ChangeStatus.Pending)
            };
        }

        _logger.Warning($"Orchestrator reached max iterations ({maxIterations})");
        return new OrchestratorResponse
        {
            Message = "Processing stopped after maximum iterations.",
            ProposedChanges = proposedChanges,
            CreatedFiles = createdFiles,
            GeneratedOutputs = generatedOutputs,
            ModifiedFiles = [],
            Confidence = 0.5f,
            NeedsConfirmation = false
        };
    }

    private async Task<ChatCompletionRequest> BuildRequest()
    {
        await _sharedKnowledge.GetProjectAsync();

        string fileIndex = _promptFormatter.FormatFileIndex();
        string contextsOverview = _promptFormatter.FormatContextsOverview();
        string pendingChanges = _promptFormatter.FormatPendingChanges(
            State.GetPendingChanges().Select(c => (c.Id, c.Path, c.Description)));

        string fullSystemPrompt = $"""
        {SystemPrompt}

        {fileIndex}

        {contextsOverview}

        {(string.IsNullOrEmpty(pendingChanges) ? "" : $"## Pending Changes\n{pendingChanges}")}
        """;

        List<Message> messages =
        [
            new() { Role = "system", Content = fullSystemPrompt },
        .. State.ConversationHistory
        ];

        return new ChatCompletionRequest
        {
            Model = _options.Llm.Model,
            Messages = messages,
            Tools = OrchestratorToolDefinitions.All,
            Temperature = 0.5
        };
    }

    private async Task<string> ExecuteTool(
        ToolCall toolCall,
        ITracerScope tracerScope,
        CancellationToken ct,
        List<string> createdFiles,
        List<string> generatedOutputs,
        List<ProposedChange> proposedChanges)
    {
        _logger.Debug($"Executing tool: {toolCall.Function.Name}");

        try
        {
            if (toolCall.Function.Name == "query_context")
            {
                return await HandleQueryContext(toolCall, tracerScope, ct);
            }

            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments, _jsonOptions);

            CommandContext commandContext = CreateCommandContext(tracerScope);

            object result = await _toolDispatcher.DispatchAsync(
                toolCall.Function.Name,
                args!,
                null!,
                commandContext,
                ct);

            if (result is CreatedFile created)
            {
                createdFiles.Add(created.Path);
            }
            else if (result is GeneratedOutput generated)
            {
                generatedOutputs.Add(generated.FullPath);
            }
            else if (result is ProposedChange proposed)
            {
                proposedChanges.Add(proposed);
            }

            return commandContext.Infrastructure.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.Warning($"Tool execution failed: {ex.Message}");
            InfrastructureServices infra = new()
            {
                FileSystem = _fileSystem,
                Logger = _logger,
                Options = _options
            };
            return infra.Serialize(new { error = ex.Message });
        }
    }

    private async Task<string> HandleQueryContext(ToolCall toolCall, ITracerScope tracerScope, CancellationToken ct)
    {
        Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            toolCall.Function.Arguments, _jsonOptions);

        string contextName = args!["context_name"].GetString()!;
        string question = args["question"].GetString()!;
        string? filesArg = args.TryGetValue("files", out JsonElement f) ? f.GetString() : null;

        List<string>? files = filesArg?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(file => !string.IsNullOrWhiteSpace(file))
            .ToList();

        IContextScope? scope = State.GetContext(contextName);
        if (scope == null)
        {
            InfrastructureServices infra = new()
            {
                FileSystem = _fileSystem,
                Logger = _logger,
                Options = _options
            };
            return infra.Serialize(new { error = $"Context '{contextName}' not found" });
        }

        ContextQuery query = files?.Count > 0
            ? ContextQuery.WithFiles(question, files.ToArray())
            : ContextQuery.Simple(question);

        using ITracerScope contextScope = _tracer.BeginContext(contextName, question, files);
        Result<ContextInfoResponse> result = await scope.QueryAsync<ContextInfoResponse>(query, contextScope, ct);

        tracerScope.RecordContextQuery(contextScope.GetContextTrace());

        InfrastructureServices infrastructure = new()
        {
            FileSystem = _fileSystem,
            Logger = _logger,
            Options = _options
        };

        return result.Match(
            success => infrastructure.Serialize(new
            {
                context_name = contextName,
                question,
                answer = success.Answer,
                files_examined = success.FilesExamined
            }),
            error => infrastructure.Serialize(new { error = error.Message, code = error.Code }));
    }

    private CommandContext CreateCommandContext(ITracerScope tracerScope)
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
                State = null,
                Config = null,
                CloneDepth = 0
            },
            Orchestration = new OrchestrationCapabilities
            {
                Registry = _registry,
                Factory = _factory,
                BudgetManager = _budgetManager,
                State = State
            }
        };
    }
}