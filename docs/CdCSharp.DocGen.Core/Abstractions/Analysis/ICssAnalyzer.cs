using CdCSharp.DocGen.Core.Models.Analysis;

namespace CdCSharp.DocGen.Core.Abstractions.Analysis;

public interface ICssAnalyzer
{
    Task<List<DestructuredCss>> AnalyzeAsync(string rootPath, List<string> cssFiles);
}