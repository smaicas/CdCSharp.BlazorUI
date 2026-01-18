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
        You are an intelligent orchestrator for a C# project analysis and modification system.
        
        Your role is to:
        1. Understand what the user wants to accomplish
        2. Gather necessary information by querying specialized contexts
        3. Propose or execute changes to the codebase
        4. Provide clear, helpful responses
        
        Available contexts:
        - CodeExplorer: Explains specific code, traces functionality
        - ArchitectureAnalyzer: Analyzes project structure and patterns
        - DependencyAnalyzer: Maps dependencies between components
        - You can also create dynamic contexts for specific tasks
        
        When modifying code:
        - Creating new files: Applied immediately
        - Modifying existing files: Requires user confirmation
        - Always explain what changes you're proposing and why
        
        Be concise but thorough. Ask clarifying questions if the request is ambiguous.
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

public sealed class ContextInfoResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("answer")]
    public string Answer { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("files_examined")]
    public List<string> FilesExamined { get; init; } = [];

    [System.Text.Json.Serialization.JsonPropertyName("confidence")]
    public float Confidence { get; init; }
}