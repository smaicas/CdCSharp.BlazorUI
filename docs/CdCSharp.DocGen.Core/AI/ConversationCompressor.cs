using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Models.AI;
using Microsoft.Extensions.Logging;

public class ConversationCompressor : IConversationCompressor
{
    private readonly IAiClient _aiClient;
    private readonly ILogger<ConversationCompressor> _logger;

    public ConversationCompressor(IAiClient aiClient, ILogger<ConversationCompressor> logger)
    {
        _aiClient = aiClient;
        _logger = logger;
    }

    public async Task<string> CompressMessagesAsync(IReadOnlyList<ChatMessage> messages)
    {
        if (messages.Count == 0)
            return string.Empty;

        string conversation = string.Join("\n\n", messages.Select(m => $"[{m.Role}]: {m.Content}"));

        string prompt = $"""
            Summarize the key points from this conversation in 3-5 concise bullets.
            Focus on: what was documented, decisions made, important findings.
            Do not include any preamble, just the bullets.
            
            Conversation:
            {conversation}
            
            Summary:
            """;

        string summary = await _aiClient.SendAsync(prompt, maxTokens: 500);

        _logger.LogDebug("Compressed {Count} messages to summary ({Length} chars)",
            messages.Count, summary.Length);

        return summary;
    }

    public ConversationWindow BuildWindow(
        IReadOnlyList<ChatMessage> anchorMessages,
        IReadOnlyList<ChatMessage> historyMessages,
        string? existingSummary,
        int windowSize,
        int compressionThreshold)
    {
        int messagesToCompress = historyMessages.Count - windowSize;
        bool requiresCompression = messagesToCompress >= compressionThreshold;

        List<ChatMessage> effectiveMessages = [.. anchorMessages];

        if (!string.IsNullOrEmpty(existingSummary))
        {
            effectiveMessages.Add(new ChatMessage("user", $"Summary of our previous work:\n{existingSummary}"));
            effectiveMessages.Add(new ChatMessage("assistant", "Understood. I'll continue from where we left off."));
        }

        int windowStart = Math.Max(0, historyMessages.Count - windowSize);
        effectiveMessages.AddRange(historyMessages.Skip(windowStart));

        return new ConversationWindow
        {
            EffectiveMessages = effectiveMessages,
            UpdatedSummary = existingSummary,
            MessagesToRemove = requiresCompression ? messagesToCompress : 0,
            RequiresCompression = requiresCompression
        };
    }
}