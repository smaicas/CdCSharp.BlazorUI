using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Orchestration;

namespace CdCSharp.DocGen.Core.Abstractions.Orchestration;

public interface ISpecialistRunner
{
    Task<List<SpecialistResult>> ExecuteAllAsync(OrchestrationPlan plan, Dictionary<string, DestructuredAssembly> destructured);
}