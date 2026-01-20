using CdCSharp.Theon.Core;

namespace CdCSharp.Theon.Tools.Queries;

public sealed record SearchFilesQuery : IToolQuery<FileSearchResult>
{
    public string ToolName => "search_files";
    public required string Pattern { get; init; }
}

public sealed record FileSearchResult(
    string Pattern,
    List<string> Files,
    List<string>? AlreadyLoadedElsewhere);

public sealed class SearchFilesQueryHandler : IQueryHandler<SearchFilesQuery, FileSearchResult>
{
    public Task<Result<FileSearchResult>> HandleAsync(
        SearchFilesQuery query,
        QueryContext context,
        CancellationToken ct)
    {
        if (query.Pattern is "*" or "**")
        {
            return Task.FromResult(Result<FileSearchResult>.Failure(
                Error.InvalidPattern(query.Pattern, "Pattern too broad. Use specific patterns like '**/*.cs'")));
        }

        List<string> files = context.Knowledge.Metadata.FindFilesByPattern(query.Pattern).ToList();

        List<string>? alreadyLoaded = null;
        if (context.Orchestration != null)
        {
            IReadOnlyDictionary<string, IReadOnlyList<string>> loadedByContexts =
                context.Orchestration.Registry.GetAllLoadedFiles();

            alreadyLoaded = files
                .Where(f => loadedByContexts.Values.Any(list => list.Contains(f)))
                .ToList();

            if (alreadyLoaded.Count == 0)
                alreadyLoaded = null;
        }

        FileSearchResult result = new(query.Pattern, files, alreadyLoaded);
        return Task.FromResult(Result<FileSearchResult>.Success(result));
    }
}