using CdCSharp.DocGen.Core.Models.Agents;
using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Models.Orchestration;

public record OrchestrationPlan
{
    [JsonPropertyName("projectType")]
    public string ProjectType { get; init; } = string.Empty;

    [JsonPropertyName("criticalContext")]
    public string CriticalContext { get; init; } = string.Empty;

    [JsonPropertyName("tasks")]
    public List<AgentTask> Tasks { get; init; } = [];

    [JsonPropertyName("outputSections")]
    public List<DocumentSection> OutputSections { get; init; } = [];

    [JsonPropertyName("keyFiles")]
    public List<string> KeyFiles { get; init; } = [];
}

public record DocumentSection
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
}