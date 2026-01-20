using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
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
    private readonly SharedProjectKnowledge _sharedKnowledge;
    private readonly TheonOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrchestratorState State { get; } = new();

    private const string SystemPrompt = """
        You are an orchestrator that coordinates specialized contexts to analyze C# codebases.

        ## Your Role
        You are a COORDINATOR, not a code analyst. You delegate technical questions to specialists.

        ## Available Contexts
        - **CodeExplorer**: Code implementation details, patterns, algorithms
        - **ArchitectureAnalyzer**: Project structure, layers, design decisions
        - **DependencyAnalyzer**: Dependencies, DI configuration, coupling

        ## How to Work
        1. Identify what expertise the user needs
        2. Use query_context to ask the appropriate specialist
        3. Specialists will read files and provide analysis
        4. Synthesize their responses for the user
        5. Use generate_output_file to create documentation when requested

        ## Important Rules
        - ALWAYS use query_context for technical questions - do NOT answer them yourself
        - When asking a context, be specific about what you need
        - If you need file analysis, tell the context which files to examine via the 'files' parameter
        - For documentation requests, gather info from contexts FIRST, then generate the file

        ## Example Flow for Documentation
        1. query_context(ArchitectureAnalyzer, "Describe the project architecture and layers")
        2. query_context(CodeExplorer, "What are the main components and patterns used?")
        3. query_context(DependencyAnalyzer, "What external dependencies are used?")
        4. Synthesize the responses
        5. generate_output_file(folder, filename, synthesized_content)
        """;

    public Orchestrator(
        IAIClient aiClient,
        IContextFactory contextFactory,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer,
        SharedProjectKnowledge sharedKnowledge,
        IOptions<TheonOptions> options)
    {
        _aiClient = aiClient;
        _contextFactory = contextFactory;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _sharedKnowledge = sharedKnowledge;
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
        InitializePredefinedContexts();
    }

    private void InitializePredefinedContexts()
    {
        State.RegisterContext("CodeExplorer", _contextFactory.GetPredefined(PredefinedContext.CodeExplorer));
        State.RegisterContext("ArchitectureAnalyzer", _contextFactory.GetPredefined(PredefinedContext.ArchitectureAnalyzer));
        State.RegisterContext("DependencyAnalyzer", _contextFactory.GetPredefined(PredefinedContext.DependencyAnalyzer));
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
                    ToolExecutionResult result = await ExecuteTool(toolCall, tracerScope, ct);
                    toolSw.Stop();

                    tracerScope.RecordToolExecution(toolCall, result.Response, toolSw.Elapsed, result.Response.Contains("\"error\""));

                    State.AddToolResult(toolCall.Id, result.Response);

                    if (result.CreatedFile != null) createdFiles.Add(result.CreatedFile);
                    if (result.GeneratedOutput != null) generatedOutputs.Add(result.GeneratedOutput);
                    if (result.ProposedChange != null) proposedChanges.Add(result.ProposedChange);
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

        string fileIndex = _sharedKnowledge.GetFileIndex();
        string contextsOverview = _registry.GetContextsOverview();
        string pendingChanges = FormatPendingChanges();

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
            Tools = OrchestratorTools.All,
            Temperature = 0.5
        };
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
                toolCall.Function.Arguments, _jsonOptions);

            return toolCall.Function.Name switch
            {
                "query_context" => await ExecuteQueryContext(args, tracerScope, ct),
                "create_dynamic_context" => ExecuteCreateDynamicContext(args),
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
            return ToolExecutionResult.Error($"Context '{contextName}' not found");

        List<string>? files = filesArg?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(file => !string.IsNullOrWhiteSpace(file))
            .ToList();

        ContextQuery query = files?.Count > 0
            ? ContextQuery.WithFiles(question, files.ToArray())
            : ContextQuery.Simple(question);

        using ITracerScope contextScope = _tracer.BeginContext(contextName, question, files);

        _logger.Debug($"Querying context '{contextName}': {question[..Math.Min(50, question.Length)]}...");

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
            return ToolExecutionResult.Error($"Context '{name}' already exists");

        IContext context = _contextFactory.CreateDynamic(name, purpose, stateful);
        State.RegisterContext(name, context);

        return ToolExecutionResult.Success(JsonSerializer.Serialize(new { created = true, name, purpose, stateful }, _jsonOptions));
    }

    private async Task<ToolExecutionResult> ExecuteProposeFileChange(Dictionary<string, JsonElement>? args, CancellationToken ct)
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
            JsonSerializer.Serialize(new { proposed = true, id = change.Id, path, description, requiresConfirmation = true }, _jsonOptions),
            change);
    }

    private async Task<ToolExecutionResult> ExecuteCreateProjectFile(Dictionary<string, JsonElement>? args, CancellationToken ct)
    {
        string path = args?["path"].GetString() ?? throw new ArgumentException("path required");
        string content = args["content"].GetString() ?? throw new ArgumentException("content required");

        if (!_options.Modification.Enabled)
            return ToolExecutionResult.Error("Project modification is disabled");

        string? existing = await _fileSystem.ReadFileAsync(path, ct);
        if (existing != null)
            return ToolExecutionResult.Error($"File already exists: {path}. Use propose_file_change to modify.");

        bool success = await _fileSystem.WriteProjectFileAsync(path, content, ct);
        if (!success)
            return ToolExecutionResult.Error($"Failed to create file: {path}");

        return ToolExecutionResult.WithCreatedFile(
            JsonSerializer.Serialize(new { created = true, path }, _jsonOptions), path);
    }

    private async Task<ToolExecutionResult> ExecuteGenerateOutputFile(Dictionary<string, JsonElement>? args, CancellationToken ct)
    {
        string folder = args?["folder"].GetString() ?? throw new ArgumentException("folder required");
        string filename = args["filename"].GetString() ?? throw new ArgumentException("filename required");
        string content = args["content"].GetString() ?? throw new ArgumentException("content required");

        await _fileSystem.WriteOutputFileAsync(folder, filename, content, ct);

        string outputPath = Path.Combine(_options.ResponsesPath, folder, filename);

        return ToolExecutionResult.WithGeneratedOutput(
            JsonSerializer.Serialize(new { generated = true, folder, filename, fullPath = outputPath }, _jsonOptions),
            outputPath);
    }

    private async Task<ToolExecutionResult> ExecuteApplyPendingChanges(Dictionary<string, JsonElement>? args, CancellationToken ct)
    {
        string changeIds = args?["change_ids"].GetString() ?? "all";
        OrchestratorResponse result = await ConfirmChangesAsync(true, changeIds, ct);

        return ToolExecutionResult.Success(JsonSerializer.Serialize(new { applied = result.ModifiedFiles, count = result.ModifiedFiles.Count }, _jsonOptions));
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