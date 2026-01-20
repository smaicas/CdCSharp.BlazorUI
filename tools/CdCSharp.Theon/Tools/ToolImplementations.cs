using CdCSharp.Theon.Context;
using CdCSharp.Theon.Context.Planning;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Orchestrator;
using CdCSharp.Theon.Orchestrator.Models;
using CdCSharp.Theon.Tracing;

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
        Tracer.Record(new FileReadEvent(tool.Path, content.Length, 0, IsEphemeral: true));

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

        context.Execution.State!.AddFileContent(tool.Path, content);
        context.Orchestration?.Registry.RegisterLoadedFile(
            context.Execution.Config!.Name,
            tool.Path,
            content);
        context.Orchestration?.BudgetManager.RecordUsage(
            context.Execution.Config!.Name,
            tokens);

        // ADD THESE LOG CALLS:
        context.Infrastructure.Logger.LogFileOperation(
            "Loaded (permanent)",
            tool.Path,
            tokens);

        Tracer.Record(new FileReadEvent(tool.Path, content.Length, tokens, IsEphemeral: false));

        allocation = context.Orchestration?.BudgetManager.GetAllocation(context.Execution.Config!.Name);
        if (allocation != null)
        {
            context.Infrastructure.Logger.LogBudgetStatus(
                context.Execution.Config!.Name,
                allocation.UsedTokens,
                allocation.MaxTokens,
                allocation.UtilizationPercent);
        }

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
        Result<ContextInfoResponse> result = await scope.QueryAsync<ContextInfoResponse>(query, ct);

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

        Result<ExecutionPlan> result = await plannerScope.QueryAsync<ExecutionPlan>(contextQuery, ct);

        return result.Bind(plan => ValidatePlan(plan, context));
    }

    private static Result<ExecutionPlan> ValidatePlan(ExecutionPlan plan, ToolContext context)
    {
        if (plan.Steps == null || plan.Steps.Count == 0)
        {
            return Result<ExecutionPlan>.Failure(
                Error.Custom("INVALID_PLAN", "Plan must have at least one step"));
        }

        context.Orchestration!.State!.SetPlan(plan);

        // ADD THESE LOG CALLS:
        context.Infrastructure.Logger.LogPlanCreated(plan.Steps.Count, plan.TaskTypes);

        foreach (PlanStep? step in plan.Steps.OrderBy(s => s.Order))
        {
            context.Infrastructure.Logger.LogPlanStep(
                step.Order,
                plan.Steps.Count,
                "Pending",
                step.TargetContext,
                step.Purpose);
        }

        return Result<ExecutionPlan>.Success(plan);
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

public sealed record QueryContextTool : ITool<ContextQueryResult>
{
    public string ToolName => "query_context";
    public bool RequiresConfirmation => false;
    public bool IsReadOnly => true;

    public required string ContextName { get; init; }
    public required string Question { get; init; }
    public string? Files { get; init; }
}

public sealed record ContextQueryResult(
    string ContextName,
    string Question,
    string Answer,
    List<string> FilesExamined,
    float Confidence);

public sealed class QueryContextHandler : IToolHandler<QueryContextTool, ContextQueryResult>
{
    public async Task<Result<ContextQueryResult>> HandleAsync(
    QueryContextTool tool,
    ToolContext context,
    CancellationToken ct)
    {
        if (context.Orchestration?.State == null)
        {
            return Result<ContextQueryResult>.Failure(
                Error.Custom("NOT_IN_ORCHESTRATOR", "query_context can only be used from orchestrator"));
        }

        // ADD THIS LOG CALL:
        context.Infrastructure.Logger.LogContextQuery(
            "Orchestrator",
            tool.ContextName,
            tool.Question);

        if (context.Orchestration.State.HasPlan)
        {
            ExecutionPlan plan = context.Orchestration.State.CurrentPlan!;

            List<string>? requestedFiles = null;
            if (!string.IsNullOrWhiteSpace(tool.Files))
            {
                requestedFiles = tool.Files
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            PlanValidator validator = new();
            Result<PlanStep> validation = validator.ValidateQueryAgainstPlan(
                plan,
                tool.ContextName,
                requestedFiles);

            if (!validation.IsSuccess)
            {
                return Result<ContextQueryResult>.Failure(validation.Error);
            }

            PlanStep step = validation.Value;

            // ADD THIS LOG CALL:
            context.Infrastructure.Logger.LogPlanStep(
                step.Order,
                plan.Steps.Count,
                "InProgress",
                step.TargetContext,
                step.Purpose);

            step.Status = PlanStepStatus.InProgress;

            IContextScope? scope = context.Orchestration.State.GetContext(tool.ContextName);
            if (scope == null)
            {
                scope = context.Orchestration.Factory.CreateDelegate(
                    tool.ContextName,
                    tool.Question);
                context.Orchestration.State.RegisterContext(tool.ContextName, scope);
            }

            List<string> filesToLoad = step.SuggestedFiles.ToList();

            if (requestedFiles != null)
            {
                foreach (string file in requestedFiles)
                {
                    if (!filesToLoad.Contains(file, StringComparer.OrdinalIgnoreCase))
                    {
                        filesToLoad.Add(file);
                    }
                }
            }

            ContextQuery query = filesToLoad.Count > 0
                ? ContextQuery.WithFiles(tool.Question, filesToLoad.ToArray())
                : ContextQuery.Simple(tool.Question);

            Result<ContextInfoResponse> result = await scope.QueryAsync<ContextInfoResponse>(query, ct);

            return result.Match(
                success =>
                {
                    context.Orchestration.State.MarkStepCompleted(step.Order, success.Answer);
                    context.Infrastructure.Logger.LogPlanStep(
                        step.Order,
                        plan.Steps.Count,
                        "Completed",
                        step.TargetContext,
                        step.Purpose);

                    return Result<ContextQueryResult>.Success(new ContextQueryResult(
                        tool.ContextName,
                        tool.Question,
                        success.Answer,
                        success.FilesExamined,
                        success.Confidence));
                },
                error =>
                {
                    context.Orchestration.State.MarkStepFailed(step.Order, error.Message);

                    // ADD THIS LOG CALL:
                    context.Infrastructure.Logger.LogPlanStep(
                        step.Order,
                        plan.Steps.Count,
                        "Failed",
                        step.TargetContext,
                        step.Purpose);

                    return Result<ContextQueryResult>.Failure(error);
                });
        }
        else
        {
            // No plan - allow direct query (for simple questions)
            IContextScope? scope = context.Orchestration.State.GetContext(tool.ContextName);
            if (scope == null)
            {
                scope = context.Orchestration.Factory.CreateDelegate(
                    tool.ContextName,
                    tool.Question);
                context.Orchestration.State.RegisterContext(tool.ContextName, scope);
            }

            List<string>? files = null;
            if (!string.IsNullOrWhiteSpace(tool.Files))
            {
                files = tool.Files
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            ContextQuery query = files != null && files.Count > 0
                ? ContextQuery.WithFiles(tool.Question, files.ToArray())
                : ContextQuery.Simple(tool.Question);

            Result<ContextInfoResponse> result = await scope.QueryAsync<ContextInfoResponse>(query, ct);

            return result.Map(r => new ContextQueryResult(
                tool.ContextName,
                tool.Question,
                r.Answer,
                r.FilesExamined,
                r.Confidence));
        }
    }
}