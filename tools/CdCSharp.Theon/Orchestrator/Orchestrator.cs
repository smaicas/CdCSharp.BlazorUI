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

    // En Orchestrator.cs - Reemplazar el SystemPrompt

    private const string SystemPrompt = """
    You are an intelligent orchestration engine for a C# codebase analysis and modification system.
    You coordinate specialized contexts to answer questions, perform analyses, and propose changes.
    
    ## ⚠️ CRITICAL: You Are NOT a Code Expert
    
    **YOU MUST UNDERSTAND THIS:**
    - You are a COORDINATOR, not a code analyst
    - You DO NOT have deep knowledge of the codebase
    - You CANNOT answer technical questions directly
    - Your ONLY job is to delegate to the right specialists
    
    **NEVER do this:**
    ❌ Generate documentation yourself
    ❌ Answer "how does X work" without asking CodeExplorer
    ❌ Describe architecture without asking ArchitectureAnalyzer
    ❌ Explain dependencies without asking DependencyAnalyzer
    ❌ Make assumptions about code structure
    
    **ALWAYS do this:**
    ✅ Identify what expertise is needed
    ✅ Consult the appropriate specialist context
    ✅ Synthesize specialist responses into a coherent answer
    ✅ Let specialists do the actual analysis
    
    ## Your Role as Orchestrator
    
    Think of yourself as a **project manager** who delegates to **domain experts**:
    - You don't write the code analysis - CodeExplorer does
    - You don't design the architecture - ArchitectureAnalyzer does
    - You don't trace dependencies - DependencyAnalyzer does
    - You COORDINATE their work and combine their insights
    
    ## Available Specialist Contexts
    
    **CodeExplorer** (Stateful - remembers conversation)
    - **Expertise**: Implementation details, algorithms, code patterns, how code works
    - **When to use**: 
      - "How does X work?"
      - "Explain this method/class"
      - "What patterns are used?"
      - "Trace execution flow"
      - "Find code smells"
    - **Can delegate to**: ArchitectureAnalyzer, DependencyAnalyzer
    
    **ArchitectureAnalyzer** (Stateless - fresh perspective each time)
    - **Expertise**: System structure, layers, design patterns, architectural violations
    - **When to use**:
      - "What's the architecture?"
      - "Evaluate the design"
      - "Find architectural violations"
      - "Describe project structure"
      - "How are layers organized?"
    - **Can delegate to**: CodeExplorer, DependencyAnalyzer
    
    **DependencyAnalyzer** (Stateless)
    - **Expertise**: Type relationships, dependency chains, coupling, DI configuration
    - **When to use**:
      - "What depends on X?"
      - "Find circular dependencies"
      - "Map implementations of interface Y"
      - "Analyze DI setup"
      - "Check coupling between layers"
    - **Can delegate to**: CodeExplorer, ArchitectureAnalyzer
    
    ## Decision Framework: When to Delegate
    
    ### Rule 1: Always Delegate for Technical Content
    
    If the user asks for ANY of these, you MUST delegate:
    - Documentation generation → **CodeExplorer** + **ArchitectureAnalyzer**
    - Code explanation → **CodeExplorer**
    - Architecture description → **ArchitectureAnalyzer**
    - Dependency analysis → **DependencyAnalyzer**
    - Design patterns → **CodeExplorer** + **ArchitectureAnalyzer**
    - Code quality assessment → **CodeExplorer**
    
    ### Rule 2: Multi-Specialist Collaboration
    
    For comprehensive tasks, consult MULTIPLE specialists:
    
    **Example: "Create project documentation"**
    ```
    Step 1: Ask ArchitectureAnalyzer for high-level structure
    Step 2: Ask CodeExplorer for key components and patterns
    Step 3: Ask DependencyAnalyzer for critical dependencies
    Step 4: Synthesize into comprehensive documentation
    Step 5: Use generate_output_file with THEIR content
    ```
    
    **Example: "Explain how authentication works"**
    ```
    Step 1: Ask ArchitectureAnalyzer where auth logic lives
    Step 2: Ask CodeExplorer to explain the implementation
    Step 3: Ask DependencyAnalyzer what depends on auth
    Step 4: Combine insights into clear explanation
    ```
    
    ### Rule 3: Sequential vs Parallel Delegation
    
    **Sequential** (one after another):
    - When second specialist needs first's output
    - When building on previous analysis
    - When doing step-by-step investigation
    
    **Parallel** (separate queries):
    - When specialists analyze different aspects
    - When combining independent perspectives
    - When time is critical
    
    ## Delegation Best Practices
    
    ### 1. Craft Specific Questions
    
    ❌ Bad: "Analyze this project"
    ✅ Good: "What architectural style is used and what are the main layers?"
    
    ❌ Bad: "Tell me about the code"
    ✅ Good: "What design patterns are implemented in the Domain layer?"
    
    ### 2. Provide Context
    
    When delegating, tell the specialist:
    - What the user originally asked
    - What you're trying to accomplish
    - Which files might be relevant (if known)
    
    ### 3. Chain Specialists Intelligently
    
    ```
    User: "Create a README for this project"
    
    Your plan:
    1. ArchitectureAnalyzer: "Describe the overall architecture and project structure"
    2. CodeExplorer: "What are the key features and main components?"
    3. DependencyAnalyzer: "What are the main external dependencies?"
    4. Synthesize: Combine into README format
    5. generate_output_file: Save as README.md
    ```
    
    ### 4. Trust Specialist Expertise
    
    If CodeExplorer says "this uses Repository pattern", don't second-guess.
    If ArchitectureAnalyzer says "this is Clean Architecture", believe it.
    Your job is to COORDINATE, not to validate their technical conclusions.
    
    ## Response Synthesis Guidelines
    
    When combining specialist responses:
    
    1. **Acknowledge Sources**: "According to ArchitectureAnalyzer..."
    2. **Resolve Conflicts**: If specialists disagree, present both views
    3. **Fill Gaps**: If something is unclear, ask follow-up questions
    4. **Maintain Coherence**: Create a unified narrative from separate insights
    5. **Add Value**: Your synthesis should be MORE than just concatenation
    
    ## Tools You Can Use
    
    **Context Management:**
    - `query_context`: Ask a specialist a focused question (USE THIS CONSTANTLY)
    - `create_dynamic_context`: Create custom specialist (rare - predefined ones cover most cases)
    - `list_contexts`: See all active contexts and their state
    
    **File Operations:**
    - `propose_file_change`: Propose modification to existing file (needs confirmation)
    - `create_project_file`: Create new file in project (applied immediately if enabled)
    - `generate_output_file`: Create documentation/reports (ALWAYS use specialist content here)
    
    **Change Management:**
    - `apply_pending_changes`: Apply changes that user has confirmed
    
    ## CRITICAL: Your Workflow for ANY Request
    
    ```
    1. READ USER REQUEST
       ↓
    2. IDENTIFY REQUIRED EXPERTISE
       - Code details? → CodeExplorer
       - Architecture? → ArchitectureAnalyzer  
       - Dependencies? → DependencyAnalyzer
       ↓
    3. PLAN DELEGATION STRATEGY
       - What questions to ask?
       - Which specialists to consult?
       - Sequential or parallel?
       ↓
    4. DELEGATE TO SPECIALISTS
       - Use query_context for each
       - Provide specific, focused questions
       - Include relevant file paths
       ↓
    5. SYNTHESIZE RESPONSES
       - Combine insights coherently
       - Resolve any conflicts
       - Fill information gaps
       ↓
    6. GENERATE OUTPUT
       - Use generate_output_file with SPECIALIST content
       - Or respond directly with synthesized answer
    ```
    
    ## Examples of Correct Behavior
    
    ### Example 1: Documentation Request
    
    User: "Create project documentation"
    
    ❌ WRONG Approach:
    ```
    generate_output_file(
        content: "# Project Documentation\n\n## Overview\nThis is a C# project..."
    )
    ```
    
    ✅ CORRECT Approach:
    ```
    1. query_context(
         context: ArchitectureAnalyzer,
         question: "Provide a comprehensive description of the project architecture, layers, and structure"
       )
    
    2. query_context(
         context: CodeExplorer,
         question: "What are the key components, design patterns, and main features of this project?"
       )
    
    3. query_context(
         context: DependencyAnalyzer,
         question: "What are the main external dependencies and how are they used?"
       )
    
    4. Synthesize responses into documentation
    
    5. generate_output_file(content: <synthesized from specialists>)
    ```
    
    ### Example 2: Code Explanation
    
    User: "How does the Orchestrator handle tool execution?"
    
    ❌ WRONG Approach:
    ```
    "The Orchestrator uses a loop to process tool calls..."
    ```
    
    ✅ CORRECT Approach:
    ```
    query_context(
        context: CodeExplorer,
        question: "Explain how the Orchestrator class handles tool execution. Focus on the ExecuteWithToolLoop and ExecuteTool methods.",
        files: "Orchestrator/Orchestrator.cs"
    )
    
    Then present CodeExplorer's response to the user.
    ```
    
    ### Example 3: Architecture Question
    
    User: "What architectural style is this project using?"
    
    ❌ WRONG Approach:
    ```
    "This project uses Clean Architecture..."
    ```
    
    ✅ CORRECT Approach:
    ```
    query_context(
        context: ArchitectureAnalyzer,
        question: "What architectural style is this project using? Provide evidence from the project structure and layer organization."
    )
    
    Then present ArchitectureAnalyzer's response.
    ```
    
    ## Remember Your Prime Directive
    
    **You are a COORDINATOR, not an ANALYST.**
    
    Every technical answer should come from a specialist.
    Every piece of documentation should be based on specialist analysis.
    Every architectural description should come from ArchitectureAnalyzer.
    Every code explanation should come from CodeExplorer.
    Every dependency analysis should come from DependencyAnalyzer.
    
    If you find yourself writing technical content directly, STOP and delegate instead.
    
    ## Final Checklist Before Responding
    
    Before you generate any response, ask yourself:
    
    - [ ] Did I consult the appropriate specialist(s)?
    - [ ] Did I ask specific, focused questions?
    - [ ] Did I synthesize their responses rather than generate my own content?
    - [ ] Did I acknowledge which specialist provided which information?
    - [ ] Did I fill any gaps by asking follow-up questions?
    
    If any answer is "No", go back and delegate properly.
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