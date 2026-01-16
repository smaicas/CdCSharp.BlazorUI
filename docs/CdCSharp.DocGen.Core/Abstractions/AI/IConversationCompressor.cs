using CdCSharp.DocGen.Core.Models.AI;

public interface IConversationCompressor
{
    Task<string> CompressMessagesAsync(IReadOnlyList<ChatMessage> messages);

    ConversationWindow BuildWindow(
        IReadOnlyList<ChatMessage> anchorMessages,
        IReadOnlyList<ChatMessage> historyMessages,
        string? existingSummary,
        int windowSize,
        int compressionThreshold);
}

public record ConversationWindow
{
    public required IReadOnlyList<ChatMessage> EffectiveMessages { get; init; }
    public required string? UpdatedSummary { get; init; }
    public required int MessagesToRemove { get; init; }
    public required bool RequiresCompression { get; init; }
}