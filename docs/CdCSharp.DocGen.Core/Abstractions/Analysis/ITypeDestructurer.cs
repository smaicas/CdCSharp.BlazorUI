using CdCSharp.DocGen.Core.Models.Analysis;

namespace CdCSharp.DocGen.Core.Abstractions.Analysis;

public interface ITypeDestructurer
{
    Task<List<DestructuredNamespace>> DestructureAsync(string rootPath, List<string> csFiles);
}