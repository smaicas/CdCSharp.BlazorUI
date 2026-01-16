using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Analysis;

namespace CdCSharp.DocGen.Core.Abstractions.Agents;

public interface IExpertiseContextBuilder
{
    Task<string> BuildContextAsync(
        AgentExpertise expertise,
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured);
}