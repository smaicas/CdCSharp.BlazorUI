using CdCSharp.DocGen.Core.Models.Analysis;

namespace CdCSharp.DocGen.Core.Abstractions.Analysis;

public interface IAssemblyScanner
{
    Task<List<AssemblyInfo>> ScanAsync(string projectPath);
}