using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Models.Analysis;
using System.Text;
using System.Text.RegularExpressions;

namespace CdCSharp.DocGen.Core.Formatting;

public partial class PlainTextFormatter : IPlainTextFormatter
{
    public string FormatStructure(ProjectStructure structure)
    {
        StringBuilder sb = new();

        sb.AppendLine($"SLN:{structure.Solution}|TYP:{structure.Summary.ProjectType}");

        if (structure.Summary.DetectedPatterns.Count > 0)
            sb.AppendLine($"PTN:{string.Join(",", structure.Summary.DetectedPatterns)}");

        sb.AppendLine();

        foreach (AssemblyInfo asm in structure.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"ASM:{asm.Name}|{asm.Path}");

            List<string> keyRefs = asm.References
                .Where(r => !r.StartsWith("System") && !r.StartsWith("Microsoft.Extensions"))
                .Take(5)
                .ToList();

            if (keyRefs.Count > 0)
                sb.AppendLine($"  REF:{string.Join(",", keyRefs)}");
        }

        sb.AppendLine();
        sb.Append($"TOT:ASM:{structure.Summary.TotalAssemblies}");
        if (structure.Summary.TotalTestProjects > 0)
            sb.Append($"(TEST:{structure.Summary.TotalTestProjects})");
        sb.Append($"|C:{structure.Summary.TotalClasses}");
        sb.Append($"|I:{structure.Summary.TotalInterfaces}");
        sb.Append($"|R:{structure.Summary.TotalRecords}");
        sb.Append($"|BC:{structure.Summary.TotalComponents}");
        sb.Append($"|SG:{structure.Summary.TotalGenerators}");
        sb.AppendLine();

