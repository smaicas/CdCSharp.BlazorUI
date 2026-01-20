using CdCSharp.Theon.AI;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Context.Tools;
using CdCSharp.Theon.Infrastructure;
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
    private readonly IContextFactory _contextFactory;
    private readonly SharedProjectKnowledge _sharedKnowledge;
    private readonly ContextRegistry _registry;
    private readonly ToolCallValidator _toolValidator;
    private readonly JsonSerializerOptions _jsonOptions;
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
        IContextFactory contextFactory,
        SharedProjectKnowledge sharedKnowledge,
        ContextRegistry registry,
        int cloneDepth = 0)
    {
        _config = config;
        _aiClient = aiClient;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _contextFactory = contextFactory;
        _sharedKnowledge = sharedKnowledge;
        _registry = registry;
        _cloneDepth = cloneDepth;
        _toolValidator = new ToolCallValidator(sharedKnowledge, fileSystem);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        _registry.RegisterContext(config.Name, config.ContextType, config.Speciality, config.MaxTokenBudget);
    }

    public Task<TResponse> AskAsync<TResponse>(ContextQuery query, CancellationToken ct = default) where TResponse : class, new()
        => AskAsync<TResponse>(query, null, ct);

    internal async Task<TResponse> AskAsync<TResponse>(ContextQuery query, ITracerScope? tracerScope, CancellationToken ct = default) where TResponse : class, new()
    {
        if (!IsStateful)
            State.Clear();

        await LoadInitialScope(query, tracerScope, ct);
        State.AddMessage(new Message { Role = "user", Content = query.Question });

        return await ExecuteWithToolLoop<TResponse>(tracerScope, ct);
    }

    public Task<TResponse> ContinueAsync<TResponse>(string followUpQuestion, CancellationToken ct = default) where TResponse : class, new()
        => ContinueAsync<TResponse>(followUpQuestion, null, ct);

    internal async Task<TResponse> ContinueAsync<TResponse>(string followUpQuestion, ITracerScope? tracerScope, CancellationToken ct = default) where TResponse : class, new()
    {
        if (!IsStateful)
            throw new InvalidOperationException("Cannot continue a stateless context.");

        State.AddMessage(new Message { Role = "user", Content = followUpQuestion });
        return await ExecuteWithToolLoop<TResponse>(tracerScope, ct);
    }

    public void Reset()
    {
        State.Clear();
        _registry.UpdateBudget(_config.Name, 0);
    }

    private async Task LoadInitialScope(ContextQuery query, ITracerScope? tracerScope, CancellationToken ct)
    {
        if (query.InitialFiles == null) return;

        foreach (string file in query.InitialFiles)
        {
            if (string.IsNullOrWhiteSpace(file)) continue;
            await LoadFileContent(file, tracerScope, ct);
        }
    }

    private async Task<TResponse> ExecuteWithToolLoop<TResponse>(ITracerScope? tracerScope, CancellationToken ct) where TResponse : class, new()
    {
        int maxIterations = 15;

        for (int i = 0; i < maxIterations; i++)
        {
            ct.ThrowIfCancellationRequested();
            _registry.UpdateBudget(_config.Name, State.EstimatedTokens);

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

                if (!State.HasCapacityFor(0, _config.MaxTokenBudget))
                {
                    _logger.Warning($"[{Name}] Token budget limit reached");
                    break;
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
            Tools = ContextTools.GetTools(_config),
            Temperature = 0.3
        };
    }

    private string BuildSystemPrompt()
    {
        string fileIndex = _sharedKnowledge.GetFileIndex();
        string contextsOverview = _registry.GetContextsOverview(_config.Name);
        string loadedFiles = FormatLoadedFiles();

        int budgetUsed = State.EstimatedTokens;
        int budgetMax = _config.MaxTokenBudget;
        int budgetPercent = budgetMax > 0 ? (budgetUsed * 100 / budgetMax) : 0;

        return $"""
            {_config.SystemPrompt}

            {fileIndex}

            {contextsOverview}

            ## Your Status
            Context: {_config.Name} ({_config.ContextType})
            Budget: {budgetUsed:N0} / {budgetMax:N0} tokens ({budgetPercent}% used)
            Clone Depth: {_cloneDepth} / {_config.MaxCloneDepth}

            ## Files in YOUR Context
            {(State.LoadedFiles.Count > 0 ? loadedFiles : "No files loaded yet.")}
            """;
    }

    private string FormatLoadedFiles()
    {
        List<string> sections = [];
        foreach (KeyValuePair<string, string> kvp in State.FileContents)
        {
            sections.Add($"### {kvp.Key}\n```csharp\n{kvp.Value}\n```");
        }
        return string.Join("\n\n", sections);
    }

    private async Task<string> ExecuteTool(ToolCall toolCall, ITracerScope? tracerScope, CancellationToken ct)
    {
        _logger.Debug($"[{Name}] Tool: {toolCall.Function.Name}");

        try
        {
            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments, _jsonOptions);

            return toolCall.Function.Name switch
            {
                "read_file" => await ExecuteReadFile(args, tracerScope, ct),
                "peek_file" => await ExecutePeekFile(args, ct),
                "search_files" => ExecuteSearchFiles(args),
                "spawn_clone" => await ExecuteSpawnClone(args, tracerScope, ct),
                "delegate_to_context" => await ExecuteDelegateToContext(args, tracerScope, ct),
                _ => Serialize(new { error = $"Unknown tool: {toolCall.Function.Name}" })
            };
        }
        catch (Exception ex)
        {
            _logger.Warning($"[{Name}] Tool failed: {ex.Message}");
            return Serialize(new { error = ex.Message });
        }
    }

    private async Task<string> ExecuteReadFile(Dictionary<string, JsonElement>? args, ITracerScope? tracerScope, CancellationToken ct)
    {
        string path = args?["path"].GetString() ?? throw new ArgumentException("path required");

        ToolCallValidation validation = _toolValidator.Validate("read_file", args);
        if (!validation.IsValid)
            return Serialize(new { error = validation.ErrorMessage, suggestion = validation.Suggestion, available_files = validation.AvailableOptions?.Take(10) });

        if (State.FileContents.TryGetValue(path, out string? cached))
            return Serialize(new { path, content = cached, source = "already_loaded" });

        int tokens = EstimateFileTokens(path);
        if (!State.HasCapacityFor(tokens, _config.MaxTokenBudget))
            return Serialize(new { error = "Budget exceeded", path, budget_remaining = _config.MaxTokenBudget - State.EstimatedTokens, file_tokens = tokens, suggestion = "Use peek_file or spawn_clone" });

        string? content = await _fileSystem.ReadFileAsync(path, ct);
        if (content == null)
            return Serialize(new { error = $"File not found: {path}", similar_files = _sharedKnowledge.FindSimilarFiles(path, 5) });

        State.AddFileContent(path, content);
        _registry.RegisterLoadedFile(_config.Name, path, content);
        tracerScope?.RecordFileLoaded(path, content.Length, tokens);

        return Serialize(new { path, content, tokens, permanent = true });
    }

    private async Task<string> ExecutePeekFile(Dictionary<string, JsonElement>? args, CancellationToken ct)
    {
        string path = args?["path"].GetString() ?? throw new ArgumentException("path required");
        string? sourceContext = args.TryGetValue("source_context", out JsonElement src) ? src.GetString() : null;

        string? content = _registry.PeekFile(path, sourceContext);
        string source = "registry";

        if (content == null)
        {
            content = await _fileSystem.ReadFileAsync(path, ct);
            source = "disk";
        }
        else
        {
            source = $"context:{_registry.FindFileOwner(path)}";
        }

        if (content == null)
            return Serialize(new { error = $"File not found: {path}", similar_files = _sharedKnowledge.FindSimilarFiles(path, 5) });

        return Serialize(new { path, content, source, ephemeral = true, note = "NOT added to your context" });
    }

    private string ExecuteSearchFiles(Dictionary<string, JsonElement>? args)
    {
        string pattern = args?["pattern"].GetString() ?? throw new ArgumentException("pattern required");

        ToolCallValidation validation = _toolValidator.Validate("search_files", args);
        if (!validation.IsValid)
            return Serialize(new { error = validation.ErrorMessage, suggestion = validation.Suggestion });

        List<string> files = _sharedKnowledge.FindFilesByPattern(pattern).ToList();
        IReadOnlyDictionary<string, IReadOnlyList<string>> loadedByContexts = _registry.GetAllLoadedFiles();
        List<string> alreadyLoaded = files.Where(f => loadedByContexts.Values.Any(list => list.Contains(f))).ToList();

        return Serialize(new { pattern, files, count = files.Count, already_loaded_elsewhere = alreadyLoaded.Count > 0 ? alreadyLoaded : null });
    }

    private async Task<string> ExecuteSpawnClone(Dictionary<string, JsonElement>? args, ITracerScope? tracerScope, CancellationToken ct)
    {
        string question = args?["question"].GetString() ?? throw new ArgumentException("question required");
        string filesArg = args["files"].GetString() ?? throw new ArgumentException("files required");
        string? purpose = args.TryGetValue("purpose", out JsonElement p) ? p.GetString() : null;

        if (_cloneDepth >= _config.MaxCloneDepth)
            return Serialize(new { error = $"Max clone depth ({_config.MaxCloneDepth}) reached" });

        if (_registry.GetCloneCount(_config.ContextType) >= _config.MaxClonesPerType)
            return Serialize(new { error = $"Max clones ({_config.MaxClonesPerType}) for {_config.ContextType} reached" });

        List<string> fileList = filesArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(f => !string.IsNullOrWhiteSpace(f)).ToList();

        if (fileList.Count == 0)
            return Serialize(new { error = "No files specified" });

        string cloneName = _registry.GenerateCloneName(_config.ContextType);
        ContextConfiguration cloneConfig = _config with { Name = cloneName, IsStateful = true };

        Context clone = new(cloneConfig, _aiClient, _projectContext, _fileSystem, _logger, _tracer,
            _contextFactory, _sharedKnowledge, _registry, _cloneDepth + 1);

        _logger.Debug($"[{Name}] Spawned clone '{cloneName}'");

        using ITracerScope cloneScope = _tracer.BeginContext(cloneName, question, fileList);
        ContextInfoResponse result = await clone.AskAsync<ContextInfoResponse>(ContextQuery.WithFiles(question, fileList.ToArray()), cloneScope, ct);
        tracerScope?.RecordContextQuery(cloneScope.GetContextTrace());

        return Serialize(new { clone_name = cloneName, question, answer = result.Answer, files_examined = result.FilesExamined, confidence = result.Confidence });
    }

    private async Task<string> ExecuteDelegateToContext(Dictionary<string, JsonElement>? args, ITracerScope? tracerScope, CancellationToken ct)
    {
        string targetName = args?["target_context"].GetString() ?? throw new ArgumentException("target_context required");
        string question = args["question"].GetString() ?? throw new ArgumentException("question required");
        string? filesArg = args.TryGetValue("relevant_files", out JsonElement f) ? f.GetString() : null;

        if (targetName.StartsWith(_config.ContextType))
            return Serialize(new { error = "Use spawn_clone for same context type", your_type = _config.ContextType });

        if (!State.CanDelegateTo(targetName, question))
        {
            if (State.DelegationDepth >= _config.MaxDelegationDepth)
                return Serialize(new { error = $"Max delegation depth ({_config.MaxDelegationDepth}) reached" });
            if (State.DelegationChain.Contains(targetName))
                return Serialize(new { error = "Circular delegation", chain = string.Join(" -> ", State.DelegationChain.Reverse().Append(targetName)) });
            return Serialize(new { error = "Already asked this context", suggestion = "Use read_file or peek_file" });
        }

        ContextMetadata? targetMeta = _registry.GetContext(targetName);
        if (targetMeta == null)
            return Serialize(new { error = $"Context '{targetName}' not found", available = _registry.GetAllContexts().Select(c => c.Name) });

        if (!Enum.TryParse<PredefinedContext>(targetMeta.ContextType, out PredefinedContext predefined))
            return Serialize(new { error = $"Unknown context type: {targetMeta.ContextType}" });

        try
        {
            State.IncrementDelegationDepth(targetName);
            State.RecordDelegation(targetName, question);

            IContext target = _contextFactory.GetPredefined(predefined);
            List<string>? fileList = filesArg?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(file => !string.IsNullOrWhiteSpace(file)).ToList();

            ContextQuery query = fileList?.Count > 0 ? ContextQuery.WithFiles(question, fileList.ToArray()) : ContextQuery.Simple(question);

            using ITracerScope scope = _tracer.BeginContext(targetName, question, fileList);
            ContextInfoResponse result = await ((Context)target).AskAsync<ContextInfoResponse>(query, scope, ct);
            tracerScope?.RecordContextQuery(scope.GetContextTrace());

            return Serialize(new { delegated_to = targetName, answer = result.Answer, files_examined = result.FilesExamined, confidence = result.Confidence });
        }
        finally
        {
            State.DecrementDelegationDepth();
        }
    }

    private async Task LoadFileContent(string path, ITracerScope? tracerScope, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(path) || State.FileContents.ContainsKey(path)) return;

        string? content = await _fileSystem.ReadFileAsync(path, ct);
        if (content == null) return;

        int tokens = EstimateFileTokens(path);
        State.AddFileContent(path, content);
        _registry.RegisterLoadedFile(_config.Name, path, content);
        tracerScope?.RecordFileLoaded(path, content.Length, tokens);
    }

    private int EstimateFileTokens(string path) => _projectContext.GetFileTokens(path);

    private string Serialize(object obj) => JsonSerializer.Serialize(obj, _jsonOptions);
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