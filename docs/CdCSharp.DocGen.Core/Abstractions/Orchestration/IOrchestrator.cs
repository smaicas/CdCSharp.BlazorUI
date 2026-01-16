using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Orchestration;

namespace CdCSharp.DocGen.Core.Abstractions.Agents;

public interface IOrchestrator
{
    IReadOnlyList<AgentMessage> ConversationHistory { get; }
    IReadOnlyDictionary<string, IAgent> ActiveAgents { get; }

    Task<OrchestrationPlan> CreatePlanAsync(
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured);

    Task<List<AgentResult>> ExecutePlanAsync(
        OrchestrationPlan plan,
        Dictionary<string, DestructuredAssembly> destructured);

    Task<AgentQueryResult> HandleAgentQueryAsync(AgentQuery query);

    Task<IAgent> GetOrCreateAgentAsync(
        string agentId,
        AgentCreationRequest? creationRequest = null,
        AgentTask? taskContext = null);
}