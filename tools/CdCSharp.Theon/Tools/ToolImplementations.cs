using CdCSharp.Theon.Context;
using CdCSharp.Theon.Context.Planning;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Orchestrator.Models;
using System.Text.Json;

namespace CdCSharp.Theon.Tools;

public sealed record PeekFileTool : ITool<FileContent>
{
    public string ToolName => "peek_file";
    public bool RequiresConfirmation => false;
    public bool IsReadOnly => true;

    public required string Path { get; init; }
    public string? SourceContext { get; init; }
}

public sealed record FileContent(
    string Path,
    string Content,
    bool IsEphemeral,
    string Source);

public sealed class PeekFileHandler : IToolHandler<PeekFileTool, FileContent>
{
    public async Task<Result<FileContent>> HandleAsync(
        PeekFileTool tool,
        ToolContext context,
        CancellationToken ct)
    {
        // First, try to get from registry (already loaded by another context)
        string? content = context.Orchestration?.Registry.PeekFile(tool.Path, tool.SourceContext);
        string source;

        if (content != null)
        {
            source = $"context:{context.Orchestration!.Registry.FindFileOwner(tool.Path)}";
            context.Infrastructure.Logger.Debug(
                $"[{context.Execution.Config?.Name}] Peeked from {source}: {tool.Path}");
        }
        else
        {
            // Not in cache, read from disk
            if (!context.Knowledge.Metadata.FileExists(tool.Path))
            {
                IEnumerable<string> similar = context.Knowledge.Metadata.FindSimilarFiles(tool.Path, 5);
                Error error = Error.FileNotFound(tool.Path);
                error.Metadata!["similar"] = similar.ToList();
                return Result<FileContent>.Failure(error);
            }

            content = await context.Infrastructure.FileSystem.ReadFileAsync(tool.Path, ct);
            source = "disk";

            if (content == null)
            {
                return Result<FileContent>.Failure(Error.FileNotFound(tool.Path));
            }

            context.Infrastructure.Logger.Debug(
                $"[{context.Execution.Config?.Name}] Peeked from disk: {tool.Path}");
        }

        // IMPORTANT: Do NOT add to state, do NOT register in registry
        // This is ephemeral - only for next LLM response
        context.Execution.Tracer?.RecordFileLoaded(tool.Path, content.Length, 0);

        return Result<FileContent>.Success(
            new FileContent(tool.Path, content, IsEphemeral: true, source));
    }
}

public sealed record ReadFileTool : ITool<LoadedFile>
{
    public string ToolName => "read_file";
    public bool RequiresConfirmation => false;
    public bool IsReadOnly => true;

    public required string Path { get; init; }
}

public sealed record LoadedFile(
    string Path,
    string Content,
    int Tokens,
    bool IsPermanent);

public sealed class ReadFileHandler : IToolHandler<ReadFileTool, LoadedFile>
{
    public async Task<Result<LoadedFile>> HandleAsync(
        ReadFileTool tool,
        ToolContext context,
        CancellationToken ct)
    {
        // Check if already loaded in THIS context
        if (context.Execution.State!.FileContents.TryGetValue(tool.Path, out string? cached))
        {
            int cachedTokens = context.Knowledge.Context.GetFileTokens(tool.Path);
            context.Infrastructure.Logger.Debug(
                $"[{context.Execution.Config?.Name}] File already in context: {tool.Path}");

            return Result<LoadedFile>.Success(
                new LoadedFile(tool.Path, cached, cachedTokens, IsPermanent: true));
        }

        // Validate file exists
        if (!context.Knowledge.Metadata.FileExists(tool.Path))
        {
            IEnumerable<string> similar = context.Knowledge.Metadata.FindSimilarFiles(tool.Path, 5);
            Error error = Error.FileNotFound(tool.Path);
            error.Metadata!["similar"] = similar.ToList();
            return Result<LoadedFile>.Failure(error);
        }

        // Check budget
        int tokens = context.Knowledge.Context.GetFileTokens(tool.Path);
        BudgetAllocation? allocation = context.Orchestration?.BudgetManager.GetAllocation(
            context.Execution.Config!.Name);

        if (allocation != null && !allocation.CanAllocate(tokens))
        {
            return Result<LoadedFile>.Failure(
                Error.BudgetExhausted(
                    context.Execution.Config!.Name,
                    tokens,
                    allocation.AvailableTokens));
        }

        // Read from disk
        string? content = await context.Infrastructure.FileSystem.ReadFileAsync(tool.Path, ct);
        if (content == null)
        {
            return Result<LoadedFile>.Failure(Error.FileNotFound(tool.Path));
        }

        // IMPORTANT: Add to state (permanent), register in registry, consume budget
        context.Execution.State.AddFileContent(tool.Path, content);
        context.Orchestration?.Registry.RegisterLoadedFile(
            context.Execution.Config!.Name,
            tool.Path,
            content);
        context.Orchestration?.BudgetManager.RecordUsage(
            context.Execution.Config!.Name,
            tokens);
        context.Execution.Tracer?.RecordFileLoaded(tool.Path, content.Length, tokens);

        context.Infrastructure.Logger.Debug(
            $"[{context.Execution.Config.Name}] Loaded permanently: {tool.Path} ({tokens} tokens)");

        return Result<LoadedFile>.Success(
            new LoadedFile(tool.Path, content, tokens, IsPermanent: true));
    }
}

