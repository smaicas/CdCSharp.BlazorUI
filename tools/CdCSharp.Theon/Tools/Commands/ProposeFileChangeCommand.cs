using CdCSharp.Theon.Core;
using CdCSharp.Theon.Orchestrator.Models;

namespace CdCSharp.Theon.Tools.Commands;

public sealed record ProposeFileChangeCommand : IToolCommand<ProposedChange>
{
    public string ToolName => "propose_file_change";
    public bool RequiresConfirmation => true;

    public required string Path { get; init; }
    public required string Description { get; init; }
    public required string NewContent { get; init; }
}

public sealed class ProposeFileChangeCommandHandler : ICommandHandler<ProposeFileChangeCommand, ProposedChange>
{
    public async Task<Result<ProposedChange>> HandleAsync(
        ProposeFileChangeCommand command,
        CommandContext context,
        CancellationToken ct)
    {
        if (context.Orchestration?.State == null)
        {
            return Result<ProposedChange>.Failure(
                Error.Custom("NOT_IN_ORCHESTRATOR", "File changes can only be proposed from orchestrator"));
        }

        string? original = await context.Infrastructure.FileSystem.ReadFileAsync(command.Path, ct);

        ProposedChange change = new()
        {
            Path = command.Path,
            Description = command.Description,
            ChangeType = original == null ? ChangeType.Create : ChangeType.Modify,
            OriginalContent = original,
            NewContent = command.NewContent,
            Status = ChangeStatus.Pending
        };

        context.Orchestration.State.ProposeChange(change);
        return Result<ProposedChange>.Success(change);
    }
}