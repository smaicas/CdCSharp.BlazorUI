using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Tools.Commands;

public sealed record GenerateOutputFileCommand : IToolCommand<GeneratedOutput>
{
    public string ToolName => "generate_output_file";
    public bool RequiresConfirmation => false;

    public required string Folder { get; init; }
    public required string Filename { get; init; }
    public required string Content { get; init; }
}

public sealed record GeneratedOutput(string FullPath);

public sealed class GenerateOutputFileCommandHandler : ICommandHandler<GenerateOutputFileCommand, GeneratedOutput>
{
    public async Task<Result<GeneratedOutput>> HandleAsync(
        GenerateOutputFileCommand command,
        CommandContext context,
        CancellationToken ct)
    {
        await context.Infrastructure.FileSystem.WriteOutputFileAsync(
            command.Folder,
            command.Filename,
            command.Content,
            ct);

        string outputPath = Path.Combine(
            context.Infrastructure.Options.ResponsesPath,
            command.Folder,
            command.Filename);

        return Result<GeneratedOutput>.Success(new GeneratedOutput(outputPath));
    }
}