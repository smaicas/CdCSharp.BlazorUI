// Models/AI/AiResponse.cs
using System.Text.Json.Serialization;

namespace CdCSharp.DocGen.Core.Models.AI;

public record ChatMessage(string Role, string Content);

public record AiResponse
{
    public bool Success { get; init; }
    public string Content { get; init; } = string.Empty;
    public AiError? Error { get; init; }
    public AiMetrics Metrics { get; init; } = new();

    public static AiResponse Ok(string content, int estimatedTokens = 0) => new()
    {
        Success = true,
        Content = content,
        Metrics = new AiMetrics
        {
            EstimatedInputTokens = 0,
            EstimatedOutputTokens = estimatedTokens > 0 ? estimatedTokens : content.Length / 4
        }
    };

    public static AiResponse Fail(AiErrorType type, string message) => new()
    {
        Success = false,
        Error = new AiError(type, message)
    };
}

public record AiError(AiErrorType Type, string Message);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AiErrorType
{
    RateLimit,
    Timeout,
    InvalidResponse,
    ConnectionError,
    Unknown
}

public record AiMetrics
{
    public int EstimatedInputTokens { get; init; }
    public int EstimatedOutputTokens { get; init; }
    public double LatencySeconds { get; init; }
}