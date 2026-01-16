using CdCSharp.DocGen.Core.Models;
using System.Text;
using System.Text.RegularExpressions;

public interface IProjectFormatter
{
    string FormatStructure(ProjectStructure structure);
    string FormatDestructured(DestructuredAssembly assembly);
}

public interface ITypeFormatter<T>
{
    string Format(T data);
    string GetLegend();
}

/// <summary>
/// Optimized plain text formatter designed for LLM consumption
/// Minimizes character count while maintaining readability
/// </summary>
public partial class PlainTextFormatter
{
    private readonly FormatterRegistry _registry = new();

    public PlainTextFormatter()
    {
        _registry.Register(new CSharpTypeFormatter());
        _registry.Register(new BlazorComponentFormatter());
        _registry.Register(new TypeScriptFormatter());
        _registry.Register(new CssFormatter());
    }

    public string FormatStructure(ProjectStructure structure)
    {
        StringBuilder sb = new();

        // Compact header
        sb.AppendLine($"SLN:{structure.Solution}|TYP:{structure.GlobalSummary.ProjectType}");

        // Detected patterns (if any)
        if (structure.GlobalSummary.DetectedPatterns.Count > 0)
            sb.AppendLine($"PTN:{string.Join(",", structure.GlobalSummary.DetectedPatterns)}");

        sb.AppendLine();

        // Assemblies (non-test only)
        foreach (AssemblyInfo asm in structure.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"ASM:{asm.Name}|{asm.Path}");

            //// Summary counts
            //List<string> counts = [];
            //if (asm.Summary.Classes > 0) counts.Add($"C:{asm.Summary.Classes}");
            //if (asm.Summary.Interfaces > 0) counts.Add($"I:{asm.Summary.Interfaces}");
            //if (asm.Summary.Records > 0) counts.Add($"R:{asm.Summary.Records}");
            //if (asm.Summary.Structs > 0) counts.Add($"S:{asm.Summary.Structs}");
            //if (asm.Summary.Enums > 0) counts.Add($"E:{asm.Summary.Enums}");
            //if (asm.Summary.Components > 0) counts.Add($"BC:{asm.Summary.Components}");
            //if (asm.Summary.Generators > 0) counts.Add($"SG:{asm.Summary.Generators}");
            //if (asm.Summary.TsModules > 0) counts.Add($"TS:{asm.Summary.TsModules}");
            //if (asm.Summary.CssFiles > 0) counts.Add($"CSS:{asm.Summary.CssFiles}");

            //if (counts.Count > 0)
            //    sb.AppendLine($"  {string.Join("|", counts)}");

            // Key references (filtered)
            List<string> keyRefs = asm.References
                .Where(r => !r.StartsWith("System") && !r.StartsWith("Microsoft.Extensions"))
                .Take(5)
                .ToList();

            if (keyRefs.Count > 0)
                sb.AppendLine($"  REF:{string.Join(",", keyRefs)}");
        }

        // Global totals
        sb.AppendLine();
        sb.Append($"TOT:ASM:{structure.GlobalSummary.TotalAssemblies}");
        if (structure.GlobalSummary.TotalTestProjects > 0)
            sb.Append($"(TEST:{structure.GlobalSummary.TotalTestProjects})");
        sb.Append($"TOT:C:{structure.GlobalSummary.TotalClasses}");
        sb.Append($"TOT:I:{structure.GlobalSummary.TotalInterfaces}");
        sb.Append($"TOT:R:{structure.GlobalSummary.TotalRecords}");
        sb.Append($"TOT:BC:{structure.GlobalSummary.TotalComponents}");
        sb.Append($"TOT:SG:{structure.GlobalSummary.TotalGenerators}");
        sb.Append($"TOT:TS:{structure.GlobalSummary.TotalTsModules}");
        sb.Append($"TOT:CSS:{structure.GlobalSummary.TotalCssFiles}");
        sb.AppendLine();

