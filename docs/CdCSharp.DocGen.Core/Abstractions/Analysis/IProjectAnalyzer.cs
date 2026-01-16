using CdCSharp.DocGen.Core.Models.Analysis;

namespace CdCSharp.DocGen.Core.Abstractions.Analysis;

public interface IProjectAnalyzer
{
    Task<AnalysisResult> AnalyzeAsync(string projectPath);
}

public record AnalysisResult
{
    public required ProjectStructure Structure { get; init; }
    public required Dictionary<string, DestructuredAssembly> Destructured { get; init; }
}