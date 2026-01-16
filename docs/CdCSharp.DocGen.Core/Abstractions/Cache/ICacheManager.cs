using CdCSharp.DocGen.Core.Models.Cache;

namespace CdCSharp.DocGen.Core.Abstractions.Cache;

public interface ICacheManager : IDisposable
{
    Task<(bool Hit, T? Result)> TryGetAnalysisAsync<T>(string filePath, string analysisType) where T : class;
    Task SetAnalysisAsync<T>(string filePath, string analysisType, T result) where T : class;
    (bool Hit, string? Response) TryGetQuery(string prompt, string specialistId);
    void SetQuery(string prompt, string specialistId, string response);
    void Clear(bool analysisOnly = false, bool queriesOnly = false);
    CacheStatistics GetStatistics();
    void PrintStatistics();
}