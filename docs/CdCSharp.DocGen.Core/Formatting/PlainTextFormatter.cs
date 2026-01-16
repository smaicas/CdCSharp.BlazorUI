using CdCSharp.DocGen.Core.Models;
using System.Text;

namespace CdCSharp.DocGen.Core.Formatting;

public class PlainTextFormatter : IProjectFormatter
{
    private const int DefaultMemberLimit = 5;

    private readonly FormatterOptions _options;

    public PlainTextFormatter(FormatterOptions? options = null)
    {
        _options = options ?? new FormatterOptions();
    }

    public string FormatStructure(ProjectStructure structure)
    {
        StringBuilder sb = new();

        AppendLegend(sb);
        sb.AppendLine($"PRJ:{structure.Solution}");
        sb.AppendLine($"TYP:{structure.GlobalSummary.ProjectType}");
        sb.AppendLine();

        sb.AppendLine("ASM:");
        foreach (AssemblyInfo asm in structure.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.Append($"  {asm.Name}");

            sb.AppendLine();
            sb.AppendLine($"    Path:{asm.Path}");

            sb.Append($" C:{asm.Summary.Classes} I:{asm.Summary.Interfaces} R:{asm.Summary.Records}");

            if (asm.Summary.Components > 0)
                sb.Append($" BC:{asm.Summary.Components}");

            if (asm.Summary.Generators > 0)
                sb.Append($" SG:{asm.Summary.Generators}");

            if (asm.Summary.TsModules > 0)
                sb.Append($" TS:{asm.Summary.TsModules}");

            if (asm.Summary.CssFiles > 0)
                sb.Append($" CSS:{asm.Summary.CssFiles}({asm.Summary.CssVariables}v)");

            sb.AppendLine();

            List<string> keyRefs = asm.References
                    .Where(r => !r.StartsWith("System") && !r.StartsWith("Microsoft.Extensions"))
                    .Take(10)
                    .ToList();

            if (keyRefs.Count > 0)
                sb.AppendLine($"    REF:{string.Join(",", keyRefs)}");
        }

        if (structure.GlobalSummary.DetectedPatterns.Count > 0)
        {
            sb.AppendLine($"PTN:{string.Join(",", structure.GlobalSummary.DetectedPatterns)}");
        }

        sb.AppendLine();
        sb.Append($"TOT: ASM:{structure.GlobalSummary.TotalAssemblies}({structure.GlobalSummary.TotalTestProjects}t)");
        sb.Append($" C:{structure.GlobalSummary.TotalClasses} I:{structure.GlobalSummary.TotalInterfaces} R:{structure.GlobalSummary.TotalRecords}");

        if (structure.GlobalSummary.TotalComponents > 0)
            sb.Append($" BC:{structure.GlobalSummary.TotalComponents}");

        if (structure.GlobalSummary.TotalGenerators > 0)
            sb.Append($" SG:{structure.GlobalSummary.TotalGenerators}");

        sb.AppendLine();
        return sb.ToString();
    }

    public string FormatDestructured(DestructuredAssembly assembly)
    {
        StringBuilder sb = new();

        sb.AppendLine($"ASM:{assembly.Assembly}");
        sb.AppendLine();

        foreach (DestructuredNamespace ns in assembly.Namespaces)
        {
            sb.AppendLine($"NS {ns.Name}");

            foreach (DestructuredType type in ns.Types)
            {
                FormatType(sb, type, 1);
            }
        }

        if (assembly.Components.Count > 0)
        {
            sb.AppendLine("BC:");
            foreach (DestructuredComponent comp in assembly.Components)
            {
                FormatComponent(sb, comp);
            }
        }

        if (assembly.TypeScript.Count > 0)
        {
            sb.AppendLine("TS:");
            foreach (DestructuredTypeScript ts in assembly.TypeScript)
            {
                FormatTypeScript(sb, ts);
            }
        }

        if (assembly.Css.Count > 0)
        {
            sb.AppendLine("CSS:");
            foreach (DestructuredCss css in assembly.Css)
            {
                FormatCss(sb, css);
            }
        }

        return sb.ToString();
    }

    private void AppendLegend(StringBuilder sb)
    {
        sb.AppendLine("=== FORMAT LEGEND ===");
        sb.AppendLine("PRJ=Project TYP=Type ASM=Assembly C=Classes I=Interfaces R=Records");
        sb.AppendLine("BC=BlazorComponents SG=SourceGenerators TS=TypeScript CSS=CSS PTN=Patterns TOT=Totals");
        sb.AppendLine("NS=Namespace C=Class I=Interface R=Record S=Struct E=Enum");
        sb.AppendLine("M=Method P=Property F=Field E=Event ctor=Constructor");
        sb.AppendLine("Component params: [ParamName:Type,Param2:Type2]");
        sb.AppendLine("Modifiers: [mod1 mod2] Attributes: @Attr1,Attr2");
        sb.AppendLine("Member limits applied when count exceeds threshold");
        sb.AppendLine("=====================");
        sb.AppendLine();
    }

