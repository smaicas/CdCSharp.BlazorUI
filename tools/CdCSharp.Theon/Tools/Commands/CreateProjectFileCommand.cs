using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Tools.Commands;

public sealed record CreateProjectFileCommand : IToolCommand<CreatedFile>
{
    public string ToolName => "create_project_file";
    public bool RequiresConfirmation => false;

    public required string Path { get; init; }
    public required string Content { get; init; }
}

public sealed record CreatedFile(string Path);

public sealed class CreateProjectFileCommandHandler : ICommandHandler<CreateProjectFileCommand, CreatedFile>
{
    public async Task<Result<CreatedFile>> HandleAsync(
        CreateProjectFileCommand command,
        CommandContext context,
        CancellationToken ct)
    {
        if (!context.Infrastructure.Options.Modification.Enabled)
        {
            return Result<CreatedFile>.Failure(Error.ModificationDisabled());
        }

        string? existing = await context.Infrastructure.FileSystem.ReadFileAsync(command.Path, ct);
        if (existing != null)
        {
            return Result<CreatedFile>.Failure(Error.FileAlreadyExists(command.Path));
        }

        bool success = await context.Infrastructure.FileSystem.WriteProjectFileAsync(
            command.Path,
            command.Content,
            ct);

        if (!success)
        {
            return Result<CreatedFile>.Failure(
                Error.Custom("FILE_WRITE_FAILED", $"Failed to create file: {command.Path}"));
        }

        return Result<CreatedFile>.Success(new CreatedFile(command.Path));
    }
}