// ==================== SearchFiles ====================
public sealed record SearchFilesTool : ITool<FileSearchResult>
{
    public string ToolName => "search_files";
    public bool RequiresConfirmation => false;
    public bool IsReadOnly => true;

    public required string Pattern { get; init; }
}

public sealed record FileSearchResult(
    string Pattern,
    List<string> Files,
    List<string>? AlreadyLoadedElsewhere);

public sealed class SearchFilesHandler : IToolHandler<SearchFilesTool, FileSearchResult>
{
    public Task<Result<FileSearchResult>> HandleAsync(
        SearchFilesTool tool,
        ToolContext context,
        CancellationToken ct)
    {
        if (tool.Pattern is "*" or "**")
        {
            return Task.FromResult(Result<FileSearchResult>.Failure(
                Error.InvalidPattern(tool.Pattern, "Pattern too broad. Use specific patterns like '**/*.cs'")));
        }

        List<string> files = context.Knowledge.Metadata.FindFilesByPattern(tool.Pattern).ToList();

        List<string>? alreadyLoaded = null;
        if (context.Orchestration != null)
        {
            IReadOnlyDictionary<string, IReadOnlyList<string>> loadedByContexts = context.Orchestration.Registry.GetAllLoadedFiles();
            alreadyLoaded = files
                .Where(f => loadedByContexts.Values.Any(list => list.Contains(f)))
                .ToList();

            if (alreadyLoaded.Count == 0)
                alreadyLoaded = null;
        }

        FileSearchResult result = new(tool.Pattern, files, alreadyLoaded);
        return Task.FromResult(Result<FileSearchResult>.Success(result));
    }
}

// ==================== CreateSubContext ====================
public sealed record CreateSubContextTool : ITool<SubContextResult>
{
    public string ToolName => "create_sub_context";
    public bool RequiresConfirmation => false;
    public bool IsReadOnly => false;

    public required SubContextType ContextType { get; init; }
    public required string Question { get; init; }
    public required List<string> Files { get; init; }
    public string? TargetContextType { get; init; }
}

public enum SubContextType
{
    Clone,
    Delegate
}

public sealed record SubContextResult(
    string ContextName,
    string Question,
    string Answer,
    List<string> FilesExamined,
    float Confidence);

