using System.Text.Json;
using System.Text.Json.Serialization;

namespace CdCSharp.Theon.Core;

public sealed class LlmStructuredResponse
{
    [JsonPropertyName("thinking")]
    public string? Thinking { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("toolCalls")]
    public List<ToolCall>? ToolCalls { get; set; }

    [JsonPropertyName("generatedFiles")]
    public List<GeneratedFileOutput>? GeneratedFiles { get; set; }

    [JsonPropertyName("confidence")]
    public float? Confidence { get; set; }

    [JsonPropertyName("taskComplete")]
    public bool TaskComplete { get; set; }

    [JsonPropertyName("needMoreContext")]
    public string? NeedMoreContext { get; set; }
}

public sealed class ToolCall
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public JsonElement Parameters { get; set; }
}

public sealed class GeneratedFileOutput
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}