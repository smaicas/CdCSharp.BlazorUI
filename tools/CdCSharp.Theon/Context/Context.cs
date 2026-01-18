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

    Task<TResponse> AskAsync<TResponse>(
        ContextQuery query,
        CancellationToken ct = default) where TResponse : class, new();

    Task<TResponse> ContinueAsync<TResponse>(
        string followUpQuestion,
        CancellationToken ct = default) where TResponse : class, new();

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
    private readonly JsonSerializerOptions _jsonOptions;

    public string Name => _config.Name;
    public bool IsStateful => _config.IsStateful;
    public ContextState State { get; } = new();

    public Context(
        ContextConfiguration config,
        IAIClient aiClient,
        IProjectContext projectContext,
        IFileSystem fileSystem,
        ITheonLogger logger,
        ITracer tracer,
        IContextFactory contextFactory)
    {
        _config = config;
        _aiClient = aiClient;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _contextFactory = contextFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<TResponse> AskAsync<TResponse>(
        ContextQuery query,
        CancellationToken ct = default) where TResponse : class, new()
    {
        return await AskAsync<TResponse>(query, tracerScope: null, ct);
    }

    internal async Task<TResponse> AskAsync<TResponse>(
        ContextQuery query,
        ITracerScope? tracerScope,
        CancellationToken ct = default) where TResponse : class, new()
    {
        if (!IsStateful)
        {
            State.Clear();
        }

        await LoadInitialScope(query, tracerScope, ct);

        State.AddMessage(new Message { Role = "user", Content = query.Question });

        return await ExecuteWithToolLoop<TResponse>(tracerScope, ct);
    }

    public async Task<TResponse> ContinueAsync<TResponse>(
        string followUpQuestion,
        CancellationToken ct = default) where TResponse : class, new()
    {
        return await ContinueAsync<TResponse>(followUpQuestion, tracerScope: null, ct);
    }

    internal async Task<TResponse> ContinueAsync<TResponse>(
        string followUpQuestion,
        ITracerScope? tracerScope,
        CancellationToken ct = default) where TResponse : class, new()
    {
        if (!IsStateful)
        {
            throw new InvalidOperationException("Cannot continue a stateless context. Use AskAsync instead.");
        }

        State.AddMessage(new Message { Role = "user", Content = followUpQuestion });

        return await ExecuteWithToolLoop<TResponse>(tracerScope, ct);
    }

    public void Reset() => State.Clear();

    private async Task LoadInitialScope(ContextQuery query, ITracerScope? tracerScope, CancellationToken ct)
    {
        if (query.InitialFiles != null)
        {
            foreach (string file in query.InitialFiles)
            {
                await LoadFileContent(file, tracerScope, ct);
            }
        }

        if (query.InitialPatterns != null)
        {
            foreach (string pattern in query.InitialPatterns)
            {
                IEnumerable<string> files = _fileSystem.EnumerateFiles(null, pattern);
                foreach (string file in files)
                {
                    if (!State.HasCapacityFor(EstimateFileTokens(file), _config.MaxTokenBudget))
                        break;

                    await LoadFileContent(file, tracerScope, ct);
                }
            }
        }
    }

    private async Task<TResponse> ExecuteWithToolLoop<TResponse>(ITracerScope? tracerScope, CancellationToken ct) where TResponse : class, new()
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            ChatCompletionRequest request = BuildRequest<TResponse>();
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

                    State.AddMessage(new Message
                    {
                        Role = "tool",
                        Content = result,
                        ToolCallId = toolCall.Id
                    });
                }

                if (!State.HasCapacityFor(0, _config.MaxTokenBudget))
                {
                    _logger.Warning($"Context '{Name}' reached token budget limit");
                    break;
                }

                continue;
            }

            State.AddMessage(choice.Message);

            string content = choice.Message.Content ?? "{}";
            TResponse? parsed = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);

            return parsed ?? new TResponse();
        }

        return new TResponse();
    }

    private ChatCompletionRequest BuildRequest<TResponse>() where TResponse : class
    {
        string systemPrompt = BuildSystemPrompt();

        List<Message> messages =
        [
            new() { Role = "system", Content = systemPrompt },
            .. State.History
        ];

        return new ChatCompletionRequest
        {
            Model = "default",
            Messages = messages,
            Tools = ContextTools.GetTools(_config),
            ResponseFormat = SchemaGenerator.CreateResponseFormat<TResponse>(),
            Temperature = 0.3
        };
    }

    private string BuildSystemPrompt()
    {
        ProjectInfo project = _projectContext.GetProjectAsync().GetAwaiter().GetResult();

        string projectStructure = FormatProjectStructure(project);
        string loadedFilesContent = FormatLoadedFiles();

        return $"""
            {_config.SystemPrompt}

            ## Project Structure
            {projectStructure}

            ## Files Currently Loaded
            {(State.LoadedFiles.Count > 0 ? loadedFilesContent : "No files loaded yet. Use tools to read files.")}

            ## Instructions
            - Use the available tools to read source files when you need more information.
            - Stay within the scope of the question.
            - Respond using the exact JSON schema provided.
            """;
    }

    private string FormatProjectStructure(ProjectInfo project)
    {
        List<string> lines = [];

        foreach (AssemblyInfo assembly in project.Assemblies.Where(a => !a.IsTestProject))
        {
            lines.Add($"- Assembly: {assembly.Name} ({assembly.RelativePath}/)");

            foreach (TypeSummary type in assembly.Types.Take(20))
            {
                lines.Add($"    - {type.Kind}: {type.Namespace}.{type.Name}");
            }

            if (assembly.Types.Count > 20)
            {
                lines.Add($"    - ... and {assembly.Types.Count - 20} more types");
            }
        }

        return string.Join("\n", lines);
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
        _logger.Debug($"Executing tool: {toolCall.Function.Name}");

        try
        {
            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments,
                _jsonOptions);

            return toolCall.Function.Name switch
            {
                "read_file" => await ExecuteReadFile(args, tracerScope, ct),
                "search_files" => await ExecuteSearchFiles(args),
                "list_assembly_files" => await ExecuteListAssemblyFiles(args),
                "delegate_to_context" => await ExecuteDelegateToContext(args, tracerScope, ct),
                _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolCall.Function.Name}" })
            };
        }
        catch (Exception ex)
        {
            _logger.Warning($"Tool execution failed: {ex.Message}");
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<string> ExecuteReadFile(Dictionary<string, JsonElement>? args, ITracerScope? tracerScope, CancellationToken ct)
    {
        string path = args?["path"].GetString() ?? throw new ArgumentException("path is required");

        if (State.FileContents.TryGetValue(path, out string? cached))
        {
            return JsonSerializer.Serialize(new { path, content = cached, source = "cache" });
        }

        int estimatedTokens = EstimateFileTokens(path);
        if (!State.HasCapacityFor(estimatedTokens, _config.MaxTokenBudget))
        {
            return JsonSerializer.Serialize(new { error = "Token budget exceeded", path });
        }

        string? content = await _fileSystem.ReadFileAsync(path, ct);

        if (content == null)
        {
            return JsonSerializer.Serialize(new { error = "File not found", path });
        }

        State.AddFileContent(path, content);
        tracerScope?.RecordFileLoaded(path, content.Length, estimatedTokens);

        return JsonSerializer.Serialize(new { path, content, tokens = estimatedTokens });
    }

    private Task<string> ExecuteSearchFiles(Dictionary<string, JsonElement>? args)
    {
        string pattern = args?["pattern"].GetString() ?? throw new ArgumentException("pattern is required");

        List<string> files = _fileSystem.EnumerateFiles(null, pattern).Take(50).ToList();

        return Task.FromResult(JsonSerializer.Serialize(new { pattern, files, count = files.Count }));
    }

    private async Task<string> ExecuteListAssemblyFiles(Dictionary<string, JsonElement>? args)
    {
        string assemblyName = args?["assembly_name"].GetString() ?? throw new ArgumentException("assembly_name is required");

        ProjectInfo project = await _projectContext.GetProjectAsync();
        AssemblyInfo? assembly = project.Assemblies.FirstOrDefault(a =>
            a.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));

        if (assembly == null)
        {
            return JsonSerializer.Serialize(new { error = "Assembly not found", assemblyName });
        }

        return JsonSerializer.Serialize(new
        {
            assembly = assembly.Name,
            files = assembly.Files,
            types = assembly.Types.Select(t => new { t.Namespace, t.Name, kind = t.Kind.ToString() })
        });
    }

    private async Task<string> ExecuteDelegateToContext(
        Dictionary<string, JsonElement>? args,
        ITracerScope? tracerScope,
        CancellationToken ct)
    {
        string targetContextName = args?["target_context"].GetString()
            ?? throw new ArgumentException("target_context is required");
        string question = args["question"].GetString()
            ?? throw new ArgumentException("question is required");
        string? filesArg = args.TryGetValue("relevant_files", out JsonElement f) ? f.GetString() : null;

        if (State.DelegationDepth >= _config.MaxDelegationDepth)
        {
            _logger.Warning($"Maximum delegation depth ({_config.MaxDelegationDepth}) reached in context '{_config.Name}'");
            return JsonSerializer.Serialize(new
            {
                error = "Maximum delegation depth reached",
                max_depth = _config.MaxDelegationDepth,
                current_chain = string.Join(" → ", State.DelegationChain.Reverse())
            });
        }

        if (State.DelegationChain.Contains(targetContextName))
        {
            _logger.Warning($"Circular delegation detected: {_config.Name} → {targetContextName}");
            return JsonSerializer.Serialize(new
            {
                error = "Circular delegation detected",
                chain = string.Join(" → ", State.DelegationChain.Reverse().Append(targetContextName))
            });
        }

        try
        {
            if (!Enum.TryParse<PredefinedContext>(targetContextName, out PredefinedContext predefinedContext))
            {
                return JsonSerializer.Serialize(new { error = $"Unknown context: {targetContextName}" });
            }

            IContext targetContext = _contextFactory.GetPredefined(predefinedContext);

            State.IncrementDelegationDepth(targetContextName);

            List<string>? fileList = filesArg?.Split(',').Select(f => f.Trim()).ToList();

            ContextQuery query = fileList != null
                ? ContextQuery.WithFiles(question, fileList.ToArray())
                : ContextQuery.Simple(question);

            using ITracerScope delegationScope = _tracer.BeginContext(
                targetContextName,
                question,
                fileList);

            _logger.Debug($"Context '{_config.Name}' delegating to '{targetContextName}': {question[..Math.Min(50, question.Length)]}...");

            ContextInfoResponse result = await ((Context)targetContext).AskAsync<ContextInfoResponse>(
                query,
                delegationScope,
                ct);

            ContextTrace contextTrace = delegationScope.GetContextTrace();
            tracerScope?.RecordContextQuery(contextTrace);

            return JsonSerializer.Serialize(new
            {
                delegated_to = targetContextName,
                question,
                answer = result.Answer,
                files_examined = result.FilesExamined,
                confidence = result.Confidence
            }, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.Error($"Delegation to '{targetContextName}' failed", ex);
            return JsonSerializer.Serialize(new
            {
                error = $"Delegation failed: {ex.Message}",
                target_context = targetContextName
            });
        }
        finally
        {
            State.DecrementDelegationDepth();
        }
    }

    private async Task LoadFileContent(string path, ITracerScope? tracerScope, CancellationToken ct)
    {
        if (State.FileContents.ContainsKey(path)) return;

        string? content = await _fileSystem.ReadFileAsync(path, ct);
        if (content != null)
        {
            int estimatedTokens = EstimateFileTokens(path);
            State.AddFileContent(path, content);
            tracerScope?.RecordFileLoaded(path, content.Length, estimatedTokens);
        }
    }

    private int EstimateFileTokens(string path)
    {
        return _projectContext.GetFileTokens(path);
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