public sealed class CreateSubContextHandler : IToolHandler<CreateSubContextTool, SubContextResult>
{
    public async Task<Result<SubContextResult>> HandleAsync(
        CreateSubContextTool tool,
        ToolContext context,
        CancellationToken ct)
    {
        if (tool.Files.Count == 0)
        {
            return Result<SubContextResult>.Failure(
                Error.Custom("NO_FILES_SPECIFIED", "No files specified for sub-context"));
        }

        IContextScope scope;

        if (tool.ContextType == SubContextType.Clone)
        {
            if (context.Execution.CloneDepth >= context.Execution.Config!.MaxCloneDepth)
            {
                return Result<SubContextResult>.Failure(
                    Error.MaxDepthReached("clone", context.Execution.Config.MaxCloneDepth));
            }

            int cloneCount = context.Orchestration!.Registry.GetCloneCount(
                context.Execution.Config.ContextType);

            if (cloneCount >= context.Execution.Config.MaxClonesPerType)
            {
                return Result<SubContextResult>.Failure(
                    Error.Custom("MAX_CLONES_REACHED",
                        $"Maximum clones ({context.Execution.Config.MaxClonesPerType}) for {context.Execution.Config.ContextType} reached"));
            }

            scope = context.Orchestration.Factory.CreateSibling(
                context.Execution.Config,
                tool.Question,
                context.Execution.CloneDepth + 1);
        }
        else
        {
            if (string.IsNullOrEmpty(tool.TargetContextType))
            {
                return Result<SubContextResult>.Failure(
                    Error.Custom("MISSING_TARGET_TYPE", "TargetContextType required for delegation"));
            }

            if (tool.TargetContextType == context.Execution.Config!.ContextType)
            {
                return Result<SubContextResult>.Failure(
                    Error.Custom("INVALID_DELEGATION", "Use Clone for same context type"));
            }

            scope = context.Orchestration!.Factory.CreateDelegate(
                tool.TargetContextType,
                tool.Question);
        }

        ContextQuery query = ContextQuery.WithFiles(tool.Question, tool.Files.ToArray());
        Result<ContextInfoResponse> result = await scope.QueryAsync<ContextInfoResponse>(
            query,
            context.Execution.Tracer,
            ct);

        return result.Map(r => new SubContextResult(
            scope.Name,
            tool.Question,
            r.Answer,
            r.FilesExamined,
            r.Confidence));
    }
}

// ==================== CreateExecutionPlan ====================
public sealed record CreateExecutionPlanTool : ITool<ExecutionPlan>
{
    public string ToolName => "create_execution_plan";
    public bool RequiresConfirmation => false;
    public bool IsReadOnly => true;

    public required string UserRequest { get; init; }
}

public sealed class CreateExecutionPlanHandler : IToolHandler<CreateExecutionPlanTool, ExecutionPlan>
{
    public async Task<Result<ExecutionPlan>> HandleAsync(
        CreateExecutionPlanTool tool,
        ToolContext context,
        CancellationToken ct)
    {
        IReadOnlyList<ContextMetadata> availableContexts = context.Orchestration!.Registry.GetAllContexts();

        IContextScope plannerScope = context.Orchestration.Factory.CreatePlanner(availableContexts);

        string fileIndex = BuildFileIndex(context);

        string planningPrompt = $"""
            ## User Request
            {tool.UserRequest}

            ## Available Files in Project
            {fileIndex}

            Analyze this request and create a comprehensive execution plan.
            
            IMPORTANT:
            - You can use `peek_file` to examine files if needed to understand structure
            - Use `search_files` to find relevant files by pattern
            - Be specific about which files should be examined in each plan step
            - Consider the order of investigation: architecture → specific code → dependencies
            
            You MUST respond with ONLY valid JSON matching the ExecutionPlan schema.
            Do NOT include any text before or after the JSON.
            Do NOT wrap the JSON in markdown code fences.
            """;

        ContextQuery contextQuery = ContextQuery.Simple(planningPrompt);

        // CRITICAL: The planner context now uses structured output internally
        Result<ContextInfoResponse> result = await plannerScope.QueryAsync<ContextInfoResponse>(
            contextQuery,
            context.Execution.Tracer,
            ct);

        return result.Bind(response => ParsePlan(response.Answer, context));
    }

