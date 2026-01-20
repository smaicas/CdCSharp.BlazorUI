using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Tools.Commands;

public sealed record LoadFileCommand : IToolCommand<LoadedFile>
{
    public string ToolName => "read_file";
    public bool RequiresConfirmation => false;
    public required string Path { get; init; }
}

public sealed record LoadedFile(string Path, string Content, int Tokens, bool Permanent);

public sealed class LoadFileCommandHandler : ICommandHandler<LoadFileCommand, LoadedFile>
{
    public async Task<Result<LoadedFile>> HandleAsync(
        LoadFileCommand command,
        CommandContext context,
        CancellationToken ct)
    {
        if (context.Execution.State!.FileContents.TryGetValue(command.Path, out string? cached))
        {
            int cachedTokens = context.Knowledge.Context.GetFileTokens(command.Path);
            return Result<LoadedFile>.Success(new LoadedFile(command.Path, cached, cachedTokens, Permanent: true));
        }

        if (!context.Knowledge.Metadata.FileExists(command.Path))
        {
            IEnumerable<string> similar = context.Knowledge.Metadata.FindSimilarFiles(command.Path, 5);
            Error error = Error.FileNotFound(command.Path);
            error.Metadata!["similar"] = similar.ToList();
            return Result<LoadedFile>.Failure(error);
        }

        int tokens = context.Knowledge.Context.GetFileTokens(command.Path);
        BudgetAllocation? allocation = context.Orchestration?.BudgetManager.GetAllocation(context.Execution.Config!.Name);

        if (allocation != null && !allocation.CanAllocate(tokens))
        {
            return Result<LoadedFile>.Failure(
                Error.BudgetExhausted(context.Execution.Config!.Name, tokens, allocation.AvailableTokens));
        }

        string? content = await context.Infrastructure.FileSystem.ReadFileAsync(command.Path, ct);
        if (content == null)
        {
            return Result<LoadedFile>.Failure(Error.FileNotFound(command.Path));
        }

        context.Execution.State.AddFileContent(command.Path, content);
        context.Orchestration?.Registry.RegisterLoadedFile(context.Execution.Config!.Name, command.Path, content);
        context.Orchestration?.BudgetManager.RecordUsage(context.Execution.Config!.Name, tokens);
        context.Execution.Tracer?.RecordFileLoaded(command.Path, content.Length, tokens);

        return Result<LoadedFile>.Success(new LoadedFile(command.Path, content, tokens, Permanent: true));
    }
}