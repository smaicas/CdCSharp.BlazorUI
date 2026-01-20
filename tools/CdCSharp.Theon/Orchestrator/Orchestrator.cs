using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Context.Planning;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestrator.Models;
using CdCSharp.Theon.Tools;
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
    private readonly PlanValidator _planValidator;
    private readonly TheonOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrchestratorState State { get; } = new();

    private const string SystemPrompt = """
        You are an orchestrator coordinating specialized contexts to analyze C# codebases.

        ## Your Role
        You COORDINATE experts, you don't analyze code yourself. You MUST plan before acting on complex tasks.

        ## CRITICAL WORKFLOW
        1. **ALWAYS start with create_execution_plan** for any non-trivial request (documentation, analysis, refactoring)
        2. Execute each step of the plan IN ORDER using query_context
        3. The system ENFORCES plan order - you cannot skip steps
        4. Collect all information before generating output
        5. Use generate_output_file only AFTER completing ALL plan steps

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

        ## Executing a Plan
        When you have a plan:
        1. Query contexts IN THE EXACT ORDER specified by the plan
        2. Include the files suggested in each step
        3. The system will reject out-of-order queries
        4. After all steps complete, synthesize results and generate output

        ## Important
        - NEVER generate documentation without first consulting specialists through the plan
        - NEVER skip plan steps - the system enforces order
        - Follow the plan exactly as created
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
        IEnumerable<ProposedChange?> changesToProcess = string.IsNullOrEmpty(changeIds) || changeIds == "all"
            ? State.GetPendingChanges()
            : changeIds.Split(',').Select(id => State.GetPendingChange(id.Trim())).Where(c => c != null)!;

        List<string> applied = [];
        List<string> rejected = [];

        foreach (ProposedChange? change in changesToProcess)
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

        int maxIterations = 30;
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
        string planStatus = _promptFormatter.FormatPlanStatus(State.CurrentPlan);

        string fullSystemPrompt = $"""
            {SystemPrompt}

            {fileIndex}

            {contextsOverview}

            {(string.IsNullOrEmpty(planStatus) ? "" : planStatus)}

            {(string.IsNullOrEmpty(pendingChanges) ? "" : $"## Pending Changes\n{pendingChanges}")}
            """;

        List<Message> messages = [
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
            // Validate output generation against plan
            if (toolCall.Function.Name is "generate_output_file" or "create_project_file")
            {
                if (State.HasPlan && !State.HasExecutedPlan)
                {
                    _logger.Warning("Attempted to generate output before executing plan");
                    return _promptFormatter.FormatError(
                        "Cannot generate output before executing plan steps. " +
                        "Please complete the planned investigation first by calling query_context for each step.");
                }

                if (State.HasPlan && !_planValidator.CanGenerateOutput(State.CurrentPlan!))
                {
                    string progress = _planValidator.GetPlanProgress(State.CurrentPlan!);
                    return _promptFormatter.FormatError(
                        $"Cannot generate output yet. Plan progress: {progress}. " +
                        "Complete all plan steps before generating output.");
                }
            }

            if (toolCall.Function.Name == "query_context")
            {
                return await HandleQueryContext(toolCall, tracerScope, ct);
            }

            if (toolCall.Function.Name == "create_execution_plan")
            {
                return await HandleCreatePlan(toolCall, tracerScope, ct);
            }

            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments, _jsonOptions);

            ToolContext toolContext = CreateToolContext(tracerScope);

            object result = await _toolDispatcher.DispatchAsync(
                toolCall.Function.Name,
                args!,
                toolContext,
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

            return toolContext.Infrastructure.Serialize(result);
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

        // ENFORCE PLAN ORDER
        if (State.HasPlan)
        {
            Result<PlanStep> validation = _planValidator.ValidateQueryAgainstPlan(
                State.CurrentPlan!,
                contextName,
                files);

            if (!validation.IsSuccess)
            {
                _logger.Warning($"Plan validation failed: {validation.Error.Message}");
                InfrastructureServices infra = new()
                {
                    FileSystem = _fileSystem,
                    Logger = _logger,
                    Options = _options
                };
                return infra.Serialize(new
                {
                    error = validation.Error.Message,
                    code = validation.Error.Code,
                    plan_progress = _planValidator.GetPlanProgress(State.CurrentPlan!)
                });
            }
        }

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

        // Mark plan step as completed
        if (State.CurrentPlan != null)
        {
            result.Match(
                success =>
                {
                    PlanStep? step = State.CurrentPlan.Steps
                        .FirstOrDefault(s => s.Status == PlanStepStatus.Pending &&
                                           s.TargetContext.Equals(contextName, StringComparison.OrdinalIgnoreCase));

                    if (step != null)
                    {
                        State.MarkStepCompleted(step.Order, success.Answer);
                        _logger.Info($"Plan step {step.Order} completed: {step.Purpose}");
                    }
                    return true;
                },
                error =>
                {
                    PlanStep? step = State.CurrentPlan.Steps
                        .FirstOrDefault(s => s.Status == PlanStepStatus.Pending &&
                                           s.TargetContext.Equals(contextName, StringComparison.OrdinalIgnoreCase));

                    if (step != null)
                    {
                        State.MarkStepFailed(step.Order, error.Message);
                        _logger.Warning($"Plan step {step.Order} failed: {error.Message}");
                    }
                    return false;
                });
        }

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
                files_examined = success.FilesExamined,
                confidence = success.Confidence,
                plan_progress = State.CurrentPlan != null
                    ? _planValidator.GetPlanProgress(State.CurrentPlan)
                    : null
            }),
            error => infrastructure.Serialize(new { error = error.Message, code = error.Code }));
    }

    private async Task<string> HandleCreatePlan(ToolCall toolCall, ITracerScope tracerScope, CancellationToken ct)
    {
        Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            toolCall.Function.Arguments, _jsonOptions);

        string userRequest = args!["user_request"].GetString()!;

        ToolContext toolContext = CreateToolContext(tracerScope);

        CreateExecutionPlanTool tool = new() { UserRequest = userRequest };
        CreateExecutionPlanHandler handler = new();

        Result<ExecutionPlan> result = await handler.HandleAsync(tool, toolContext, ct);

        return result.Match(
            plan =>
            {
                State.SetPlan(plan);
                _logger.Info($"Execution plan created: {plan.Steps.Count} steps for tasks: {string.Join(", ", plan.TaskTypes)}");

                return toolContext.Infrastructure.Serialize(new
                {
                    success = true,
                    plan = new
                    {
                        task_types = plan.TaskTypes,
                        reasoning = plan.Reasoning,
                        steps = plan.Steps.Select(s => new
                        {
                            order = s.Order,
                            context = s.TargetContext,
                            question = s.Question,
                            files = s.SuggestedFiles,
                            purpose = s.Purpose,
                            contributes_to = s.ContributesTo
                        }),
                        expected_outputs = plan.ExpectedOutputs.Select(o => new
                        {
                            task_type = o.TaskType,
                            description = o.Description,
                            output_type = o.Type.ToString().ToLowerInvariant()
                        })
                    },
                    message = "Plan created successfully. Now execute each step IN ORDER using query_context with the specified context, question, and files."
                });
            },
            error => toolContext.Infrastructure.Serialize(new { error = error.Message, code = error.Code }));
    }

    private ToolContext CreateToolContext(ITracerScope tracerScope)
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