    private static string BuildFileIndex(ToolContext context)
    {
        return context.Knowledge.Metadata.FileIndex
            .Where(kvp => kvp.Value.EstimatedTokens > 0)
            .GroupBy(kvp => Path.GetDirectoryName(kvp.Key) ?? "")
            .OrderBy(g => g.Key)
            .Aggregate("", (acc, group) =>
            {
                string dir = string.IsNullOrEmpty(group.Key) ? "(root)" : group.Key;
                string files = string.Join("\n", group
                    .OrderBy(f => f.Key)
                    .Select(f => $"  - `{f.Key}` ({f.Value.EstimatedTokens} tokens)"));
                return $"{acc}\n**{dir}/**\n{files}\n";
            });
    }

    private static Result<ExecutionPlan> ParsePlan(string content, ToolContext context)
    {
        try
        {
            // The response should already be structured JSON from the LLM
            // But we still need to be defensive
            string trimmed = content.Trim();

            // Remove markdown code fences if present (shouldn't happen with structured output)
            if (trimmed.StartsWith("```"))
            {
                int start = trimmed.IndexOf('{');
                int end = trimmed.LastIndexOf('}');
                if (start >= 0 && end > start)
                {
                    trimmed = trimmed[start..(end + 1)];
                }
            }

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };

            ExecutionPlanDto? dto = JsonSerializer.Deserialize<ExecutionPlanDto>(trimmed, options);

            if (dto == null)
            {
                context.Infrastructure.Logger.Error("Deserialization returned null");
                context.Infrastructure.Logger.Debug($"Content: {content}");
                return Result<ExecutionPlan>.Failure(
                    Error.Custom("PLAN_PARSE_ERROR", "Failed to deserialize execution plan"));
            }

            ExecutionPlan plan = MapFromDto(dto);

            if (plan.Steps.Count == 0)
            {
                return Result<ExecutionPlan>.Failure(
                    Error.Custom("INVALID_PLAN", "Plan must have at least one step"));
            }

            return Result<ExecutionPlan>.Success(plan);
        }
        catch (JsonException ex)
        {
            context.Infrastructure.Logger.Warning($"Failed to parse plan: {ex.Message}");
            context.Infrastructure.Logger.Debug($"Content: {content}");
            context.Infrastructure.Logger.Debug($"Path: {ex.Path} | Line: {ex.LineNumber} | Position: {ex.BytePositionInLine}");
            return Result<ExecutionPlan>.Failure(
                Error.Custom("PLAN_PARSE_ERROR", $"Invalid JSON in plan: {ex.Message}"));
        }
    }

    private static ExecutionPlan MapFromDto(ExecutionPlanDto dto)
    {
        return new ExecutionPlan
        {
            TaskTypes = dto.TaskTypes ?? [],
            Reasoning = dto.Reasoning ?? "",
            Steps = dto.Steps?.Select(s => new PlanStep
            {
                Order = s.Order,
                TargetContext = s.TargetContext ?? "",
                Question = s.Question ?? "",
                SuggestedFiles = s.SuggestedFiles ?? [],
                Purpose = s.Purpose ?? "",
                ContributesTo = s.ContributesTo ?? []
            }).ToList() ?? [],
            ExpectedOutputs = dto.ExpectedOutputs?.Select(o => new ExpectedOutput
            {
                TaskType = o.TaskType ?? "",
                Description = o.Description ?? "",
                Type = ParseOutputType(o.OutputType)
            }).ToList() ?? []
        };
    }

    private static OutputType ParseOutputType(string? outputType)
    {
        return outputType?.ToLowerInvariant() switch
        {
            "documentation" => OutputType.Documentation,
            "codechange" => OutputType.CodeChange,
            "analysisreport" => OutputType.AnalysisReport,
            "projectfile" => OutputType.ProjectFile,
            _ => OutputType.Documentation
        };
    }

    private sealed class ExecutionPlanDto
    {
        public List<string>? TaskTypes { get; set; }
        public string? Reasoning { get; set; }
        public List<PlanStepDto>? Steps { get; set; }
        public List<ExpectedOutputDto>? ExpectedOutputs { get; set; }
    }

    private sealed class PlanStepDto
    {
        public int Order { get; set; }
        public string? TargetContext { get; set; }
        public string? Question { get; set; }
        public List<string>? SuggestedFiles { get; set; }
        public string? Purpose { get; set; }
        public List<string>? ContributesTo { get; set; }
    }

    private sealed class ExpectedOutputDto
    {
        public string? TaskType { get; set; }
        public string? Description { get; set; }
        public string? OutputType { get; set; }
    }
}