        return sb.ToString();
    }

    public string FormatDestructured(DestructuredAssembly assembly)
    {
        StringBuilder sb = new();

        sb.AppendLine($"### {assembly.Assembly}");
        sb.AppendLine();

        Try(sb, assembly.Namespaces);
        Try(sb, assembly.Components);
        Try(sb, assembly.TypeScript);
        Try(sb, assembly.Css);

        return sb.ToString();
    }

    private void Try<T>(StringBuilder sb, T data)
    {
        if (_registry.TryFormat(data, out string? text))
        {
            sb.Append(text);
            sb.AppendLine();
        }
    }

    public static string GetLegend()
    {
        PlainTextFormatter f = new();
        StringBuilder sb = new();

        sb.AppendLine("=== FORMAT LEGEND ===");
        foreach (string legend in f._registry.GetLegends())
            sb.AppendLine(legend);
        sb.AppendLine("=====================");

        return sb.ToString();
    }
}

/// <summary>
/// Registry for type-specific formatters
/// </summary>
public sealed class FormatterRegistry
{
    private readonly Dictionary<Type, Entry> _entries = [];

    private sealed class Entry
    {
        public required Func<object, string> Format;
        public required Func<string> Legend;
    }

    public void Register<T>(ITypeFormatter<T> formatter)
    {
        _entries[typeof(T)] = new Entry
        {
            Format = o => o is T t ? formatter.Format(t) : string.Empty,
            Legend = formatter.GetLegend
        };
    }

    public bool TryFormat<T>(T data, out string result)
    {
        if (_entries.TryGetValue(typeof(T), out Entry? entry))
        {
            result = entry.Format(data!);
            return true;
        }

        result = string.Empty;
        return false;
    }

    public IEnumerable<string> GetLegends()
        => _entries.Values.Select(e => e.Legend());
}

/// <summary>
/// C# type formatter - handles classes, interfaces, records, etc.
/// </summary>
public partial class CSharpTypeFormatter : ITypeFormatter<List<DestructuredNamespace>>
{
    private const int MaxMembersPerType = 8;

    public string[] SupportedExtensions => [".cs"];

