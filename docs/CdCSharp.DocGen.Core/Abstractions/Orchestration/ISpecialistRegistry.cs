using CdCSharp.DocGen.Core.Models.Orchestration;

namespace CdCSharp.DocGen.Core.Abstractions.Orchestration;

public interface ISpecialistRegistry
{
    IReadOnlyList<SpecialistDefinition> GetAll();
    SpecialistDefinition? Get(string id);
    void Register(SpecialistDefinition specialist);
    string GetSpecialistListForPrompt();
}