using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestrator.Models;
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
    private readonly IContextFactory _contextFactory;
    private readonly IProjectContext _projectContext;
    private readonly IFileSystem _fileSystem;
    private readonly ITheonLogger _logger;
    private readonly ITracer _tracer;
    private readonly TheonOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrchestratorState State { get; } = new();

    private const string SystemPrompt = """
        You are an intelligent orchestration engine for a C# codebase analysis and modification system.
        You coordinate specialized contexts to answer questions, perform analyses, and propose changes.
        
        ## Your Role
        You are the central coordinator that decides which specialized contexts to consult and how to synthesize their insights.
        Think of yourself as a project manager delegating to domain experts, then combining their reports into coherent answers.
        
        ## Available Specialized Contexts
        
        **CodeExplorer** (Stateful - maintains conversation history)
        - Expert in implementation details, algorithms, and code patterns
        - Can trace execution flow and explain how code works
        - Best for: "How does X work?", "Explain this method", "Trace the flow from A to B"
        - Can delegate to other contexts for architectural or dependency questions
        
        **ArchitectureAnalyzer** (Stateless - fresh perspective each time)
        - Expert in system structure, layers, and design patterns
        - Evaluates architectural styles and identifies violations
        - Best for: "What's the architecture?", "Evaluate the design", "Find architectural violations"
        - Can delegate to CodeExplorer for implementation details or DependencyAnalyzer for relationships
        
        **DependencyAnalyzer** (Stateless)
        - Expert in type relationships, dependency chains, and coupling
        - Detects circular dependencies and evaluates DI configurations
        - Best for: "What depends on X?", "Find circular dependencies", "Map all implementations of IFoo"
        - Can delegate to CodeExplorer for why dependencies exist or ArchitectureAnalyzer for structural impact
        
        **Custom Contexts**
        - You can create specialized contexts for specific tasks not covered above
        - Use create_dynamic_context when you need a unique perspective
        
        ## Intelligent Context Selection Strategy
        
        When the user asks a question, think through:
        
        1. **Question Classification**
           - Implementation-focused? → CodeExplorer
           - Structure-focused? → ArchitectureAnalyzer
           - Relationship-focused? → DependencyAnalyzer
           - Multi-faceted? → Multiple contexts in sequence or parallel
        
        2. **Context Collaboration**
           - Simple question: Use one context directly
           - Complex question: Query multiple contexts, let them delegate to each other
           - The contexts are smart enough to delegate when they need expertise from another domain
        
        3. **Response Synthesis**
           - Combine insights from multiple contexts coherently
           - Resolve conflicts by explaining trade-offs and different perspectives
           - Provide a unified, actionable answer
           - Don't just concatenate context responses - synthesize them meaningfully
        
        ## Examples of Good Context Usage
        
        User: "How does the Orchestrator handle tool execution?"
        Strategy: Query CodeExplorer to examine ExecuteWithToolLoop and ExecuteTool methods.
        
        User: "What's the overall architecture of this system?"
        Strategy: Query ArchitectureAnalyzer to evaluate structure and layers.
        
        User: "Are there circular dependencies in the Domain layer?"
        Strategy: Query DependencyAnalyzer to trace dependencies within Domain assembly.
        
        User: "Explain the dependency injection setup and how it relates to the architecture"
        Strategy: Query ArchitectureAnalyzer for DI structure, then DependencyAnalyzer for specific registrations.
        Or let ArchitectureAnalyzer delegate to DependencyAnalyzer automatically.
        
        ## Code Modification Workflow
        
        When proposing changes to the codebase:
        
        1. **Understand Current State**
           - Use contexts to deeply analyze existing code
           - Identify exactly what needs to change and why
           - Verify your understanding before proposing modifications
        
        2. **Propose Changes Clearly**
           - For NEW files: Use `create_project_file` (applied immediately if modification enabled)
           - For MODIFYING existing files: Use `propose_file_change` (always requires user confirmation)
           - Always explain the reasoning: what problem does this solve? what are the trade-offs?
        
        3. **Validate Proposals**
           - Consider architectural impact (does this violate layers?)
           - Check for dependency violations (does this create circular dependencies?)
           - Ensure consistency with existing patterns in the codebase
           - Think about testability and maintainability
        
        ## C# and .NET Domain Knowledge
        
        You understand:
        - .NET project structure (.csproj files define assemblies)
        - Namespace organization and file layout conventions
        - Common patterns: Dependency Injection, Repository, CQRS, MediatR
        - Modern C# features: records, pattern matching, nullable reference types, async/await
        - NuGet package dependencies and versioning
        - Testing frameworks (xUnit, NUnit, MSTest)
        
        ## Output Guidelines
        
        - Be conversational but technically precise
        - Avoid overwhelming the user with unnecessary details
        - When uncertain, ask clarifying questions before diving deep
        - Provide confidence levels when appropriate (e.g., "I'm quite confident this is Clean Architecture")
        - Reference specific files, line numbers, and code snippets when relevant
        - If a context provides low confidence, acknowledge uncertainty
        
        ## Tools You Can Use
        
        **Context Management:**
        - `query_context`: Ask a specialized context a focused question
        - `create_dynamic_context`: Create a custom context for unique needs
        - `list_contexts`: See all active contexts and their state
        
        **File Operations:**
        - `propose_file_change`: Propose modification to existing file (needs confirmation)
        - `create_project_file`: Create new file in project (applied immediately if enabled)
        - `generate_output_file`: Create documentation/reports in output folder (always allowed)
        
        **Change Management:**
        - `apply_pending_changes`: Apply changes that user has confirmed
        
        ## Important Principles
        
        - Trust your specialized contexts - they have deep expertise in their domains
        - Let contexts collaborate through delegation - they're smart enough to ask each other
        - Synthesize insights rather than just passing through raw context responses
        - Be honest about limitations and uncertainty
        - Prioritize correctness and maintainability over quick answers
        - Think critically about architectural impact before proposing changes
        
        Remember: You orchestrate a team of experts. Delegate wisely, synthesize thoughtfully, and always prioritize clarity and technical correctness.
        """;

    public Orchestrator(
        IAIClient aiClient,
        IContextFactory contextFactory,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer,
        IOptions<TheonOptions> options)
    {
        _aiClient = aiClient;
        _contextFactory = contextFactory;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        InitializePredefinedContexts();
    }

    public async Task<OrchestratorResponse> ProcessAsync(string userInput, CancellationToken ct = default)
    {
        _logger.Debug($"Processing user input: {userInput[..Math.Min(50, userInput.Length)]}...");

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
            tracerScope.SetResult(new ExecutionResult
            {
                Success = false,
                Error = ex.Message
            });
            throw;
        }
    }

    public async Task<OrchestratorResponse> ConfirmChangesAsync(
        bool confirm,
        string? changeIds = null,
        CancellationToken ct = default)
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
        InitializePredefinedContexts();
    }

    private void InitializePredefinedContexts()
    {
        State.RegisterContext("CodeExplorer", _contextFactory.GetPredefined(PredefinedContext.CodeExplorer));
        State.RegisterContext("ArchitectureAnalyzer", _contextFactory.GetPredefined(PredefinedContext.ArchitectureAnalyzer));
        State.RegisterContext("DependencyAnalyzer", _contextFactory.GetPredefined(PredefinedContext.DependencyAnalyzer));
    }

    // Continuación de Orchestrator.cs

    private async Task<OrchestratorResponse> ExecuteWithToolLoop(ITracerScope tracerScope, CancellationToken ct)
    {
        List<string> createdFiles = [];
        List<string> generatedOutputs = [];
        List<ProposedChange> proposedChanges = [];

        while (true)
        {
            ct.ThrowIfCancellationRequested();

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
                    ToolExecutionResult result = await ExecuteTool(toolCall, tracerScope, ct);
                    toolSw.Stop();

                    tracerScope.RecordToolExecution(toolCall, result.Response, toolSw.Elapsed, result.Response.Contains("\"error\""));

                    State.AddToolResult(toolCall.Id, result.Response);

                    if (result.CreatedFile != null)
                        createdFiles.Add(result.CreatedFile);

                    if (result.GeneratedOutput != null)
                        generatedOutputs.Add(result.GeneratedOutput);

                    if (result.ProposedChange != null)
                        proposedChanges.Add(result.ProposedChange);
                }

                continue;
            }

            State.AddAssistantMessage(choice.Message);

            bool needsConfirmation = proposedChanges.Any(c => c.Status == ChangeStatus.Pending);

            return new OrchestratorResponse
            {
                Message = choice.Message.Content ?? string.Empty,
                ProposedChanges = proposedChanges,
                CreatedFiles = createdFiles,
                GeneratedOutputs = generatedOutputs,
                ModifiedFiles = [],
                Confidence = 0.9f,
                NeedsConfirmation = needsConfirmation
            };
        }
    }

    private async Task<ChatCompletionRequest> BuildRequest()
    {
        ProjectInfo project = await _projectContext.GetProjectAsync();
        string projectStructure = FormatProjectStructure(project);
        string pendingChanges = FormatPendingChanges();

        string fullSystemPrompt = $"""
            {SystemPrompt}
            
            ## Project Structure
            {projectStructure}
            
            ## Active Contexts
            {string.Join(", ", State.ActiveContexts.Keys)}

            {(string.IsNullOrEmpty(pendingChanges) ? "" : $"## Pending Changes\n{pendingChanges}")}
            """;

        List<Message> messages =
        [
            new() { Role = "system", Content = fullSystemPrompt },
            .. State.ConversationHistory
        ];

        return new ChatCompletionRequest
        {
            Model = "default",
            Messages = messages,
            Tools = OrchestratorTools.All,
            Temperature = 0.5
        };
    }

    private string FormatProjectStructure(ProjectInfo project)
    {
        List<string> lines = [];

        foreach (AssemblyInfo assembly in project.Assemblies.Where(a => !a.IsTestProject))
        {
            lines.Add($"- {assembly.Name} ({assembly.Files.Count} files, {assembly.Types.Count} types)");
        }

        return string.Join("\n", lines);
    }

    private string FormatPendingChanges()
    {
        IEnumerable<ProposedChange> pending = State.GetPendingChanges();
        if (!pending.Any()) return string.Empty;

        return string.Join("\n", pending.Select(c => $"- [{c.Id}] {c.Path}: {c.Description}"));
    }

    private async Task<ToolExecutionResult> ExecuteTool(ToolCall toolCall, ITracerScope tracerScope, CancellationToken ct)
    {
        _logger.Debug($"Executing tool: {toolCall.Function.Name}");

        try
        {
            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments,
                _jsonOptions);

            return toolCall.Function.Name switch
            {
                "query_context" => await ExecuteQueryContext(args, tracerScope, ct),
                "create_dynamic_context" => ExecuteCreateDynamicContext(args),
                "list_contexts" => ExecuteListContexts(),
                "propose_file_change" => await ExecuteProposeFileChange(args, ct),
                "create_project_file" => await ExecuteCreateProjectFile(args, ct),
                "generate_output_file" => await ExecuteGenerateOutputFile(args, ct),
                "apply_pending_changes" => await ExecuteApplyPendingChanges(args, ct),
                _ => ToolExecutionResult.Error($"Unknown tool: {toolCall.Function.Name}")
            };
        }
        catch (Exception ex)
        {
            _logger.Warning($"Tool execution failed: {ex.Message}");
            return ToolExecutionResult.Error(ex.Message);
        }
    }

    private async Task<ToolExecutionResult> ExecuteQueryContext(
        Dictionary<string, JsonElement>? args,
        ITracerScope orchestratorScope,
        CancellationToken ct)
    {
        string contextName = args?["context_name"].GetString() ?? throw new ArgumentException("context_name required");
        string question = args["question"].GetString() ?? throw new ArgumentException("question required");
        string? filesArg = args.TryGetValue("files", out JsonElement f) ? f.GetString() : null;

        IContext? context = State.GetContext(contextName);
        if (context == null)
        {
            return ToolExecutionResult.Error($"Context '{contextName}' not found");
        }

        List<string>? files = filesArg?.Split(',').Select(file => file.Trim()).ToList();

        ContextQuery query = files != null
            ? ContextQuery.WithFiles(question, files.ToArray())
            : ContextQuery.Simple(question);

        using ITracerScope contextScope = _tracer.BeginContext(contextName, question, files);

        ContextInfoResponse result = await ((Context.Context)context).AskAsync<ContextInfoResponse>(query, contextScope, ct);

        ContextTrace contextTrace = contextScope.GetContextTrace();
        orchestratorScope.RecordContextQuery(contextTrace);

        ContextQueryResult queryResult = new()
        {
            ContextName = contextName,
            Question = question,
            Answer = result.Answer,
            FilesExamined = result.FilesExamined
        };

        State.AddContextQueryResult(queryResult);

        return ToolExecutionResult.Success(JsonSerializer.Serialize(queryResult, _jsonOptions));
    }

    private ToolExecutionResult ExecuteCreateDynamicContext(Dictionary<string, JsonElement>? args)
    {
        string name = args?["name"].GetString() ?? throw new ArgumentException("name required");
        string purpose = args["purpose"].GetString() ?? throw new ArgumentException("purpose required");
        bool stateful = args.TryGetValue("stateful", out JsonElement s) && s.GetString() == "true";

        if (State.GetContext(name) != null)
        {
            return ToolExecutionResult.Error($"Context '{name}' already exists");
        }

        IContext context = _contextFactory.CreateDynamic(name, purpose, stateful);
        State.RegisterContext(name, context);

        return ToolExecutionResult.Success(JsonSerializer.Serialize(new
        {
            created = true,
            name,
            purpose,
            stateful
        }, _jsonOptions));
    }

    private ToolExecutionResult ExecuteListContexts()
    {
        var contexts = State.ActiveContexts.Select(kvp => new
        {
            name = kvp.Key,
            stateful = kvp.Value.IsStateful,
            filesLoaded = kvp.Value.State.LoadedFiles.Count
        });

        return ToolExecutionResult.Success(JsonSerializer.Serialize(new { contexts }, _jsonOptions));
    }

    // Continuación y finalización de Orchestrator.cs

    private async Task<ToolExecutionResult> ExecuteProposeFileChange(
        Dictionary<string, JsonElement>? args,
        CancellationToken ct)
    {
        string path = args?["path"].GetString() ?? throw new ArgumentException("path required");
        string description = args["description"].GetString() ?? throw new ArgumentException("description required");
        string newContent = args["new_content"].GetString() ?? throw new ArgumentException("new_content required");

        string? originalContent = await _fileSystem.ReadFileAsync(path, ct);

        ProposedChange change = new()
        {
            Path = path,
            Description = description,
            ChangeType = originalContent == null ? ChangeType.Create : ChangeType.Modify,
            OriginalContent = originalContent,
            NewContent = newContent,
            Status = ChangeStatus.Pending
        };

        State.ProposeChange(change);

        return ToolExecutionResult.WithProposedChange(
            JsonSerializer.Serialize(new
            {
                proposed = true,
                id = change.Id,
                path,
                description,
                requiresConfirmation = true
            }, _jsonOptions),
            change);
    }

    private async Task<ToolExecutionResult> ExecuteCreateProjectFile(
        Dictionary<string, JsonElement>? args,
        CancellationToken ct)
    {
        string path = args?["path"].GetString() ?? throw new ArgumentException("path required");
        string content = args["content"].GetString() ?? throw new ArgumentException("content required");

        if (!_options.Modification.Enabled)
        {
            return ToolExecutionResult.Error(
                "Project modification is disabled. Enable it in configuration to create project files.");
        }

        string? existing = await _fileSystem.ReadFileAsync(path, ct);
        if (existing != null)
        {
            return ToolExecutionResult.Error(
                $"File already exists: {path}. Use propose_file_change to modify existing files.");
        }

        bool success = await _fileSystem.WriteProjectFileAsync(path, content, ct);

        if (!success)
        {
            return ToolExecutionResult.Error($"Failed to create file: {path}");
        }

        return ToolExecutionResult.WithCreatedFile(
            JsonSerializer.Serialize(new { created = true, path, location = "project" }, _jsonOptions),
            path);
    }

    private async Task<ToolExecutionResult> ExecuteGenerateOutputFile(
        Dictionary<string, JsonElement>? args,
        CancellationToken ct)
    {
        string folder = args?["folder"].GetString() ?? throw new ArgumentException("folder required");
        string filename = args["filename"].GetString() ?? throw new ArgumentException("filename required");
        string content = args["content"].GetString() ?? throw new ArgumentException("content required");

        await _fileSystem.WriteOutputFileAsync(folder, filename, content, ct);

        string outputPath = Path.Combine(_options.ResponsesPath, folder, filename);

        return ToolExecutionResult.WithGeneratedOutput(
            JsonSerializer.Serialize(new
            {
                generated = true,
                folder,
                filename,
                location = "output",
                fullPath = outputPath
            }, _jsonOptions),
            outputPath);
    }

    private async Task<ToolExecutionResult> ExecuteApplyPendingChanges(
        Dictionary<string, JsonElement>? args,
        CancellationToken ct)
    {
        string changeIds = args?["change_ids"].GetString() ?? "all";

        OrchestratorResponse result = await ConfirmChangesAsync(true, changeIds, ct);

        return ToolExecutionResult.Success(JsonSerializer.Serialize(new
        {
            applied = result.ModifiedFiles,
            count = result.ModifiedFiles.Count
        }, _jsonOptions));
    }

    private sealed record ToolExecutionResult(
        string Response,
        string? CreatedFile = null,
        string? GeneratedOutput = null,
        ProposedChange? ProposedChange = null)
    {
        public static ToolExecutionResult Success(string response) => new(response);
        public static ToolExecutionResult Error(string message) => new(JsonSerializer.Serialize(new { error = message }));
        public static ToolExecutionResult WithCreatedFile(string response, string path) => new(response, CreatedFile: path);
        public static ToolExecutionResult WithGeneratedOutput(string response, string path) => new(response, GeneratedOutput: path);
        public static ToolExecutionResult WithProposedChange(string response, ProposedChange change) => new(response, ProposedChange: change);
    }
}