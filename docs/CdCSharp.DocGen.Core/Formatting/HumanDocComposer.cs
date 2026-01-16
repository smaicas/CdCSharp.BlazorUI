using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using System.Text;

namespace CdCSharp.DocGen.Core.Formatting;

public class HumanDocComposer
{
    private readonly ILogger _logger;

    public HumanDocComposer(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    public string Compose(GenerationContext context)
    {
        _logger.Progress("Composing human-readable documentation...");

        StringBuilder sb = new();

        sb.AppendLine($"# {context.Structure.Solution}");
        sb.AppendLine();
        sb.AppendLine($"**Type:** {context.Structure.GlobalSummary.ProjectType}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(context.CriticalContext))
        {
            sb.AppendLine(context.CriticalContext);
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();

        IOrderedEnumerable<DocumentSection> orderedSections = context.Plan.OutputSections.OrderBy(s => s.Order);

        foreach (DocumentSection section in orderedSections)
        {
            List<SpecialistResult> sectionResults = context.Results
                .Where(r => r.TargetSections.Contains(section.Id))
                .ToList();

            if (sectionResults.Count == 0)
                continue;

            sb.AppendLine($"## {section.Title}");
            sb.AppendLine();

            foreach (SpecialistResult result in sectionResults)
            {
                sb.AppendLine(CleanContent(result.Content));
                sb.AppendLine();
            }
        }

        AppendProjectStructure(sb, context.Structure);

        string doc = sb.ToString();
        _logger.Success($"Human documentation: {doc.Length} chars");

        return OptimizeMarkdown(doc);
    }

    private void AppendProjectStructure(StringBuilder sb, ProjectStructure structure)
    {
        sb.AppendLine("## Project Structure");
        sb.AppendLine();

        sb.AppendLine("### Assemblies");
        sb.AppendLine();

        foreach (AssemblyInfo assembly in structure.Assemblies.Where(a => !a.IsTestProject))
        {
            sb.AppendLine($"**{assembly.Name}** (`{assembly.Path}`)");
            sb.AppendLine();

            List<string> stats = [];

            if (assembly.Summary.Classes > 0)
                stats.Add($"{assembly.Summary.Classes} classes");
            if (assembly.Summary.Interfaces > 0)
                stats.Add($"{assembly.Summary.Interfaces} interfaces");
            if (assembly.Summary.Records > 0)
                stats.Add($"{assembly.Summary.Records} records");
            if (assembly.Summary.Components > 0)
                stats.Add($"{assembly.Summary.Components} components");

            if (stats.Count > 0)
                sb.AppendLine($"- {string.Join(", ", stats)}");

            sb.AppendLine();
        }

        if (structure.Assemblies.Any(a => a.IsTestProject))
        {
            sb.AppendLine("### Test Projects");
            sb.AppendLine();

            foreach (AssemblyInfo assembly in structure.Assemblies.Where(a => a.IsTestProject))
            {
                sb.AppendLine($"- {assembly.Name}");
            }

            sb.AppendLine();
        }
    }

    private static string CleanContent(string content)
    {
        string cleaned = content.Trim();

        if (cleaned.StartsWith("```markdown"))
            cleaned = cleaned[11..];
        else if (cleaned.StartsWith("```md"))
            cleaned = cleaned[5..];
        else if (cleaned.StartsWith("```"))
            cleaned = cleaned[3..];

        if (cleaned.EndsWith("```"))
            cleaned = cleaned[..^3];

        return cleaned.Trim();
    }

    private static string OptimizeMarkdown(string text)
    {
        string[] lines = text.Split('\n');
        List<string> result = [];
        bool lastEmpty = false;

        foreach (string line in lines)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(line);

            if (isEmpty && lastEmpty)
                continue;

            result.Add(line.TrimEnd());
            lastEmpty = isEmpty;
        }

        return string.Join('\n', result).Trim();
    }
}