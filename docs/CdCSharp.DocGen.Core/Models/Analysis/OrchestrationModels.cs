using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Models.Orchestration;

public record OrchestrationPlan
{
    [JsonPropertyName("projectType")]
    public string ProjectType { get; init; } = string.Empty;

    [JsonPropertyName("criticalContext")]
    public string CriticalContext { get; init; } = string.Empty;

    [JsonPropertyName("specialists")]
    public List<SpecialistTask> Specialists { get; init; } = [];

    [JsonPropertyName("outputSections")]
    public List<DocumentSection> OutputSections { get; init; } = [];

    [JsonPropertyName("keyFiles")]
    public List<string> KeyFiles { get; init; } = [];
}

public record SpecialistTask
{
    [JsonPropertyName("specialistId")]
    public string SpecialistId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("focus")]
    public string Focus { get; init; } = string.Empty;

    [JsonPropertyName("targetSections")]
    public List<string> TargetSections { get; init; } = [];

    [JsonPropertyName("prompts")]
    public List<SpecialistPrompt> Prompts { get; init; } = [];

    [JsonPropertyName("priority")]
    public int Priority { get; init; }
}

public record RequiredFiles
{
    [JsonPropertyName("destructured")]
    public List<string> Destructured { get; init; } = [];

    [JsonPropertyName("fullContent")]
    public List<string> FullContent { get; init; } = [];
}

public record SpecialistPrompt
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("instruction")]
    public string Instruction { get; init; } = string.Empty;

    [JsonPropertyName("expectedOutput")]
    public string ExpectedOutput { get; init; } = string.Empty;

    [JsonPropertyName("maxTokens")]
    public int MaxTokens { get; init; } = 2000;

    [JsonPropertyName("requiredFiles")]
    public RequiredFiles RequiredFiles { get; init; } = new();

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

public record SpecialistDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("defaultFocus")]
    public string DefaultFocus { get; init; } = string.Empty;

    [JsonPropertyName("capabilities")]
    public List<string> Capabilities { get; init; } = [];

    [JsonPropertyName("systemPrompt")]
    public string SystemPrompt { get; init; } = string.Empty;

    [JsonPropertyName("isBuiltIn")]
    public bool IsBuiltIn { get; init; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record SpecialistResult
{
    [JsonPropertyName("specialistId")]
    public string SpecialistId { get; init; } = string.Empty;

    [JsonPropertyName("promptId")]
    public string PromptId { get; init; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    [JsonPropertyName("targetSections")]
    public List<string> TargetSections { get; init; } = [];

    [JsonPropertyName("executedAt")]
    public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("tokenCount")]
    public int TokenCount { get; init; }
}