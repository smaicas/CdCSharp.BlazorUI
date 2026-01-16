namespace CdCSharp.Theon.Models;

public record ConversationMessage
{
    public required MessageRole Role { get; init; }
    public required string Content { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? FromAgentId { get; init; }
    public string? ToAgentId { get; init; }
}

public enum MessageRole { System, User, Assistant, AgentQuery, AgentResponse }

public record CompressedHistory
{
    public required string Summary { get; init; }
    public required List<ConversationMessage> RecentMessages { get; init; }
    public int OriginalMessageCount { get; init; }
    public DateTime CompressedAt { get; init; } = DateTime.UtcNow;
}