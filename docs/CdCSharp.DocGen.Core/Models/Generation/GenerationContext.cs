using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Orchestration;

namespace CdCSharp.DocGen.Core.Models.Generation;

public record GenerationContext
{
    public required ProjectStructure Structure { get; init; }
    public required Dictionary<string, DestructuredAssembly> Destructured { get; init; }
    public required OrchestrationPlan Plan { get; init; }
    public required List<AgentResult> Results { get; init; }
}