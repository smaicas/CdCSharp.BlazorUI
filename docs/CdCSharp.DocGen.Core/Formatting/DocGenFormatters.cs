using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using System.Text;

namespace CdCSharp.DocGen.Core.Formatting;

public interface IDocumentationFormatter
{
    Task<string> FormatAsync(ProjectInfo project, List<ComponentInfo> components);
}

public class HumanFormatter : IDocumentationFormatter
{
    private readonly IAiClient? _ai;
    private readonly ILogger _logger;

    public HumanFormatter(IAiClient? ai = null, ILogger? logger = null)
    {
        _ai = ai;
        _logger = logger ?? new NullLogger();
    }

    public async Task<string> FormatAsync(ProjectInfo project, List<ComponentInfo> components)
    {
        _logger.Verbose("Formatting human-readable documentation...");

        StringBuilder sb = new();

        sb.AppendLine($"# {project.Name}");
        sb.AppendLine();
        sb.AppendLine($"**Type:** {project.Type} | **Files:** {project.Files.Count} | **Estimated Tokens:** {project.Files.Sum(f => f.TokenEstimate):N0}");
        sb.AppendLine();

        await AppendPublicApiAsync(sb, project);
        AppendComponents(sb, components);
        await AppendPatternsAsync(sb, project);
        AppendFileStructure(sb, project);
        await AppendCriticalTypesAsync(sb, project);

        _logger.Verbose("Human-readable documentation formatted");

        return Optimize(sb.ToString());
    }

    private async Task AppendPublicApiAsync(StringBuilder sb, ProjectInfo project)
    {
        if (project.PublicTypes.Count == 0) return;

        _logger.Verbose("  Formatting public API section...");

        sb.AppendLine("## Public API");
        sb.AppendLine();

        IOrderedEnumerable<IGrouping<string, TypeInfo>> byNamespace = project.PublicTypes
            .GroupBy(t => t.Namespace)
            .OrderBy(g => g.Key);

        foreach (IGrouping<string, TypeInfo> ns in byNamespace)
        {
            sb.AppendLine($"### {ns.Key}");
            sb.AppendLine();

            foreach (IGrouping<TypeKind, TypeInfo> kindGroup in ns.GroupBy(t => t.Kind))
            {
                sb.AppendLine($"**{kindGroup.Key.ToPlural()}:** {string.Join(", ", kindGroup.Select(t => t.Name))}");
            }
            sb.AppendLine();
        }

        await Task.CompletedTask;
    }

    private void AppendComponents(StringBuilder sb, List<ComponentInfo> components)
    {
        if (components.Count == 0) return;

        _logger.Verbose("  Formatting Blazor components section...");

        sb.AppendLine("## Blazor Components");
        sb.AppendLine();

        foreach (ComponentInfo c in components.OrderBy(c => c.Name))
        {
            sb.AppendLine($"### {c.Name}");
            sb.AppendLine();
            sb.AppendLine($"**File:** `{c.FilePath}`");

            if (c.Parameters.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("**Parameters:**");
                foreach (ParameterInfo p in c.Parameters)
                {
                    string req = p.IsRequired ? " (required)" : "";
                    sb.AppendLine($"- `{p.Name}` : {p.Type}{req}");
                }
            }
            sb.AppendLine();
        }
    }

    private async Task AppendPatternsAsync(StringBuilder sb, ProjectInfo project)
    {
        if (project.Patterns.Count == 0) return;

        _logger.Verbose("  Formatting architectural patterns section...");

        sb.AppendLine("## Architectural Patterns");
        sb.AppendLine();

        foreach (PatternInfo p in project.Patterns)
        {
            sb.AppendLine($"### {p.Name}");
            sb.AppendLine();

            if (_ai != null)
            {
                _logger.Verbose($"    Requesting AI explanation for {p.Name}...");
                string explanation = await _ai.ExplainPatternAsync(p.Name, p.AffectedFiles);
                sb.AppendLine(!string.IsNullOrWhiteSpace(explanation) ? explanation : p.Description);
            }
            else
            {
                sb.AppendLine(p.Description);
            }

            sb.AppendLine();
            sb.AppendLine($"**Affected files:** {p.AffectedFiles.Count}");
            sb.AppendLine();
        }
    }

    private void AppendFileStructure(StringBuilder sb, ProjectInfo project)
    {
        _logger.Verbose("  Formatting file structure section...");

        sb.AppendLine("## File Structure");
        sb.AppendLine();

        IOrderedEnumerable<IGrouping<FileType, Models.FileInfo>> byType = project.Files
            .Where(f => f.Importance >= ImportanceLevel.Normal)
            .GroupBy(f => f.Type)
            .OrderBy(g => g.Key);

        foreach (IGrouping<FileType, Models.FileInfo> group in byType)
        {
            List<Models.FileInfo> files = group.ToList();
            sb.AppendLine($"### {group.Key} ({files.Count} files, {files.Sum(f => f.LineCount):N0} lines)");
            sb.AppendLine();

            List<Models.FileInfo> important = files.Where(f => f.Importance >= ImportanceLevel.High).ToList();
            foreach (Models.FileInfo f in important.Take(10))
            {
                sb.Append($"- `{f.RelativePath}`");
                if (f.PublicSymbols.Count > 0)
                    sb.Append($" - {string.Join(", ", f.PublicSymbols.Take(3))}");
                sb.AppendLine();
            }

            int remaining = files.Count - important.Count;
            if (remaining > 0)
                sb.AppendLine($"- *{remaining} additional files*");

            sb.AppendLine();
        }
    }

