using CdCSharp.Theon.Models;
using System.Text;

namespace CdCSharp.Theon.Analysis;

public class LlmFormatter
{
    public string FormatProjectStructure(ProjectStructure structure)
    {
        StringBuilder sb = new();

        sb.AppendLine($"PROJECT:{structure.Solution}|TYPE:{structure.Summary.ProjectType}");
        if (structure.Summary.DetectedPatterns.Count > 0)
            sb.AppendLine($"PATTERNS:{string.Join(",", structure.Summary.DetectedPatterns)}");
        sb.AppendLine();

        foreach (AssemblyStructure asm in structure.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"ASM:{asm.Name}|{asm.Path}");

            List<string> keyRefs = asm.References
                .Where(r => !r.StartsWith("System") && !r.StartsWith("Microsoft.Extensions"))
                .Take(5)
                .ToList();

            if (keyRefs.Count > 0)
                sb.AppendLine($"  REF:{string.Join(",", keyRefs)}");

            sb.AppendLine($"  FILES:CS={asm.Files.CSharp.Count},RZ={asm.Files.Razor.Count},TS={asm.Files.TypeScript.Count}");
        }

        return sb.ToString();
    }

    public string FormatAssemblyDetail(AssemblyStructure assembly)
    {
        StringBuilder sb = new();

        sb.AppendLine($"### {assembly.Name}");
        sb.AppendLine();

        foreach (NamespaceInfo ns in assembly.Namespaces)
        {
            int typeCount = ns.Types.Count;
            sb.AppendLine($"NS:{ns.Name}|{typeCount}types");

            foreach (TypeInfo type in ns.Types.Take(20))
            {
                sb.Append($"  {GetKindCode(type.Kind)}");
                sb.Append(type.Modifiers.Contains("public") ? "+" : "~");
                sb.Append($" {type.Name}");

                if (type.BaseTypes.Count > 0)
                    sb.Append($":{string.Join(",", type.BaseTypes.Take(2))}");

                if (type.Members.Count > 0)
                {
                    Dictionary<MemberKind, int> memberCounts = type.Members
                        .GroupBy(m => m.Kind)
                        .ToDictionary(g => g.Key, g => g.Count());

                    sb.Append(" {");
                    sb.Append(string.Join(",", memberCounts.Select(kvp =>
                        $"{GetMemberCode(kvp.Key)}:{kvp.Value}")));
                    sb.Append('}');
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public string FormatFileList(FileCollection files)
    {
        StringBuilder sb = new();

        if (files.CSharp.Count > 0)
        {
            sb.AppendLine("CS:");
            foreach (string f in files.CSharp) sb.AppendLine($"  {f}");
        }

        if (files.Razor.Count > 0)
        {
            sb.AppendLine("RAZOR:");
            foreach (string f in files.Razor) sb.AppendLine($"  {f}");
        }

        if (files.TypeScript.Count > 0)
        {
            sb.AppendLine("TS:");
            foreach (string f in files.TypeScript) sb.AppendLine($"  {f}");
        }

        return sb.ToString();
    }

    private static string GetKindCode(TypeKind kind) => kind switch
    {
        TypeKind.Class => "C",
        TypeKind.Interface => "I",
        TypeKind.Record => "R",
        TypeKind.Struct => "S",
        TypeKind.Enum => "E",
        TypeKind.Delegate => "D",
        _ => "?"
    };

    private static string GetMemberCode(MemberKind kind) => kind switch
    {
        MemberKind.Constructor => "ctor",
        MemberKind.Method => "m",
        MemberKind.Property => "p",
        MemberKind.Field => "f",
        MemberKind.Event => "e",
        _ => "?"
    };
}