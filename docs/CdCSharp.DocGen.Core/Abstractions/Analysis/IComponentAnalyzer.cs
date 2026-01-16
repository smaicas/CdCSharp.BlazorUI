using CdCSharp.DocGen.Core.Models.Analysis;

namespace CdCSharp.DocGen.Core.Abstractions.Analysis;

public interface IComponentAnalyzer
{
    Task<List<DestructuredComponent>> AnalyzeAsync(string rootPath, List<string> razorFiles);
}