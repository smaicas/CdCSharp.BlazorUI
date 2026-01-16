using CdCSharp.DocGen.Core.Models.Agents;

namespace CdCSharp.DocGen.Core.Abstractions.Agents;

public interface IAgentRegistry
{
    IReadOnlyList<AgentDefinition> GetAll();
    AgentDefinition? Get(string id);
    AgentDefinition? FindByExpertise(string expertise);
    void Register(AgentDefinition definition);
    void Remove(string id);
    string GetAgentListForPrompt();
}