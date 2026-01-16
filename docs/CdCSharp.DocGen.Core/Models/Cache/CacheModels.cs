using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Models.Cache;

public record CacheManifest
{
    [JsonPropertyName("version")]
    public string Version { get; init; } = "2.0";

    [JsonPropertyName("projectPath")]
    public string ProjectPath { get; init; } = string.Empty;

    [JsonPropertyName("lastUpdate")]
    public DateTime LastUpdate { get; set; }

    [JsonPropertyName("analysisEntries")]
    public Dictionary<string, AnalysisCacheEntry> AnalysisEntries { get; init; } = [];

    [JsonPropertyName("queryEntries")]
    public Dictionary<string, QueryCacheEntry> QueryEntries { get; init; } = [];

    [JsonPropertyName("statistics")]
    public CacheStatistics Statistics { get; set; } = new();
}

public record AnalysisCacheEntry
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; init; } = string.Empty;

    [JsonPropertyName("fileHash")]
    public string FileHash { get; init; } = string.Empty;

    [JsonPropertyName("analysisType")]
    public string AnalysisType { get; init; } = string.Empty;

    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }
}

public record QueryCacheEntry
{
    [JsonPropertyName("promptHash")]
    public string PromptHash { get; init; } = string.Empty;

    [JsonPropertyName("specialistId")]
    public string SpecialistId { get; init; } = string.Empty;

    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("tokenCount")]
    public int TokenCount { get; init; }
}

public record CacheStatistics
{
    [JsonPropertyName("analysisHits")]
    public int AnalysisHits { get; set; }

    [JsonPropertyName("analysisMisses")]
    public int AnalysisMisses { get; set; }

    [JsonPropertyName("queryHits")]
    public int QueryHits { get; set; }

    [JsonPropertyName("queryMisses")]
    public int QueryMisses { get; set; }

    [JsonPropertyName("totalSize")]
    public long TotalSize { get; set; }

    [JsonIgnore]
    public double AnalysisHitRate => AnalysisHits + AnalysisMisses > 0
        ? (double)AnalysisHits / (AnalysisHits + AnalysisMisses)
        : 0;

    [JsonIgnore]
    public double QueryHitRate => QueryHits + QueryMisses > 0
        ? (double)QueryHits / (QueryHits + QueryMisses)
        : 0;
}