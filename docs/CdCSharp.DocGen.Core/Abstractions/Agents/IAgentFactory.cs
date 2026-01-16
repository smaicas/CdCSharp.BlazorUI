using CdCSharp.DocGen.Core.Models.Agents;

namespace CdCSharp.DocGen.Core.Abstractions.Agents;

public interface IAgentFactory
{
    IAgent Create(AgentDefinition definition);
    AgentDefinition BuildDefinition(AgentCreationRequest request);
}