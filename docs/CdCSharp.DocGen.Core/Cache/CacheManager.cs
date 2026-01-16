using CdCSharp.DocGen.Core.Abstractions.Cache;
using CdCSharp.DocGen.Core.Models.Cache;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CdCSharp.DocGen.Core.Cache;

public class CacheManager : ICacheManager
{
    private readonly string _cacheDir;
    private readonly string _manifestPath;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<CacheManager> _logger;
    private CacheManifest _manifest;
    private bool _isDirty;

    public CacheManager(IOptions<DocGenOptions> options, ILogger<CacheManager> logger)
    {
        _cacheOptions = options.Value.Cache;
        _logger = logger;
        _cacheDir = Path.Combine(options.Value.ProjectPath, ".doccache");
        _manifestPath = Path.Combine(_cacheDir, "manifest.json");

        Directory.CreateDirectory(_cacheDir);
        _manifest = LoadManifest(options.Value.ProjectPath);
    }

    private CacheManifest LoadManifest(string projectPath)
    {
        if (File.Exists(_manifestPath))
        {
            try
            {
                string json = File.ReadAllText(_manifestPath);
                CacheManifest? manifest = JsonSerializer.Deserialize<CacheManifest>(json);
                if (manifest != null)
                {
                    _logger.LogDebug("Cache loaded: {AnalysisCount} analysis, {QueryCount} queries",
                        manifest.AnalysisEntries.Count, manifest.QueryEntries.Count);
                    return manifest;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load cache");
            }
        }

        return new CacheManifest { ProjectPath = projectPath, LastUpdate = DateTime.UtcNow };
    }

    public async Task<(bool Hit, T? Result)> TryGetAnalysisAsync<T>(string filePath, string analysisType) where T : class
    {
        if (!_cacheOptions.EnableAnalysisCache)
            return (false, null);

        string fileHash = await ComputeFileHashAsync(filePath);
        string key = $"{filePath}:{analysisType}";

        if (_manifest.AnalysisEntries.TryGetValue(key, out AnalysisCacheEntry? entry) && entry.FileHash == fileHash)
        {
            _manifest.Statistics.AnalysisHits++;
            _isDirty = true;
            _logger.LogDebug("Cache HIT: {FileName} ({AnalysisType})", Path.GetFileName(filePath), analysisType);

            try
            {
                return (true, JsonSerializer.Deserialize<T>(entry.Result));
            }
            catch
            {
                return (false, null);
            }
        }

        _manifest.Statistics.AnalysisMisses++;
        _isDirty = true;
        return (false, null);
    }

    public async Task SetAnalysisAsync<T>(string filePath, string analysisType, T result) where T : class
    {
        if (!_cacheOptions.EnableAnalysisCache)
            return;

        string fileHash = await ComputeFileHashAsync(filePath);
        string key = $"{filePath}:{analysisType}";
        string json = JsonSerializer.Serialize(result);

        _manifest.AnalysisEntries[key] = new AnalysisCacheEntry
        {
            FilePath = filePath,
            FileHash = fileHash,
            AnalysisType = analysisType,
            Result = json,
            Timestamp = DateTime.UtcNow
        };

        _manifest.Statistics.TotalSize += json.Length;
        _isDirty = true;
        _logger.LogDebug("Cached: {FileName} ({AnalysisType})", Path.GetFileName(filePath), analysisType);
    }

    public (bool Hit, string? Response) TryGetQuery(string prompt, string specialistId)
    {
        if (!_cacheOptions.EnableQueryCache)
            return (false, null);

        string promptHash = ComputeStringHash(prompt);
        string key = $"{specialistId}:{promptHash}";

        if (_manifest.QueryEntries.TryGetValue(key, out QueryCacheEntry? entry))
        {
            if (DateTime.UtcNow - entry.Timestamp < _cacheOptions.QueryCacheExpiration)
            {
                _manifest.Statistics.QueryHits++;
                _isDirty = true;
                _logger.LogDebug("Query cache HIT: {SpecialistId}", specialistId);
                return (true, entry.Response);
            }

            _manifest.QueryEntries.Remove(key);
        }

        _manifest.Statistics.QueryMisses++;
        _isDirty = true;
        return (false, null);
    }

    public void SetQuery(string prompt, string specialistId, string response)
    {
        if (!_cacheOptions.EnableQueryCache)
            return;

        string promptHash = ComputeStringHash(prompt);
        string key = $"{specialistId}:{promptHash}";

        _manifest.QueryEntries[key] = new QueryCacheEntry
        {
            PromptHash = promptHash,
            SpecialistId = specialistId,
            Response = response,
            Timestamp = DateTime.UtcNow,
            TokenCount = response.Length / 4
        };

        _manifest.Statistics.TotalSize += response.Length;
        _isDirty = true;
        _logger.LogDebug("Query cached: {SpecialistId}", specialistId);
    }

    public void Clear(bool analysisOnly = false, bool queriesOnly = false)
    {
        if (!queriesOnly)
        {
            _manifest.AnalysisEntries.Clear();
            _logger.LogInformation("Analysis cache cleared");
        }

        if (!analysisOnly)
        {
            _manifest.QueryEntries.Clear();
            _logger.LogInformation("Query cache cleared");
        }

        _manifest.Statistics = new CacheStatistics();
        _isDirty = true;
    }

    public CacheStatistics GetStatistics() => _manifest.Statistics;

    public void PrintStatistics()
    {
        CacheStatistics stats = _manifest.Statistics;
        Console.WriteLine("Cache Statistics:");
        Console.WriteLine($"  Analysis: {_manifest.AnalysisEntries.Count} entries, {stats.AnalysisHitRate:P0} hit rate");
        Console.WriteLine($"  Queries:  {_manifest.QueryEntries.Count} entries, {stats.QueryHitRate:P0} hit rate");
        Console.WriteLine($"  Size:     {FormatSize(stats.TotalSize)}");
    }

    private static string FormatSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:F1} {sizes[order]}";
    }

    private static async Task<string> ComputeFileHashAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return string.Empty;

        await using FileStream stream = File.OpenRead(filePath);
        byte[] hash = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash);
    }

    private static string ComputeStringHash(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16];
    }

    public void Save()
    {
        if (!_isDirty)
            return;

        try
        {
            _manifest.LastUpdate = DateTime.UtcNow;
            string json = JsonSerializer.Serialize(_manifest, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_manifestPath, json);
            _isDirty = false;
            _logger.LogDebug("Cache saved");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save cache");
        }
    }

    public void Dispose() => Save();
}