    public string Format(List<DestructuredNamespace> namespaces)
    {
        StringBuilder sb = new();

        foreach (DestructuredNamespace ns in namespaces)
        {
            // Namespace header with type counts
            Dictionary<TypeKind, int> typeCounts = ns.Types
                .GroupBy(t => t.Kind)
                .ToDictionary(g => g.Key, g => g.Count());

            List<string> counts = typeCounts
                .Select(kvp => $"{GetKindCode(kvp.Key)}:{kvp.Value}")
                .ToList();

            sb.AppendLine($"NS:{ns.Name}|{string.Join(",", counts)}");

            // Format important types (public, interfaces, attributed)
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

    private void FormatType(StringBuilder sb, DestructuredType type)
    {
        // Type header: kind modifiers name :base @attrs
        sb.Append($"  {GetKindCode(type.Kind)}");

        if (type.Modifiers.Contains("public"))
            sb.Append("+");
        else if (type.Modifiers.Contains("internal"))
            sb.Append("~");

        if (type.Modifiers.Contains("sealed"))
            sb.Append("!");
        if (type.Modifiers.Contains("abstract"))
            sb.Append("*");
        if (type.Modifiers.Contains("static"))
            sb.Append("#");

        sb.Append($" {type.Name}");

        // Base types
        if (type.Base.Count > 0)
            sb.Append($":{string.Join(",", type.Base.Take(3))}");

        // Key attributes
        List<string> keyAttrs = type.Attributes
            .Where(a => !a.Contains("System") && !a.Contains("CompilerGenerated"))
            .Take(2)
            .ToList();
        if (keyAttrs.Count > 0)
            sb.Append($"@{string.Join(",", keyAttrs)}");

        // Members grouped by kind
        if (type.Members.Count > 0)
        {
            sb.Append(" {");

            Dictionary<MemberKind, List<DestructuredMember>> membersByKind = type.Members
                .GroupBy(m => m.Kind)
                .ToDictionary(g => g.Key, g => g.ToList());

            bool first = true;
            foreach ((MemberKind kind, List<DestructuredMember> members) in membersByKind)
            {
                if (!first) sb.Append(";");
                first = false;

                int shown = Math.Min(members.Count, MaxMembersPerType);
                for (int i = 0; i < shown; i++)
                {
                    if (i > 0) sb.Append(",");
                    FormatMember(sb, members[i]);
                }

                if (members.Count > shown)
                    sb.Append($",+{members.Count - shown}");
            }

            sb.Append("}");
        }

        sb.AppendLine();

        // Nested types (indented)
        foreach (DestructuredType nested in type.NestedTypes)
        {
            sb.Append("  ");
            FormatType(sb, nested);
        }
    }

    private void FormatMember(StringBuilder sb, DestructuredMember member)
    {
        sb.Append(GetMemberCode(member.Kind));
        sb.Append(":");

        string signature = member.Signature;

        // Clean property signatures
        if (member.Kind == MemberKind.Property)
            signature = PropertyAccessorsRegex().Replace(signature, "");

        // Compact common types
        signature = CompactTypes(signature);

        sb.Append(signature);

        // Key attributes
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

    public string GetLegend()
    {
        return @"C# (.cs):
  C=Class I=Interface R=Record S=Struct E=Enum D=Delegate
  +=public ~=internal !=sealed *=abstract #=static
  Members: ct=ctor m=method p=property f=field e=event ix=indexer
  Types: L=List D=Dict IE=IEnumerable T=Task str=string b=bool i=int
";
    }
}

/// <summary>
/// Blazor component formatter - optimized for component parameters
/// </summary>
public class BlazorComponentFormatter : ITypeFormatter<List<DestructuredComponent>>
{
    public string[] SupportedExtensions => [".razor"];

    public string Format(List<DestructuredComponent> components)
    {
        StringBuilder sb = new();
        sb.AppendLine($"BC:{components.Count}");

        foreach (DestructuredComponent comp in components)
        {
            sb.Append($"  {comp.Name}");

            // Parameters (ALWAYS show these)
            if (comp.Parameters.Count > 0)
            {
                List<string> pars = comp.Parameters
                    .Select(p => $"{p.Name}:{CompactType(p.Type)}{(p.Required ? "!" : "")}")
                    .ToList();
                sb.Append($"[{string.Join(",", pars)}]");
            }

            // Cascading parameters
            if (comp.CascadingParameters.Count > 0)
            {
                sb.Append($"^{comp.CascadingParameters.Count}");
            }

            // Injectables
            if (comp.Injectables.Count > 0)
            {
                sb.Append($"@{comp.Injectables.Count}");
            }

            // Event callbacks
            if (comp.EventCallbacks.Count > 0)
            {
                sb.Append($"E{comp.EventCallbacks.Count}");
            }

            // Render fragments
            if (comp.RenderFragments.Count > 0)
            {
                sb.Append($"R{comp.RenderFragments.Count}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string CompactType(string type)
    {
        return type
            .Replace("EventCallback<", "EC<")
            .Replace("RenderFragment<", "RF<")
            .Replace("string", "str")
            .Replace("bool", "b")
            .Replace("int", "i");
    }

    public string GetLegend()
    {
        return @"BLAZOR (.razor):
  BC=Blazor component
  [param:type!] = Parameters (!=required)
  ^N = N cascading parameters
  @N = N injected services
  EN = N event callbacks
  RN = N render fragments
";
    }
}

/// <summary>
/// TypeScript formatter
/// </summary>
public class TypeScriptFormatter : ITypeFormatter<List<DestructuredTypeScript>>
{
    public string[] SupportedExtensions => [".ts", ".tsx"];

    public string Format(List<DestructuredTypeScript> modules)
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
                    .Select(e => $"{GetExportCode(e.Kind)}{(e.IsDefault ? "*" : "")}{e.Name}")
                    .ToList();
                sb.Append($"[{string.Join(",", exports)}]");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetExportCode(TsExportKind kind) => kind switch
    {
        TsExportKind.Function => "f",
        TsExportKind.Class => "c",
        TsExportKind.Interface => "i",
        TsExportKind.Type => "t",
        TsExportKind.Const => "k",
        TsExportKind.Enum => "e",
        _ => "?"
    };

    public string GetLegend()
    {
        return @"TYPESCRIPT (.ts, .tsx):
  TS=Typescript
  [exports] = f=function c=class i=interface t=type k=const e=enum
  * = default export
";
    }
}

/// <summary>
/// CSS formatter
/// </summary>
public class CssFormatter : ITypeFormatter<List<DestructuredCss>>
{
    public string[] SupportedExtensions => [".css", ".scss", ".less"];

    public string Format(List<DestructuredCss> files)
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

    public string GetLegend()
    {
        return @"CSS (.css, .scss, .less):
  CSS=File V=Variables S=Selectors
  [var=value,...] = First 3 variables
";
    }
}