        return sb.ToString();
    }

    public string FormatDestructured(DestructuredAssembly assembly)
    {
        StringBuilder sb = new();

        sb.AppendLine($"### {assembly.Assembly}");
        sb.AppendLine();

        if (assembly.Namespaces.Count > 0)
        {
            sb.AppendLine(FormatNamespaces(assembly.Namespaces));
            sb.AppendLine();
        }

        if (assembly.Components.Count > 0)
        {
            sb.AppendLine(FormatComponents(assembly.Components));
            sb.AppendLine();
        }

        if (assembly.TypeScript.Count > 0)
        {
            sb.AppendLine(FormatTypeScript(assembly.TypeScript));
            sb.AppendLine();
        }

        if (assembly.Css.Count > 0)
        {
            sb.AppendLine(FormatCss(assembly.Css));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string GetLegend()
    {
        StringBuilder sb = new();

        sb.AppendLine("=== FORMAT LEGEND ===");
        sb.AppendLine(CSharpLegend);
        sb.AppendLine(BlazorLegend);
        sb.AppendLine(TypeScriptLegend);
        sb.AppendLine(CssLegend);
        sb.AppendLine("=====================");

        return sb.ToString();
    }

    private static string FormatNamespaces(List<DestructuredNamespace> namespaces)
    {
        StringBuilder sb = new();

        foreach (DestructuredNamespace ns in namespaces)
        {
            Dictionary<TypeKind, int> typeCounts = ns.Types
                .GroupBy(t => t.Kind)
                .ToDictionary(g => g.Key, g => g.Count());

            List<string> counts = typeCounts
                .Select(kvp => $"{GetKindCode(kvp.Key)}:{kvp.Value}")
                .ToList();

            sb.AppendLine($"NS:{ns.Name}|{string.Join(",", counts)}");

            IEnumerable<DestructuredType> importantTypes = ns.Types
                .Where(t => t.Modifiers.Contains("public") ||
                           t.Kind == TypeKind.Interface ||
                           t.Attributes.Any(a => !a.Contains("CompilerGenerated")))
                .Take(20);

            foreach (DestructuredType type in importantTypes)
            {
                FormatType(sb, type);
            }
        }

        return sb.ToString();
    }

    private static void FormatType(StringBuilder sb, DestructuredType type)
    {
        sb.Append($"  {GetKindCode(type.Kind)}");

        if (type.Modifiers.Contains("public"))
            sb.Append('+');
        else if (type.Modifiers.Contains("internal"))
            sb.Append('~');

        if (type.Modifiers.Contains("sealed"))
            sb.Append('!');
        if (type.Modifiers.Contains("abstract"))
            sb.Append('*');
        if (type.Modifiers.Contains("static"))
            sb.Append('#');

        sb.Append($" {type.Name}");

        if (type.Base.Count > 0)
            sb.Append($":{string.Join(",", type.Base.Take(3))}");

        List<string> keyAttrs = type.Attributes
            .Where(a => !a.Contains("System") && !a.Contains("CompilerGenerated"))
            .Take(2)
            .ToList();
        if (keyAttrs.Count > 0)
            sb.Append($"@{string.Join(",", keyAttrs)}");

        if (type.Members.Count > 0)
        {
            sb.Append(" {");

            Dictionary<MemberKind, List<DestructuredMember>> membersByKind = type.Members
                .GroupBy(m => m.Kind)
                .ToDictionary(g => g.Key, g => g.ToList());

            bool first = true;
            foreach ((MemberKind kind, List<DestructuredMember> members) in membersByKind)
            {
                if (!first) sb.Append(';');
                first = false;

                int shown = Math.Min(members.Count, 8);
                for (int i = 0; i < shown; i++)
                {
                    if (i > 0) sb.Append(',');
                    FormatMember(sb, members[i]);
                }

                if (members.Count > shown)
                    sb.Append($",+{members.Count - shown}");
            }

            sb.Append('}');
        }

        sb.AppendLine();

        foreach (DestructuredType nested in type.NestedTypes)
        {
            sb.Append("  ");
            FormatType(sb, nested);
        }
    }

    private static void FormatMember(StringBuilder sb, DestructuredMember member)
    {
        sb.Append(GetMemberCode(member.Kind));
        sb.Append(':');

        string signature = member.Signature;

        if (member.Kind == MemberKind.Property)
            signature = PropertyAccessorsRegex().Replace(signature, "");

        signature = CompactTypes(signature);

        sb.Append(signature);

        if (member.Attributes.Any(a => a.Contains("Required") || a.Contains("Obsolete")))
            sb.Append($"@{string.Join(",", member.Attributes.Take(1))}");
    }

    private static string CompactTypes(string signature)
    {
        return signature
            .Replace("System.", "")
            .Replace("Collections.Generic.", "")
            .Replace("List<", "L<")
            .Replace("Dictionary<", "D<")
            .Replace("IEnumerable<", "IE<")
            .Replace("Task<", "T<")
            .Replace("string", "str")
            .Replace("bool", "b")
            .Replace("int", "i");
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
        MemberKind.Constructor => "ct",
        MemberKind.Method => "m",
        MemberKind.Property => "p",
        MemberKind.Field => "f",
        MemberKind.Event => "e",
        MemberKind.Indexer => "ix",
        _ => "?"
    };

    [GeneratedRegex(@"\s*\{\s*get;\s*(?:set;|init;)?\s*\}", RegexOptions.Compiled)]
    private static partial Regex PropertyAccessorsRegex();

    private static string FormatComponents(List<DestructuredComponent> components)
    {
        StringBuilder sb = new();
        sb.AppendLine($"BC:{components.Count}");

        foreach (DestructuredComponent comp in components)
        {
            sb.Append($"  {comp.Name}");

            if (comp.Parameters.Count > 0)
            {
                List<string> pars = comp.Parameters
                    .Select(p => $"{p.Name}:{CompactComponentType(p.Type)}{(p.Required ? "!" : "")}")
                    .ToList();
                sb.Append($"[{string.Join(",", pars)}]");
            }

            if (comp.CascadingParameters.Count > 0)
                sb.Append($"^{comp.CascadingParameters.Count}");

            if (comp.Injectables.Count > 0)
                sb.Append($"@{comp.Injectables.Count}");

            if (comp.EventCallbacks.Count > 0)
                sb.Append($"E{comp.EventCallbacks.Count}");

            if (comp.RenderFragments.Count > 0)
                sb.Append($"R{comp.RenderFragments.Count}");

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string CompactComponentType(string type)
    {
        return type
            .Replace("EventCallback<", "EC<")
            .Replace("RenderFragment<", "RF<")
            .Replace("string", "str")
            .Replace("bool", "b")
            .Replace("int", "i");
    }

    private static string FormatTypeScript(List<DestructuredTypeScript> modules)
    {
        StringBuilder sb = new();
        sb.AppendLine($"TS:{modules.Count}");

        foreach (DestructuredTypeScript ts in modules.Take(15))
        {
            sb.Append($"  {Path.GetFileName(ts.File)}");

            if (ts.Exports.Count > 0)
            {
                List<string> exports = ts.Exports
                    .Take(5)
                    .Select(e => $"{GetTsExportCode(e.Kind)}{(e.IsDefault ? "*" : "")}{e.Name}")
                    .ToList();
                sb.Append($"[{string.Join(",", exports)}]");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetTsExportCode(TsExportKind kind) => kind switch
    {
        TsExportKind.Function => "f",
        TsExportKind.Class => "c",
        TsExportKind.Interface => "i",
        TsExportKind.Type => "t",
        TsExportKind.Const => "k",
        TsExportKind.Enum => "e",
        _ => "?"
    };

    private static string FormatCss(List<DestructuredCss> files)
    {
        StringBuilder sb = new();

        int totalVars = files.Sum(f => f.Variables.Count);
        int totalSelectors = files.Sum(f => f.Selectors.Count);

        sb.AppendLine($"CSS:{files.Count}|V:{totalVars}|S:{totalSelectors}");

        foreach (DestructuredCss css in files.Take(10))
        {
            sb.Append($"  {Path.GetFileName(css.File)}");

            if (css.Variables.Count > 0)
            {
                List<string> vars = css.Variables
                    .Take(3)
                    .Select(v => $"{v.Name}={v.Value}")
                    .ToList();
                sb.Append($"[{string.Join(",", vars)}]");

                if (css.Variables.Count > 3)
                    sb.Append($"+{css.Variables.Count - 3}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private const string CSharpLegend = """
        C# (.cs):
          C=Class I=Interface R=Record S=Struct E=Enum D=Delegate
          +=public ~=internal !=sealed *=abstract #=static
          Members: ct=ctor m=method p=property f=field e=event ix=indexer
          Types: L=List D=Dict IE=IEnumerable T=Task str=string b=bool i=int
        """;

    private const string BlazorLegend = """
        BLAZOR (.razor):
          BC=Blazor component
          [param:type!] = Parameters (!=required)
          ^N = N cascading parameters
          @N = N injected services
          EN = N event callbacks
          RN = N render fragments
        """;

    private const string TypeScriptLegend = """
        TYPESCRIPT (.ts, .tsx):
          TS=Typescript
          [exports] = f=function c=class i=interface t=type k=const e=enum
          * = default export
        """;

    private const string CssLegend = """
        CSS (.css, .scss, .less):
          CSS=File V=Variables S=Selectors
          [var=value,...] = First 3 variables
        """;
}