using CdCSharp.DocGen.Core.Abstractions.AI;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace CdCSharp.DocGen.Core.AI;

public class ConversationManager : IConversationManager
{
    private readonly ConcurrentDictionary<string, Conversation> _conversations = new();
    private readonly IAiClient _aiClient;
    private readonly ILogger<ConversationManager> _logger;
    private readonly ConversationOptions _options;

    public ConversationManager(
        IAiClient aiClient,
        IOptions<DocGenOptions> docGenOptions,
        ILogger<ConversationManager> logger)
    {
        _aiClient = aiClient;
        _logger = logger;
        _options = docGenOptions.Value.Conversation;
    }

    public IConversation CreateConversation(string conversationId, string systemPrompt)
    {
        Conversation conversation = new(conversationId, systemPrompt, _aiClient, _logger, _options);
        _conversations[conversationId] = conversation;
        _logger.LogDebug("Created conversation: {Id}", conversationId);
        return conversation;
    }

    public IConversation? GetConversation(string conversationId)
    {
        return _conversations.TryGetValue(conversationId, out Conversation? conv) ? conv : null;
    }

    public void ClearAll()
    {
        _conversations.Clear();
        _logger.LogDebug("All conversations cleared");
    }
}

internal class Conversation : IConversation
{
    private readonly List<ChatMessage> _anchorMessages = [];
    private readonly List<ChatMessage> _historyMessages = [];
    private string? _compressionSummary;

    private readonly IAiClient _aiClient;
    private readonly ILogger _logger;
    private readonly ConversationOptions _options;

    public string Id { get; }
    public string SystemPrompt { get; }
    public IReadOnlyList<ChatMessage> Messages => BuildMessageList();

    public Conversation(
        string id,
        string systemPrompt,
        IAiClient aiClient,
        ILogger logger,
        ConversationOptions options)
    {
        Id = id;
        SystemPrompt = systemPrompt;
        _aiClient = aiClient;
        _logger = logger;
        _options = options;

        _anchorMessages.Add(new ChatMessage("system", systemPrompt));
    }

    public void AddContext(string context)
    {
        _anchorMessages.Add(new ChatMessage("user", $"Project context:\n\n{context}"));
        _anchorMessages.Add(new ChatMessage("assistant", "I've reviewed the project context. Ready to help document this project."));

        _logger.LogDebug("Context added to conversation {Id}: {Length} chars", Id, context.Length);
    }

    public async Task<string> SendAsync(string userMessage, int maxTokens = 2000)
    {
        await CompressIfNeededAsync();

        _historyMessages.Add(new ChatMessage("user", userMessage));

        List<ChatMessage> messages = BuildMessageList();

        _logger.LogDebug("Conversation {Id}: sending {Count} messages (~{Tokens} tokens)",
            Id, messages.Count, EstimateTokens(messages));

        string response = await _aiClient.SendMessagesAsync(messages, maxTokens);

        if (!string.IsNullOrWhiteSpace(response))
        {
            _historyMessages.Add(new ChatMessage("assistant", response));
        }

        return response;
    }

    public void Clear()
    {
        _historyMessages.Clear();
        _compressionSummary = null;
    }

    private List<ChatMessage> BuildMessageList()
    {
        List<ChatMessage> messages = [.. _anchorMessages];

        if (!string.IsNullOrEmpty(_compressionSummary))
        {
            messages.Add(new ChatMessage("user", $"Summary of our previous work:\n{_compressionSummary}"));
            messages.Add(new ChatMessage("assistant", "Understood. I'll continue from where we left off."));
        }

        int windowStart = Math.Max(0, _historyMessages.Count - _options.SlidingWindowSize);
        messages.AddRange(_historyMessages.Skip(windowStart));

        return messages;
    }

    private async Task CompressIfNeededAsync()
    {
        int messagesToCompress = _historyMessages.Count - _options.SlidingWindowSize;

        if (messagesToCompress < _options.CompressionThreshold)
            return;

        _logger.LogDebug("Conversation {Id}: compressing {Count} messages", Id, messagesToCompress);

        List<ChatMessage> oldMessages = _historyMessages.Take(messagesToCompress).ToList();

        string compressionPrompt = BuildCompressionPrompt(oldMessages);
        string newSummary = await _aiClient.SendAsync(compressionPrompt, maxTokens: 500);

        if (!string.IsNullOrWhiteSpace(newSummary))
        {
            _compressionSummary = string.IsNullOrEmpty(_compressionSummary)
                ? newSummary
                : $"{_compressionSummary}\n\n{newSummary}";

            _historyMessages.RemoveRange(0, messagesToCompress);

            _logger.LogDebug("Conversation {Id}: compressed to summary ({Length} chars)",
                Id, _compressionSummary.Length);
        }
    }

    private static string BuildCompressionPrompt(List<ChatMessage> messages)
    {
        string conversation = string.Join("\n\n", messages.Select(m => $"[{m.Role}]: {m.Content}"));

        return $"""
            Summarize the key points from this conversation in 3-5 concise bullets.
            Focus on: what was documented, decisions made, important findings.
            
            Conversation:
            {conversation}
            
            Summary (bullets only, no preamble):
            """;
    }

    private static int EstimateTokens(List<ChatMessage> messages)
    {
        return messages.Sum(m => m.Content.Length) / 4;
    }
}