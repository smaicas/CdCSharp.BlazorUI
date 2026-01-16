using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Generation;
using CdCSharp.DocGen.Core.Models.Orchestration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CdCSharp.DocGen.Core.Formatting;

public class HumanDocComposer : IHumanDocComposer
{
    private readonly ILogger<HumanDocComposer> _logger;

    public HumanDocComposer(ILogger<HumanDocComposer> logger)
    {
        _logger = logger;
    }

    public string Compose(GenerationContext context)
    {
        _logger.LogInformation("Composing human-readable documentation...");

        StringBuilder sb = new();

        sb.AppendLine($"# {context.Structure.Solution}");
        sb.AppendLine();
        sb.AppendLine($"**Type:** {context.Structure.Summary.ProjectType}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(context.Plan.CriticalContext))
        {
            sb.AppendLine(context.Plan.CriticalContext);
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
        _logger.LogInformation("Human documentation: {Length} chars", doc.Length);

        return OptimizeMarkdown(doc);
    }

    private static void AppendProjectStructure(StringBuilder sb, ProjectStructure structure)
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

            if (assembly.Metrics.Classes > 0)
                stats.Add($"{assembly.Metrics.Classes} classes");
            if (assembly.Metrics.Interfaces > 0)
                stats.Add($"{assembly.Metrics.Interfaces} interfaces");
            if (assembly.Metrics.Records > 0)
                stats.Add($"{assembly.Metrics.Records} records");
            if (assembly.Metrics.Components > 0)
                stats.Add($"{assembly.Metrics.Components} components");

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