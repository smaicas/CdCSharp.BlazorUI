using CdCSharp.DocGen.Core.Models;
using System.Text;

namespace CdCSharp.DocGen.Core.Formatting;

/// <summary>
/// Formatter ultra-optimizado para minimizar tokens en documentación LLM
/// Usa codificación compacta y elimina redundancias
/// </summary>
public class OptimizedFormatter : IProjectFormatter
{
    public string FormatStructure(ProjectStructure structure)
    {
        StringBuilder sb = new();

        // Cabecera compacta
        sb.Append($"P:{structure.Solution}|T:{structure.GlobalSummary.ProjectType}|");
        sb.Append($"A:{structure.GlobalSummary.TotalAssemblies}");

        if (structure.GlobalSummary.TotalTestProjects > 0)
            sb.Append($"({structure.GlobalSummary.TotalTestProjects}t)");

        sb.Append($"|C:{structure.GlobalSummary.TotalClasses}|");
        sb.Append($"I:{structure.GlobalSummary.TotalInterfaces}|");
        sb.Append($"R:{structure.GlobalSummary.TotalRecords}");

        if (structure.GlobalSummary.TotalComponents > 0)
            sb.Append($"|BC:{structure.GlobalSummary.TotalComponents}");
        if (structure.GlobalSummary.TotalGenerators > 0)
            sb.Append($"|SG:{structure.GlobalSummary.TotalGenerators}");

        sb.AppendLine();

        // Patrones detectados (solo si hay)
        if (structure.GlobalSummary.DetectedPatterns.Count > 0)
            sb.AppendLine($"PTN:{string.Join(",", structure.GlobalSummary.DetectedPatterns)}");

        // Assemblies (solo no-test)
        sb.AppendLine("ASM:");
        foreach (AssemblyInfo? asm in structure.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.Append($"{asm.Name}|");
            FormatAssemblySummary(sb, asm.Summary);

            // Referencias clave (filtradas)
            List<string> keyRefs = asm.References
                .Where(r => !r.StartsWith("System") && !r.StartsWith("Microsoft.Extensions"))
                .Take(5)
                .ToList();

            if (keyRefs.Count > 0)
                sb.Append($"|REF:{string.Join(",", keyRefs)}");

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string FormatDestructured(DestructuredAssembly assembly)
    {
        StringBuilder sb = new();

        sb.AppendLine($"#{assembly.Assembly}");

        // Namespaces con tipos
        foreach (DestructuredNamespace ns in assembly.Namespaces)
        {
            sb.Append($"NS:{ns.Name}|");

            // Agrupar tipos por kind
            IEnumerable<IGrouping<TypeKind, DestructuredType>> typesByKind = ns.Types.GroupBy(t => t.Kind);
            IEnumerable<string> kindCounts = typesByKind.Select(g => $"{GetKindCode(g.Key)}:{g.Count()}");
            sb.AppendLine(string.Join(",", kindCounts));

            // Tipos importantes (públicos, interfaces, con attributes especiales)
            IEnumerable<DestructuredType> importantTypes = ns.Types
                .Where(t => t.Kind == TypeKind.Interface ||
                           t.Modifiers.Contains("public") ||
                           t.Attributes.Any(a => a.Contains("Generator") || a.Contains("Attribute")))
                .Take(10);

            foreach (DestructuredType? type in importantTypes)
            {
                FormatTypeCompact(sb, type);
            }
        }

        // Components (formato compacto)
        if (assembly.Components.Count > 0)
        {
            sb.AppendLine($"BC:{assembly.Components.Count}");
            foreach (DestructuredComponent? comp in assembly.Components.Take(20))
            {
                sb.Append($"{comp.Name}");

                if (comp.Parameters.Count > 0)
                {
                    string paramStr = string.Join(",", comp.Parameters
                        .Take(5)
                        .Select(p => $"{p.Name}:{CompactType(p.Type)}{(p.Required ? "!" : "")}"));
                    sb.Append($"[{paramStr}]");
                }

                if (comp.Injectables.Count > 0)
                {
                    sb.Append($"@{comp.Injectables.Count}");
                }

                sb.AppendLine();
            }
        }

        // TypeScript (ultra-compacto)
        if (assembly.TypeScript.Count > 0)
        {
            sb.AppendLine($"TS:{assembly.TypeScript.Count}");
            foreach (DestructuredTypeScript? ts in assembly.TypeScript.Take(10))
            {
                string exports = string.Join(",", ts.Exports.Take(5).Select(e =>
                    $"{GetTsKindCode(e.Kind)}{(e.IsDefault ? "*" : "")}{e.Name}"));
                sb.AppendLine($"{Path.GetFileName(ts.File)}[{exports}]");
            }
        }

        // CSS (mínimo)
        if (assembly.Css.Count > 0)
        {
            int totalVars = assembly.Css.Sum(c => c.Variables.Count);
            sb.AppendLine($"CSS:{assembly.Css.Count}|V:{totalVars}");
        }

        return sb.ToString();
    }

    private void FormatTypeCompact(StringBuilder sb, DestructuredType type)
    {
        string kind = GetKindCode(type.Kind);
        string mods = type.Modifiers.Count > 0 && type.Modifiers.Contains("public") ? "+" : "";

        sb.Append($" {kind}{mods}{type.Name}");

        // Base types (compacto)
        if (type.Base.Count > 0)
        {
            string bases = string.Join(",", type.Base.Select(CompactType).Take(3));
            sb.Append($":{bases}");
        }

        // Attributes importantes
        IEnumerable<string> keyAttrs = type.Attributes
            .Where(a => !a.Contains("System") && !a.Contains("CompilerGenerated"))
            .Take(2);
        if (keyAttrs.Any())
            sb.Append($"@{string.Join(",", keyAttrs)}");

        // Miembros (solo cuenta por tipo)
        List<string> memberCounts = type.Members
            .GroupBy(m => m.Kind)
            .Select(g => $"{GetMemberKindCode(g.Key)}:{g.Count()}")
            .ToList();

        if (memberCounts.Count > 0)
            sb.Append($"{{{string.Join(",", memberCounts)}}}");

        sb.AppendLine();
    }

    private void FormatAssemblySummary(StringBuilder sb, AssemblySummary summary)
    {
        sb.Append($"C:{summary.Classes}");
        if (summary.Interfaces > 0) sb.Append($"|I:{summary.Interfaces}");
        if (summary.Records > 0) sb.Append($"|R:{summary.Records}");
        if (summary.Structs > 0) sb.Append($"|S:{summary.Structs}");
        if (summary.Enums > 0) sb.Append($"|E:{summary.Enums}");
        if (summary.Components > 0) sb.Append($"|BC:{summary.Components}");
        if (summary.Generators > 0) sb.Append($"|SG:{summary.Generators}");
    }

    private string CompactType(string fullType)
    {
        // Eliminar System., Microsoft., etc.
        fullType = fullType
            .Replace("System.", "")
            .Replace("Microsoft.", "M.")
            .Replace("Collections.Generic.", "");

        // Acortar genéricos comunes
        fullType = fullType
            .Replace("List<", "L<")
            .Replace("Dictionary<", "D<")
            .Replace("IEnumerable<", "IE<")
            .Replace("Task<", "T<");

        return fullType;
    }

    private string GetKindCode(TypeKind kind) => kind switch
    {
        TypeKind.Class => "C",
        TypeKind.Interface => "I",
        TypeKind.Record => "R",
        TypeKind.Struct => "S",
        TypeKind.Enum => "E",
        TypeKind.Delegate => "D",
        _ => "?"
    };

    private string GetMemberKindCode(MemberKind kind) => kind switch
    {
        MemberKind.Constructor => "ct",
        MemberKind.Method => "m",
        MemberKind.Property => "p",
        MemberKind.Field => "f",
        MemberKind.Event => "e",
        MemberKind.Indexer => "i",
        _ => "?"
    };

    private string GetTsKindCode(TsExportKind kind) => kind switch
    {
        TsExportKind.Function => "f",
        TsExportKind.Class => "c",
        TsExportKind.Interface => "i",
        TsExportKind.Type => "t",
        TsExportKind.Const => "k",
        TsExportKind.Enum => "e",
        _ => "?"
    };
}

public interface IProjectFormatter
{
    string FormatStructure(ProjectStructure structure);
    string FormatDestructured(DestructuredAssembly assembly);
}

/// <summary>
/// Genera leyenda para el formato compacto
/// </summary>
public static class FormatLegend
{
    public static string Generate()
    {
        return @"=== FORMAT LEGEND ===
P=Project T=Type A=Assemblies C=Classes I=Interfaces R=Records S=Structs E=Enums
BC=BlazorComponents SG=SourceGenerators TS=TypeScript PTN=Patterns REF=References
NS=Namespace +=public @=Attributes
Members: ct=Constructor m=Method p=Property f=Field e=Event i=Indexer
Component: [name:type!] !=required @N=injectables
TypeScript: f=fn c=class i=interface t=type k=const e=enum *=default
Types: L=List D=Dictionary IE=IEnumerable T=Task M.=Microsoft.
=====================";
    }
}