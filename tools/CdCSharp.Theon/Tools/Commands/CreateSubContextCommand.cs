using CdCSharp.Theon.Context;
using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Tools.Commands;

public sealed record CreateSubContextCommand : IToolCommand<SubContextResult>
{
    public string ToolName => "create_sub_context";
    public bool RequiresConfirmation => false;

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

public sealed class CreateSubContextCommandHandler : ICommandHandler<CreateSubContextCommand, SubContextResult>
{
    public async Task<Result<SubContextResult>> HandleAsync(
        CreateSubContextCommand command,
        CommandContext context,
        CancellationToken ct)
    {
        if (command.Files.Count == 0)
        {
            return Result<SubContextResult>.Failure(
                Error.Custom("NO_FILES_SPECIFIED", "No files specified for sub-context"));
        }

        IContextScope scope;

        if (command.ContextType == SubContextType.Clone)
        {
            if (context.Execution.CloneDepth >= context.Execution.Config!.MaxCloneDepth)
            {
                return Result<SubContextResult>.Failure(
                    Error.MaxDepthReached("clone", context.Execution.Config.MaxCloneDepth));
            }

            int cloneCount = context.Orchestration!.Registry.GetCloneCount(context.Execution.Config.ContextType);
            if (cloneCount >= context.Execution.Config.MaxClonesPerType)
            {
                return Result<SubContextResult>.Failure(
                    Error.Custom("MAX_CLONES_REACHED",
                        $"Maximum clones ({context.Execution.Config.MaxClonesPerType}) for {context.Execution.Config.ContextType} reached"));
            }

            scope = context.Orchestration.Factory.CreateSibling(
                context.Execution.Config,
                command.Question,
                context.Execution.CloneDepth + 1);
        }
        else
        {
            if (string.IsNullOrEmpty(command.TargetContextType))
            {
                return Result<SubContextResult>.Failure(
                    Error.Custom("MISSING_TARGET_TYPE", "TargetContextType required for delegation"));
            }

            if (command.TargetContextType == context.Execution.Config!.ContextType)
            {
                return Result<SubContextResult>.Failure(
                    Error.Custom("INVALID_DELEGATION", "Use Clone for same context type"));
            }

            scope = context.Orchestration!.Factory.CreateDelegate(
                command.TargetContextType,
                command.Question);
        }

        ContextQuery query = ContextQuery.WithFiles(command.Question, command.Files.ToArray());
        Result<ContextInfoResponse> result = await scope.QueryAsync<ContextInfoResponse>(
            query,
            context.Execution.Tracer,
            ct);

        return result.Map(r => new SubContextResult(
            scope.Name,
            command.Question,
            r.Answer,
            r.FilesExamined,
            r.Confidence));
    }
}