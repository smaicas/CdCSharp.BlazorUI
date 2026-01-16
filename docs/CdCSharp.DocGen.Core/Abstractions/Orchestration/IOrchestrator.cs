using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Orchestration;

namespace CdCSharp.DocGen.Core.Abstractions.Orchestration;

public interface IOrchestrator
{
    Task<OrchestrationPlan> CreatePlanAsync(ProjectStructure structure, Dictionary<string, DestructuredAssembly> destructured);
}