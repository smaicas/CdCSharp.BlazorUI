using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestrator.Models;
using CdCSharp.Theon.Tools;
using CdCSharp.Theon.Tracing;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace CdCSharp.Theon.Orchestrator;

public interface IOrchestrator : IDisposable
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
    private readonly SharedProjectKnowledge _sharedKnowledge;
    private readonly ContextRegistry _registry;
    private readonly ContextBudgetManager _budgetManager;
    private readonly PromptFormatter _promptFormatter;
    private readonly ToolDispatcher _toolDispatcher;
    private readonly PlanValidator _planValidator;
    private readonly TheonOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public OrchestratorState State { get; } = new();

    private const string SystemPrompt = """
    You are an orchestrator coordinating specialized contexts to analyze C# codebases.

    ## Your Role
    You COORDINATE experts, you don't analyze code yourself. You MUST plan before acting on complex tasks.

    ## CRITICAL WORKFLOW
    1. **ALWAYS start with create_execution_plan** for any non-trivial request (documentation, analysis, refactoring)
    2. **Execute each step of the plan IN EXACT ORDER using query_context**
    3. **The system ENFORCES plan order** - you cannot skip steps
    4. **Collect all information before generating output**
    5. **Use generate_output_file only AFTER completing ALL plan steps**

    ## PLAN EXECUTION RULES - READ CAREFULLY
    
    When you have a plan with steps, you MUST:
    
    ### Step Tracking
    - The plan status section shows which steps are Pending/InProgress/Completed
    - ONLY query the NEXT pending step's targetContext
    - If you try to skip a step, the system will reject your query
    - After completing step N, proceed IMMEDIATELY to step N+1
    
    ### Example Plan Flow
    ```
    Step 1: [ArchitectureAnalyzer] Understand structure → COMPLETE
    Step 2: [CodeExplorer] Examine key files       → PENDING (you MUST do THIS next)
    Step 3: [DependencyAnalyzer] Analyze DI        → PENDING
    ```
    
    After step 1 completes, you will receive:
    ```json
    {
      "contextName": "ArchitectureAnalyzer",
      "answer": "...",
      "completedStepNumber": 1,
      "totalSteps": 3,
      "nextStepAction": "Proceed to Step 2: Query 'CodeExplorer' with question \"Which files contain core functionality?\""
    }
    ```
    
    You MUST then call query_context with:
    - context_name: "CodeExplorer" (the targetContext of step 2)
    - question: The exact question from step 2
    - files: The suggestedFiles from step 2
    
    ### Handling Step Results
    - Each query_context response includes step completion information:
      * completedStepNumber: which step just finished
      * totalSteps: how many steps in total
      * nextStepAction: EXACT instructions for the next step
    - READ the nextStepAction field carefully - it tells you exactly what to do next
    - **Some steps allow multiple calls** (shown as "allows N more call(s)" in nextStepAction)
      * You may query the same context again with a refined question
      * OR proceed to the next step if you have enough information
    - If a step returns incomplete results (no files examined), that step is STILL complete
    - The validation system will catch issues - your job is to FOLLOW THE PLAN
    - Do NOT retry the same step unless it explicitly allows multiple calls
    - When nextStepAction says "All steps complete", use generate_output_file
    
    ### After All Steps Complete
    - When ALL steps show "Completed", synthesize the collected information
    - Use generate_output_file to create the final documentation/report
    - Include insights from ALL completed steps

    ## Tools Available
    - **create_execution_plan**: MUST call first for documentation, analysis, or comprehensive tasks
    - **query_context**: Ask CodeExplorer, ArchitectureAnalyzer, or DependencyAnalyzer
    - **propose_file_change**: Propose modifications (requires confirmation)
    - **create_project_file**: Create new files immediately
    - **generate_output_file**: Generate documentation/reports (only after plan execution)

    ## Planning Rules
    - Simple questions (e.g., "what does X do?") → can skip planning, use query_context directly
    - Documentation requests → MUST plan first
    - Analysis requests → MUST plan first
    - Refactoring requests → MUST plan first
    - Any request involving multiple files or comprehensive output → MUST plan first

    ## CRITICAL REMINDERS
    - READ the plan status carefully before each action
    - FOLLOW the step order exactly as shown
    - DO NOT skip or repeat steps
    - The system WILL reject out-of-order queries
    - Trust the plan - it was created for a reason
    """;

    public Orchestrator(
        IAIClient aiClient,
        IContextFactory factory,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
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
        _sharedKnowledge = sharedKnowledge;
        _registry = registry;
        _budgetManager = budgetManager;
        _promptFormatter = promptFormatter;
        _options = options.Value;
        _toolDispatcher = ToolDispatcher.CreateForOrchestrator();
        _planValidator = new PlanValidator();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        InitializePredefinedContexts();
    }

    public async Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default)
    {
        ThrowIfDisposed();
        ct.ThrowIfCancellationRequested();

        Tracer.StartSession($"Query: {userInput}");

        try
        {
            using (Tracer.Span("orchestrator", "Orchestrator", userInput))
            {
                State.AddUserMessage(userInput);
                OrchestratorResponse response = await ExecuteWithToolLoop(ct);
                Tracer.EndSession(true, response.Message);
                return response;
            }
        }
        catch (OperationCanceledException)
        {
            Tracer.EndSession(false, "Cancelled by user");
            throw;
        }
        catch (Exception ex)
        {
            Tracer.Record(new ErrorEvent(ex.GetType().Name, ex.Message, ex.StackTrace));
            Tracer.EndSession(false, ex.Message);
            throw;
        }
    }

    public async Task<OrchestratorResponse> ConfirmChangesAsync(bool confirm, string? changeIds = null, CancellationToken ct = default)
    {
        ThrowIfDisposed();
        ct.ThrowIfCancellationRequested();

        IEnumerable<ProposedChange?> changesToProcess = string.IsNullOrEmpty(changeIds) || changeIds == "all"
            ? State.GetPendingChanges()
            : changeIds.Split(',').Select(id => State.GetPendingChange(id.Trim())).Where(c => c != null)!;

        List<string> applied = [];
        List<string> rejected = [];

        foreach (ProposedChange? change in changesToProcess)
        {
            ct.ThrowIfCancellationRequested();

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
        ThrowIfDisposed();
        State.Clear();
        _registry.Clear();
        _budgetManager.Clear();
        InitializePredefinedContexts();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        State.Clear();
        _registry.Clear();
        _budgetManager.Clear();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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

    private async Task<OrchestratorResponse> ExecuteWithToolLoop(CancellationToken ct)
    {
        _logger.Section("Processing Request"); // ADD THIS

        List<string> createdFiles = [];
        List<string> generatedOutputs = [];
        List<ProposedChange> proposedChanges = [];

        int maxIterations = 30;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            ct.ThrowIfCancellationRequested();

            ChatCompletionRequest request = await BuildRequest(ct);

            string? firstUserMsg = request.Messages.FirstOrDefault(m => m.Role == "user")?.Content;

            // ADD THIS LOG CALL:
            _logger.LogLlmCall("Orchestrator", request.Messages.Count, request.Tools?.Count);

            // KEEP EXISTING TRACER:
            Tracer.Record(new LlmRequestEvent(
                request.Model,
                request.Messages.Count,
                request.Tools?.Count ?? 0,
                firstUserMsg));

            Stopwatch sw = Stopwatch.StartNew();
            ChatCompletionResponse response = await _aiClient.SendAsync(request, ct);
            sw.Stop();

            Choice choice = response.Choices[0];

            // KEEP EXISTING TRACER:
            Tracer.Record(new LlmResponseEvent(
                choice.FinishReason,
                choice.Message.Content,
                choice.Message.ToolCalls?.Count,
                sw.Elapsed.TotalMilliseconds));

            if (choice.FinishReason == "tool_calls" && choice.Message.ToolCalls?.Count > 0)
            {
                State.AddAssistantMessage(choice.Message);

                foreach (ToolCall toolCall in choice.Message.ToolCalls)
                {
                    ct.ThrowIfCancellationRequested();

                    // ADD LOGGING BEFORE TOOL EXECUTION:
                    string? toolDetails = ExtractToolDetails(toolCall);
                    _logger.LogToolCall("Orchestrator", toolCall.Function.Name, toolDetails);

                    // KEEP EXISTING TRACER:
                    Tracer.Record(new ToolCallEvent(toolCall.Function.Name, toolCall.Function.Arguments));

                    Stopwatch toolSw = Stopwatch.StartNew();
                    string result = await ExecuteTool(toolCall, ct, createdFiles, generatedOutputs, proposedChanges);
                    toolSw.Stop();

                    bool isError = result.Contains("\"error\"");

                    // ADD LOGGING AFTER TOOL EXECUTION:
                    string? summary = ExtractToolResultSummary(toolCall.Function.Name, result, isError);
                    _logger.LogToolResult("Orchestrator", toolCall.Function.Name, !isError, summary);

                    // KEEP EXISTING TRACER:
                    Tracer.Record(new ToolResultEvent(toolCall.Function.Name, result, isError, toolSw.Elapsed.TotalMilliseconds));

                    State.AddToolResult(toolCall.Id, result);
                }

                continue;
            }

            State.AddAssistantMessage(choice.Message);

            _logger.Success("Request completed"); // ADD THIS

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

    private string? ExtractToolDetails(ToolCall toolCall)
    {
        try
        {
            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments, _jsonOptions);

            return toolCall.Function.Name switch
            {
                "query_context" => $"{args?["context_name"]} - {args?["question"].GetString()?[..50]}...",
                "create_execution_plan" => $"{args?["user_request"].GetString()?[..50]}...",
                "propose_file_change" => args?["path"].GetString(),
                "create_project_file" => args?["path"].GetString(),
                "generate_output_file" => $"{args?["folder"]}/{args?["filename"]}",
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private string? ExtractToolResultSummary(string toolName, string result, bool isError)
    {
        if (isError)
        {
            try
            {
                Dictionary<string, JsonElement>? error = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result);
                return error?["error"].GetString();
            }
            catch
            {
                return "Error occurred";
            }
        }

        try
        {
            Dictionary<string, JsonElement>? data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result);

            return toolName switch
            {
                "create_execution_plan" => $"{data?["steps"].GetArrayLength()} steps created",
                "query_context" => $"Examined {data?["filesExamined"].GetArrayLength()} files",
                "generate_output_file" => data?["fullPath"].GetString(),
                "create_project_file" => data?["path"].GetString(),
                "propose_file_change" => $"{data?["changeType"]} - {data?["path"].GetString()}",
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task<ChatCompletionRequest> BuildRequest(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        await _sharedKnowledge.GetProjectAsync(ct);

        string fileIndex = _promptFormatter.FormatFileIndex();
        string contextsOverview = _promptFormatter.FormatContextsOverview();
        string pendingChanges = _promptFormatter.FormatPendingChanges(
            State.GetPendingChanges().Select(c => (c.Id, c.Path, c.Description)));
        string planStatus = _promptFormatter.FormatPlanStatus(State.CurrentPlan);

        string fullSystemPrompt = $"""
            {SystemPrompt}

            {fileIndex}

            {contextsOverview}

            {(string.IsNullOrEmpty(planStatus) ? "" : planStatus)}

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
        CancellationToken ct,
        List<string> createdFiles,
        List<string> generatedOutputs,
        List<ProposedChange> proposedChanges)
    {
        Dictionary<string, JsonElement>? args =
            JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(toolCall.Function.Arguments, _jsonOptions);

        ToolContext toolContext = CreateToolContext();

        object result = await _toolDispatcher.DispatchAsync(toolCall.Function.Name, args!, toolContext, ct);

        if (result is CreatedFile cf) createdFiles.Add(cf.Path);
        if (result is GeneratedOutput go) generatedOutputs.Add(go.FullPath);
        if (result is ProposedChange pc) proposedChanges.Add(pc);

        return toolContext.Infrastructure.Serialize(result);
    }

    private ToolContext CreateToolContext()
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