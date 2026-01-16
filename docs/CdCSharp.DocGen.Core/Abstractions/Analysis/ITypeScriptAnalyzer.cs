using CdCSharp.DocGen.Core.Models.Analysis;

namespace CdCSharp.DocGen.Core.Abstractions.Analysis;

public interface ITypeScriptAnalyzer
{
    Task<List<DestructuredTypeScript>> AnalyzeAsync(string rootPath, List<string> tsFiles);
}