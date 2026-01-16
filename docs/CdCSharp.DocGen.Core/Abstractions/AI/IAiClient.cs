using CdCSharp.DocGen.Core.AI;

namespace CdCSharp.DocGen.Core.Abstractions.AI;

public interface IAiClient : IDisposable
{
    Task<string> SendAsync(string prompt, int maxTokens = 2000, double temperature = 0.3);
    Task<string> SendMessagesAsync(IReadOnlyList<ChatMessage> messages, int maxTokens = 2000, double temperature = 0.3);
    Task<T?> SendAsync<T>(string prompt, int maxTokens = 2000, double temperature = 0.3) where T : class;
}