    private async Task AppendCriticalTypesAsync(StringBuilder sb, ProjectInfo project)
    {
        List<TypeInfo> critical = project.PublicTypes.Where(t => t.Importance == ImportanceLevel.Critical).ToList();
        if (critical.Count == 0) return;

        _logger.Verbose($"  Formatting {critical.Count} critical types...");

        sb.AppendLine("## Critical Implementations");
        sb.AppendLine();

        foreach (TypeInfo t in critical)
        {
            sb.AppendLine($"### {t.Namespace}.{t.Name}");
            sb.AppendLine();
            sb.AppendLine($"**Kind:** {t.Kind} | **File:** `{t.FilePath}`");
            sb.AppendLine();

            if (_ai != null && string.IsNullOrWhiteSpace(t.AiSummary))
            {
                string fullPath = Path.Combine(project.RootPath, t.FilePath);
                if (File.Exists(fullPath))
                {
                    _logger.Verbose($"    Requesting AI summary for {t.Name}...");
                    string code = await File.ReadAllTextAsync(fullPath);
                    string summary = await _ai.SummarizeAsync(code);
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        sb.AppendLine(summary);
                        sb.AppendLine();
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(t.AiSummary))
            {
                sb.AppendLine(t.AiSummary);
                sb.AppendLine();
            }

            if (t.PublicMembers.Count > 0)
            {
                sb.AppendLine("**Key members:**");
                foreach (MemberInfo m in t.PublicMembers.Take(5))
                    sb.AppendLine($"- `{m.Signature}`");

                if (t.PublicMembers.Count > 5)
                    sb.AppendLine($"- *+{t.PublicMembers.Count - 5} more*");

                sb.AppendLine();
            }

            if (t.Attributes.Count > 0)
            {
                sb.AppendLine($"**Attributes:** {string.Join(", ", t.Attributes.Select(a => $"[{a.Name}]"))}");
                sb.AppendLine();
            }
        }
    }

    private static string Optimize(string text)
    {
        string[] lines = text.Split('\n');
        List<string> result = [];
        bool lastEmpty = false;

        foreach (string line in lines)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(line);
            if (isEmpty && lastEmpty) continue;
            result.Add(line.TrimEnd());
            lastEmpty = isEmpty;
        }

        return string.Join('\n', result).Trim();
    }
}

public class LlmFormatter : IDocumentationFormatter
{
    private readonly ILogger _logger;

    public LlmFormatter(ILogger? logger = null)
    {
        _logger = logger ?? new NullLogger();
    }

    public Task<string> FormatAsync(ProjectInfo project, List<ComponentInfo> components)
    {
        _logger.Verbose("Formatting LLM-optimized documentation...");

        StringBuilder sb = new();

        // Header ultra-compacto
        sb.AppendLine($"PROJECT {project.Name} {project.Type}");

        // API en formato denso
        _logger.Verbose("  Formatting public API...");
        IOrderedEnumerable<IGrouping<string, TypeInfo>> byNamespace = project.PublicTypes
            .GroupBy(t => t.Namespace)
            .OrderBy(g => g.Key);

        foreach (IGrouping<string, TypeInfo> ns in byNamespace)
        {
            sb.AppendLine($"NS {ns.Key}");
            foreach (TypeInfo t in ns.OrderBy(x => x.Name))
            {
                string kind = t.Kind.ToString()[0].ToString();
                string bases = t.BaseTypes.Count > 0 ? $" : {string.Join(",", t.BaseTypes)}" : "";
                sb.AppendLine($"  {kind} {t.Name}{bases}");

                foreach (MemberInfo m in t.PublicMembers.Take(20)) // Limitar a 20 miembros
                {
                    string mk = m.Kind switch
                    {
                        MemberKind.Method => "M",
                        MemberKind.Property => "P",
                        MemberKind.Field => "F",
                        MemberKind.Event => "E",
                        _ => "?"
                    };
                    sb.AppendLine($"    {mk} {m.Signature}");
                }

                if (t.PublicMembers.Count > 20)
                    sb.AppendLine($"    ... +{t.PublicMembers.Count - 20} more members");
            }
        }

        // Componentes
        if (components.Count > 0)
        {
            _logger.Verbose($"  Formatting {components.Count} components...");
            sb.AppendLine("COMPONENTS");
            foreach (ComponentInfo c in components.OrderBy(c => c.Name))
            {
                sb.Append($"  {c.Name}");
                if (c.Parameters.Count > 0)
                {
                    string pars = string.Join(",", c.Parameters.Select(p => $"{p.Name}:{p.Type}"));
                    sb.Append($" [{pars}]");
                }
                sb.AppendLine();
            }
        }

        // Files agrupados por tipo
        _logger.Verbose("  Formatting file structure...");
        sb.AppendLine("FILES");
        foreach (IGrouping<FileType, Models.FileInfo> g in project.Files.GroupBy(f => f.Type).OrderBy(g => g.Key))
        {
            sb.AppendLine($"  {g.Key}:");
            foreach (Models.FileInfo f in g.OrderBy(x => x.RelativePath))
                sb.AppendLine($"    {f.RelativePath}");
        }

        // Patterns
        if (project.Patterns.Count > 0)
        {
            _logger.Verbose($"  Formatting {project.Patterns.Count} patterns...");
            sb.AppendLine($"PATTERNS {string.Join(",", project.Patterns.Select(p => p.Name.Replace(" ", "")))}");
        }

        _logger.Verbose("LLM-optimized documentation formatted");

        return Task.FromResult(sb.ToString().TrimEnd());
    }
}