using System.Text.Json.Serialization;

namespace CdCSharp.Theon.Context;

/// <summary>
/// Standard response format for context queries.
/// </summary>
public sealed class ContextInfoResponse
{
    [JsonPropertyName("answer")]
    public string Answer { get; init; } = string.Empty;

    [JsonPropertyName("files_examined")]
    public List<string> FilesExamined { get; init; } = [];

    [JsonPropertyName("confidence")]
    public float Confidence { get; init; }
}

/// <summary>
/// Helper to build response format schemas for different response types.
/// </summary>
public static class ResponseSchemas
{
    public static object GetSchemaFor<TResponse>()
    {
        if (typeof(TResponse) == typeof(ContextInfoResponse))
        {
            return new
            {
                type = "object",
                properties = new
                {
                    answer = new
                    {
                        type = "string",
                        description = "Detailed answer to the question"
                    },
                    files_examined = new
                    {
                        type = "array",
                        items = new { type = "string" },
                        description = "List of files that were examined"
                    },
                    confidence = new
                    {
                        type = "number",
                        description = "Confidence level from 0.0 to 1.0"
                    }
                },
                required = new[] { "answer", "files_examined", "confidence" },
                additionalProperties = false
            };
        }

        // Default schema for unknown types
        return new
        {
            type = "object",
            properties = new
            {
                result = new { type = "string" }
            },
            required = new[] { "result" }
        };
    }
}