    private void FormatType(StringBuilder sb, DestructuredType type, int indent)
    {
        string prefix = new(' ', indent * 2);
        string kind = type.Kind switch
        {
            TypeKind.Class => "C",
            TypeKind.Interface => "I",
            TypeKind.Record => "R",
            TypeKind.Struct => "S",
            TypeKind.Enum => "E",
            _ => "?"
        };

        string mods = type.Modifiers.Count > 0 ? $"[{string.Join(" ", type.Modifiers)}]" : "";
        string bases = type.Base.Count > 0 ? $":{string.Join(",", type.Base)}" : "";
        string attrs = type.Attributes.Count > 0 ? $"@{string.Join(",", type.Attributes)}" : "";

        sb.Append($"{prefix}{kind} {mods}{type.Name}{bases}{attrs}");

        List<IGrouping<MemberKind, DestructuredMember>> membersByKind = type.Members.GroupBy(m => m.Kind).ToList();
        int totalMembers = type.Members.Count;
        int shownMembers = 0;

        if (totalMembers > 0)
        {
            sb.Append(" {");

            foreach (IGrouping<MemberKind, DestructuredMember> group in membersByKind)
            {
                int count = group.Count();
                int limit = _options.GetMemberLimit(group.Key);
                int toShow = Math.Min(count, limit);
                int hidden = count - toShow;

                foreach (DestructuredMember? member in group.Take(toShow))
                {
                    if (shownMembers > 0) sb.Append(";");
                    FormatMemberInline(sb, member);
                    shownMembers++;
                }

                if (hidden > 0)
                {
                    if (shownMembers > 0) sb.Append(";");
                    sb.Append($"[+{hidden}{GetMemberKindShort(group.Key)}]");
                }
            }

            sb.Append("}");
        }

        sb.AppendLine();

        foreach (DestructuredType nested in type.NestedTypes)
        {
            FormatType(sb, nested, indent + 1);
        }
    }

    private void FormatMemberInline(StringBuilder sb, DestructuredMember member)
    {
        string kind = GetMemberKindShort(member.Kind);
        string attrs = member.Attributes.Count > 0 ? $"@{string.Join(",", member.Attributes)}" : "";
        sb.Append($"{kind}:{member.Signature}{attrs}");
    }

    private string GetMemberKindShort(MemberKind kind) => kind switch
    {
        MemberKind.Constructor => "ctor",
        MemberKind.Method => "M",
        MemberKind.Property => "P",
        MemberKind.Field => "F",
        MemberKind.Event => "E",
        MemberKind.Indexer => "Idx",
        _ => "?"
    };

    private void FormatComponent(StringBuilder sb, DestructuredComponent comp)
    {
        sb.Append($"  {comp.Name}");

        List<string> allParams = [];

        foreach (ComponentParameter param in comp.Parameters)
        {
            string req = param.Required ? "!" : "";
            string def = param.DefaultValue != null ? $"={param.DefaultValue}" : "";
            allParams.Add($"{param.Name}:{param.Type}{req}{def}");
        }

        foreach (ComponentParameter param in comp.CascadingParameters)
        {
            allParams.Add($"^{param.Name}:{param.Type}");
        }

        foreach (InjectableService inject in comp.Injectables)
        {
            allParams.Add($"@{inject.Name}:{inject.Type}");
        }

        foreach (string evt in comp.EventCallbacks)
        {
            allParams.Add($"E:{evt}");
        }

        foreach (string frag in comp.RenderFragments)
        {
            allParams.Add($"RF:{frag}");
        }

        if (allParams.Count > 0)
        {
            sb.Append($"[{string.Join(",", allParams)}]");
        }

        sb.AppendLine();
    }

    private void FormatTypeScript(StringBuilder sb, DestructuredTypeScript ts)
    {
        sb.Append($"  {ts.File}");

        if (ts.Exports.Count > 0)
        {
            sb.Append(" [");
            for (int i = 0; i < ts.Exports.Count; i++)
            {
                TsExport exp = ts.Exports[i];
                if (i > 0) sb.Append(",");

                string kind = GetTsExportKindShort(exp.Kind);
                string def = exp.IsDefault ? "*" : "";
                string sig = exp.Signature != null ? $":{exp.Signature}" : "";
                sb.Append($"{kind}{def}{exp.Name}{sig}");
            }
            sb.Append("]");
        }

        sb.AppendLine();
    }

    private string GetTsExportKindShort(TsExportKind kind) => kind switch
    {
        TsExportKind.Function => "f",
        TsExportKind.Class => "c",
        TsExportKind.Interface => "i",
        TsExportKind.Type => "t",
        TsExportKind.Const => "k",
        TsExportKind.Enum => "e",
        _ => "?"
    };

    private void FormatCss(StringBuilder sb, DestructuredCss css)
    {
        sb.Append($"  {css.File}[{css.Type}]");

        if (css.Variables.Count > 0)
        {
            sb.Append($" V:{css.Variables.Count}");

            if (_options.ShowCssVariableSamples)
            {
                int limit = Math.Min(css.Variables.Count, _options.CssVariableSampleLimit);
                sb.Append("(");
                for (int i = 0; i < limit; i++)
                {
                    if (i > 0) sb.Append(",");
                    CssVariable v = css.Variables[i];
                    sb.Append($"{v.Name}:{v.Value}");
                }

                if (css.Variables.Count > limit)
                    sb.Append($",+{css.Variables.Count - limit}");

                sb.Append(")");
            }
        }

        if (css.Selectors.Count > 0)
        {
            sb.Append($" S:{css.Selectors.Count}");
        }

        sb.AppendLine();
    }
}

public class FormatterOptions
{
    public int MethodLimit { get; set; } = 5;
    public int PropertyLimit { get; set; } = 8;
    public int FieldLimit { get; set; } = 5;
    public int EventLimit { get; set; } = 3;
    public int ConstructorLimit { get; set; } = 3;
    public int IndexerLimit { get; set; } = 2;

    public bool ShowCssVariableSamples { get; set; } = true;
    public int CssVariableSampleLimit { get; set; } = 3;

    public int GetMemberLimit(MemberKind kind) => kind switch
    {
        MemberKind.Method => MethodLimit,
        MemberKind.Property => PropertyLimit,
        MemberKind.Field => FieldLimit,
        MemberKind.Event => EventLimit,
        MemberKind.Constructor => ConstructorLimit,
        MemberKind.Indexer => IndexerLimit,
        _ => 5
    };
}