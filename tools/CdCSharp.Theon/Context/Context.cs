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
    private readonly SharedProjectKnowledge _sharedKnowledge;
    private readonly ToolCallValidator _toolValidator;
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
        IContextFactory contextFactory,
        SharedProjectKnowledge sharedKnowledge)
    {
        _config = config;
        _aiClient = aiClient;
        _projectContext = projectContext;
        _fileSystem = fileSystem;
        _logger = logger;
        _tracer = tracer;
        _contextFactory = contextFactory;
        _sharedKnowledge = sharedKnowledge;
        _toolValidator = new ToolCallValidator(sharedKnowledge, fileSystem);
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

            ChatCompletionRequest request = await BuildRequest<TResponse>();
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

    private async Task<ChatCompletionRequest> BuildRequest<TResponse>() where TResponse : class
    {
        string systemPrompt = await BuildSystemPrompt();

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

    private async Task<string> BuildSystemPrompt()
    {
        string projectStructure = _config.IncludeProjectStructure
            ? _sharedKnowledge.GetCompactSummary()
            : "Project structure not loaded. Use explore_project_structure to view it.";

        string loadedFilesContent = FormatLoadedFiles();

        string antiPatterns = """
            
            ## ⚠️ Common Mistakes to Avoid
            
            ❌ **DON'T** call read_file with a directory path (e.g., "Domain", "Infrastructure")
            ❌ **DON'T** call read_file patterns (e.g., "*", "*.cs")
            ✅ **DO** use search_files or explore_project_structure first to find specific files
            
            ❌ **DON'T** assume file locations without checking the Project Structure above
            ✅ **DO** verify paths exist in the structure before calling read_file
            
            ❌ **DON'T** delegate to another context for information you can get yourself
            ✅ **DO** use your tools (read_file, search_files, explore_project_structure) before delegating
            
            ❌ **DON'T** ask vague questions when delegating (e.g., "analyze this")
            ✅ **DO** provide specific, focused questions with context (e.g., "What design patterns are used in Domain/Entities/User.cs?")
            
            ❌ **DON'T** call the same tool repeatedly with the same arguments
            ✅ **DO** remember previous tool results and build upon them
            
            ## 🎯 Decision Tree for Tool Usage
            
            **When starting a new task:**
            1. Check the Project Structure above to orient yourself
            2. If you need more detail → use explore_project_structure
            3. If you know exactly what file to read → use read_file
            4. If you need to find files by pattern → use search_files
            5. If you need another perspective → delegate_to_context
            
            **When reading files:**
            - Always verify the path exists in Project Structure first
            - Paths must be exact (e.g., "Domain/Entities/User.cs", not "Domain" or "User.cs")
            - If unsure, use search_files to find the correct path
            
            **When delegating:**
            - Only delegate when you genuinely need expertise from another domain
            - Provide specific questions with relevant file paths
            - Don't delegate for simple file reading or searching
            
            **When exploring structure:**
            - Use "summary" for initial orientation (assemblies and top namespaces)
            - Use "types" when you need to find specific classes/interfaces
            - Use "full" only when you need detailed member signatures
            """;

        return $"""
            {_config.SystemPrompt}

            {projectStructure}

            ## Files Currently Loaded
            {(State.LoadedFiles.Count > 0 ? loadedFilesContent : "No files loaded yet. Use tools to read files as needed.")}

            {antiPatterns}

            ## Instructions
            - Use the available tools to read source files when you need more information.
            - The Project Structure above is ALWAYS available - consult it before calling read_file.
            - Stay within the scope of the question.
            - Respond using the exact JSON schema provided.
            - If you make a mistake, the system will provide feedback - read it carefully and correct your approach.
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
        _logger.Debug($"Executing tool: {toolCall.Function.Name}");

        try
        {
            Dictionary<string, JsonElement>? args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                toolCall.Function.Arguments,
                _jsonOptions);

            ToolCallValidation validation = _toolValidator.Validate(toolCall.Function.Name, args);

            if (!validation.IsValid)
            {
                _logger.Warning($"Tool call validation failed: {validation.ErrorMessage}");

                return JsonSerializer.Serialize(new
                {
                    error = validation.ErrorMessage,
                    suggestion = validation.Suggestion,
                    available_options = validation.AvailableOptions
                }, _jsonOptions);
            }

            return toolCall.Function.Name switch
            {
                "read_file" => await ExecuteReadFile(args, tracerScope, ct),
                "search_files" => await ExecuteSearchFiles(args),
                "list_assembly_files" => await ExecuteListAssemblyFiles(args),
                "explore_project_structure" => await ExecuteExploreProjectStructure(args),
                "delegate_to_context" => await ExecuteDelegateToContext(args, tracerScope, ct),
                _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolCall.Function.Name}" }, _jsonOptions)
            };
        }
        catch (Exception ex)
        {
            _logger.Warning($"Tool execution failed: {ex.Message}");
            return JsonSerializer.Serialize(new { error = ex.Message }, _jsonOptions);
        }
    }

    private async Task<string> ExecuteReadFile(Dictionary<string, JsonElement>? args, ITracerScope? tracerScope, CancellationToken ct)
    {
        string path = args?["path"].GetString() ?? throw new ArgumentException("path is required");

        if (State.FileContents.TryGetValue(path, out string? cached))
        {
            return JsonSerializer.Serialize(new { path, content = cached, source = "cache" }, _jsonOptions);
        }

        int estimatedTokens = EstimateFileTokens(path);
        if (!State.HasCapacityFor(estimatedTokens, _config.MaxTokenBudget))
        {
            return JsonSerializer.Serialize(new { error = "Token budget exceeded", path }, _jsonOptions);
        }

        string? content = await _fileSystem.ReadFileAsync(path, ct);

        if (content == null)
        {
            return JsonSerializer.Serialize(new { error = "File not found", path }, _jsonOptions);
        }

        State.AddFileContent(path, content);
        tracerScope?.RecordFileLoaded(path, content.Length, estimatedTokens);

        return JsonSerializer.Serialize(new { path, content, tokens = estimatedTokens }, _jsonOptions);
    }

    private async Task<string> ExecuteSearchFiles(Dictionary<string, JsonElement>? args)
    {
        string pattern = args?["pattern"].GetString() ?? throw new ArgumentException("pattern is required");

        List<string> files = _fileSystem.EnumerateFiles(null, pattern).ToList();

        return JsonSerializer.Serialize(new { pattern, files, count = files.Count }, _jsonOptions);
    }

    private async Task<string> ExecuteListAssemblyFiles(Dictionary<string, JsonElement>? args)
    {
        string assemblyName = args?["assembly_name"].GetString() ?? throw new ArgumentException("assembly_name is required");

        ProjectInfo project = await _projectContext.GetProjectAsync();
        AssemblyInfo? assembly = project.Assemblies.FirstOrDefault(a =>
            a.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));

        if (assembly == null)
        {
            return JsonSerializer.Serialize(new { error = "Assembly not found", assemblyName }, _jsonOptions);
        }

        return JsonSerializer.Serialize(new
        {
            assembly = assembly.Name,
            files = assembly.Files,
            types = assembly.Types.Select(t => new { t.Namespace, t.Name, kind = t.Kind.ToString() })
        }, _jsonOptions);
    }

    private async Task<string> ExecuteExploreProjectStructure(Dictionary<string, JsonElement>? args)
    {
        string detailLevel = args?.TryGetValue("detail_level", out JsonElement level) == true
            ? level.GetString() ?? "summary"
            : "summary";

        await _sharedKnowledge.GetProjectAsync(); // Ensure initialized

        string structure = detailLevel switch
        {
            "types" => _sharedKnowledge.GetDetailedSummary(),
            "full" => _sharedKnowledge.GetDetailedSummary(), // Could be extended with member info
            _ => _sharedKnowledge.GetCompactSummary()
        };

        return JsonSerializer.Serialize(new
        {
            detail_level = detailLevel,
            structure,
            assemblies = _sharedKnowledge.GetAssemblyNames(),
            total_files = _sharedKnowledge.FileIndex.Count
        }, _jsonOptions);
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

        // Delegation circuit breaker
        if (!State.CanDelegateTo(targetContextName, question))
        {
            if (State.DelegationDepth >= _config.MaxDelegationDepth)
            {
                _logger.Warning($"Maximum delegation depth ({_config.MaxDelegationDepth}) reached in context '{_config.Name}'");
                return JsonSerializer.Serialize(new
                {
                    error = "Maximum delegation depth reached",
                    max_depth = _config.MaxDelegationDepth,
                    current_chain = string.Join(" → ", State.DelegationChain.Reverse())
                }, _jsonOptions);
            }

            if (State.DelegationChain.Contains(targetContextName))
            {
                _logger.Warning($"Circular delegation detected: {_config.Name} → {targetContextName}");
                return JsonSerializer.Serialize(new
                {
                    error = "Circular delegation detected",
                    chain = string.Join(" → ", State.DelegationChain.Reverse().Append(targetContextName))
                }, _jsonOptions);
            }

            // Ya se hizo esta pregunta a este contexto
            _logger.Warning($"Repeated query to {targetContextName} detected");
            return JsonSerializer.Serialize(new
            {
                error = "This question was already asked to this context",
                suggestion = "Try reformulating the question or using your own tools (read_file, search_files)"
            }, _jsonOptions);
        }

        try
        {
            if (!Enum.TryParse<PredefinedContext>(targetContextName, out PredefinedContext predefinedContext))
            {
                return JsonSerializer.Serialize(new { error = $"Unknown context: {targetContextName}" }, _jsonOptions);
            }

            IContext targetContext = _contextFactory.GetPredefined(predefinedContext);

            State.IncrementDelegationDepth(targetContextName);
            State.RecordDelegation(targetContextName, question);

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
            }, _jsonOptions);
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