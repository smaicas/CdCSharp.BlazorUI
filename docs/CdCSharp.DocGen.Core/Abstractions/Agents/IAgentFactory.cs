using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Models.Agents;

public interface IAgentFactory
{
    IAgent Create(AgentDefinition definition);
    AgentDefinition BuildDefinition(AgentCreationRequest request);
    void SetQueryHandler(Func<AgentQuery, Task<AgentQueryResult>> handler);
}