// ==================== ProposeFileChange ====================
public sealed record ProposeFileChangeTool : ITool<ProposedChange>
{
    public string ToolName => "propose_file_change";
    public bool RequiresConfirmation => true;
    public bool IsReadOnly => false;

    public required string Path { get; init; }
    public required string Description { get; init; }
    public required string NewContent { get; init; }
}

public sealed class ProposeFileChangeHandler : IToolHandler<ProposeFileChangeTool, ProposedChange>
{
    public async Task<Result<ProposedChange>> HandleAsync(
        ProposeFileChangeTool tool,
        ToolContext context,
        CancellationToken ct)
    {
        if (context.Orchestration?.State == null)
        {
            return Result<ProposedChange>.Failure(
                Error.Custom("NOT_IN_ORCHESTRATOR", "File changes can only be proposed from orchestrator"));
        }

        string? original = await context.Infrastructure.FileSystem.ReadFileAsync(tool.Path, ct);

        ProposedChange change = new()
        {
            Path = tool.Path,
            Description = tool.Description,
            ChangeType = original == null ? ChangeType.Create : ChangeType.Modify,
            OriginalContent = original,
            NewContent = tool.NewContent,
            Status = ChangeStatus.Pending
        };

        context.Orchestration.State.ProposeChange(change);
        return Result<ProposedChange>.Success(change);
    }
}

// ==================== CreateProjectFile ====================
public sealed record CreateProjectFileTool : ITool<CreatedFile>
{
    public string ToolName => "create_project_file";
    public bool RequiresConfirmation => false;
    public bool IsReadOnly => false;

    public required string Path { get; init; }
    public required string Content { get; init; }
}

public sealed record CreatedFile(string Path);

public sealed class CreateProjectFileHandler : IToolHandler<CreateProjectFileTool, CreatedFile>
{
    public async Task<Result<CreatedFile>> HandleAsync(
        CreateProjectFileTool tool,
        ToolContext context,
        CancellationToken ct)
    {
        if (!context.Infrastructure.Options.Modification.Enabled)
        {
            return Result<CreatedFile>.Failure(Error.ModificationDisabled());
        }

        string? existing = await context.Infrastructure.FileSystem.ReadFileAsync(tool.Path, ct);
        if (existing != null)
        {
            return Result<CreatedFile>.Failure(Error.FileAlreadyExists(tool.Path));
        }

        bool success = await context.Infrastructure.FileSystem.WriteProjectFileAsync(
            tool.Path,
            tool.Content,
            ct);

        if (!success)
        {
            return Result<CreatedFile>.Failure(
                Error.Custom("FILE_WRITE_FAILED", $"Failed to create file: {tool.Path}"));
        }

        return Result<CreatedFile>.Success(new CreatedFile(tool.Path));
    }
}

// ==================== GenerateOutputFile ====================
public sealed record GenerateOutputFileTool : ITool<GeneratedOutput>
{
    public string ToolName => "generate_output_file";
    public bool RequiresConfirmation => false;
    public bool IsReadOnly => false;

    public required string Folder { get; init; }
    public required string Filename { get; init; }
    public required string Content { get; init; }
}

public sealed record GeneratedOutput(string FullPath);

public sealed class GenerateOutputFileHandler : IToolHandler<GenerateOutputFileTool, GeneratedOutput>
{
    public async Task<Result<GeneratedOutput>> HandleAsync(
        GenerateOutputFileTool tool,
        ToolContext context,
        CancellationToken ct)
    {
        await context.Infrastructure.FileSystem.WriteOutputFileAsync(
            tool.Folder,
            tool.Filename,
            tool.Content,
            ct);

        string outputPath = Path.Combine(
            context.Infrastructure.Options.ResponsesPath,
            tool.Folder,
            tool.Filename);

        return Result<GeneratedOutput>.Success(new GeneratedOutput(outputPath));
    }
}