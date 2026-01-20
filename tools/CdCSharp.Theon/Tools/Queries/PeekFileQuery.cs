using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Tools.Queries;

public sealed record PeekFileQuery : IToolQuery<FileContent>
{
    public string ToolName => "peek_file";
    public required string Path { get; init; }
    public string? SourceContext { get; init; }
}

public sealed record FileContent(string Path, string Content, bool Ephemeral);

public sealed class PeekFileQueryHandler : IQueryHandler<PeekFileQuery, FileContent>
{
    public async Task<Result<FileContent>> HandleAsync(
        PeekFileQuery query,
        QueryContext context,
        CancellationToken ct)
    {
        string? content = context.Orchestration?.Registry.PeekFile(query.Path, query.SourceContext);
        string source;

        if (content != null)
        {
            source = $"context:{context.Orchestration.Registry.FindFileOwner(query.Path)}";
        }
        else
        {
            content = await context.Infrastructure.FileSystem.ReadFileAsync(query.Path, ct);
            source = "disk";
        }

        if (content == null)
        {
            IEnumerable<string> similar = context.Knowledge.Metadata.FindSimilarFiles(query.Path, 5);
            Error error = Error.FileNotFound(query.Path);
            error.Metadata["similar"] = similar.ToList();
            return Result<FileContent>.Failure(error);
        }

        return Result<FileContent>.Success(new FileContent(query.Path, content, Ephemeral: true));
    }
}