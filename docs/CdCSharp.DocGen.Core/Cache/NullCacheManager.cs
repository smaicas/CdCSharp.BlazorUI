using CdCSharp.DocGen.Core.Abstractions.Cache;
using CdCSharp.DocGen.Core.Models.Cache;

namespace CdCSharp.DocGen.Core.Cache;

public class NullCacheManager : ICacheManager
{
    public Task<(bool Hit, T? Result)> TryGetAnalysisAsync<T>(string filePath, string analysisType) where T : class
        => Task.FromResult<(bool, T?)>((false, null));

    public Task SetAnalysisAsync<T>(string filePath, string analysisType, T result) where T : class
=> Task.CompletedTask;
    public (bool Hit, string? Response) TryGetQuery(string prompt, string specialistId)
    => (false, null);

    public void SetQuery(string prompt, string specialistId, string response) { }

    public void Clear(bool analysisOnly = false, bool queriesOnly = false) { }

    public CacheStatistics GetStatistics() => new();

    public void PrintStatistics()
    {
        Console.WriteLine("Cache: Disabled");
    }

    public void Dispose() { }
}