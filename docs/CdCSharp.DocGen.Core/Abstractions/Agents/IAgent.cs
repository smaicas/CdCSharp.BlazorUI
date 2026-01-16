// Abstractions/Agents/IAgent.cs
using CdCSharp.DocGen.Core.Models.Agents;

namespace CdCSharp.DocGen.Core.Abstractions.Agents;

public interface IAgent
{
    string Id { get; }
    string Name { get; }
    AgentDefinition Definition { get; }
    IReadOnlyList<AgentMessage> ConversationHistory { get; }

    void LoadExpertiseContext(string context);
    Task<string> ExecuteAsync(string instruction, int maxTokens = 2000);
    Task<string> ExecuteAsync(string instruction, int maxTokens, bool requiresMemory);
    Task<AgentQueryResult> QueryAsync(AgentQuery query);
    void ClearConversation();
}