using CdCSharp.DocGen.Core.Models.Analysis;

namespace CdCSharp.DocGen.Core.Abstractions.Formatting;

public interface IPlainTextFormatter
{
    string FormatStructure(ProjectStructure structure);
    string FormatDestructured(DestructuredAssembly assembly);
    string GetLegend();
}