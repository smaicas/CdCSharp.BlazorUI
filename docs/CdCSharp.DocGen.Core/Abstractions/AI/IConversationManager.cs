namespace CdCSharp.DocGen.Core.Abstractions.AI;

public interface IConversationManager
{
    IConversation CreateConversation(string conversationId, string systemPrompt);
    IConversation? GetConversation(string conversationId);
    void ClearAll();
}

public interface IConversation
{
    string Id { get; }
    string SystemPrompt { get; }
    IReadOnlyList<ChatMessage> Messages { get; }

    void AddContext(string context);
    Task<string> SendAsync(string userMessage, int maxTokens = 2000);
    void Clear();
}

public record ChatMessage(string Role, string Content);
