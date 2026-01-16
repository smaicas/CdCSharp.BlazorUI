using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.RegularExpressions;
namespace CdCSharp.DocGen.Core.Agents;

public class ExpertiseContextBuilder : IExpertiseContextBuilder
{
    private readonly IPlainTextFormatter _formatter;
    private readonly ILogger<ExpertiseContextBuilder> _logger;
    private readonly string _projectRoot;
    public ExpertiseContextBuilder(
    IPlainTextFormatter formatter,
    IOptions<DocGenOptions> options,
    ILogger<ExpertiseContextBuilder> logger)
    {
        _formatter = formatter;
        _projectRoot = options.Value.ProjectPath;
        _logger = logger;
    }

    public async Task<string> BuildContextAsync(
        AgentExpertise expertise,
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured)
    {
        StringBuilder sb = new();

        foreach (string assemblyName in expertise.Assemblies)
        {
            if (destructured.TryGetValue(assemblyName, out DestructuredAssembly? assembly))
            {
                sb.AppendLine(_formatter.FormatDestructured(assembly));
                sb.AppendLine();
            }
        }

        if (expertise.Namespaces.Count > 0)
        {
            foreach ((string name, DestructuredAssembly assembly) in destructured)
            {
                List<DestructuredNamespace> matchingNamespaces = assembly.Namespaces
                    .Where(ns => expertise.Namespaces.Any(pattern =>
                        ns.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (matchingNamespaces.Count > 0)
                {
                    sb.AppendLine($"### {name} (filtered namespaces)");
                    foreach (DestructuredNamespace ns in matchingNamespaces)
                    {
                        sb.AppendLine($"NS: {ns.Name} ({ns.Types.Count} types)");
                    }
                    sb.AppendLine();
                }
            }
        }

        if (expertise.FilePatterns.Count > 0)
        {
            List<string> matchingFiles = FindMatchingFiles(structure, expertise.FilePatterns);
            foreach (string file in matchingFiles.Take(10))
            {
                await AppendFileContentAsync(sb, file);
            }
        }

        foreach (string file in expertise.Files)
        {
            await AppendFileContentAsync(sb, file);
        }

        string context = sb.ToString();
        _logger.LogDebug("Built expertise context: {Length} chars", context.Length);

        return context;
    }

    private List<string> FindMatchingFiles(ProjectStructure structure, List<string> patterns)
    {
        List<string> allFiles = structure.Assemblies
            .Where(a => !a.IsTestProject)
            .SelectMany(a => a.Files.CSharp
                .Concat(a.Files.Razor)
                .Concat(a.Files.TypeScript)
                .Concat(a.Files.Css))
            .ToList();

        List<string> matching = [];

        foreach (string pattern in patterns)
        {
            string regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            Regex regex = new(regexPattern, RegexOptions.IgnoreCase);

            matching.AddRange(allFiles.Where(f =>
                regex.IsMatch(Path.GetFileName(f))));
        }

        return matching.Distinct().ToList();
    }

    private async Task AppendFileContentAsync(StringBuilder sb, string relativePath)
    {
        string fullPath = Path.Combine(_projectRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogDebug("File not found: {Path}", relativePath);
            return;
        }

        try
        {
            string content = await File.ReadAllTextAsync(fullPath);
            string truncated = content.Length > 5000 ? content[..5000] + "\n// ... (truncated)" : content;

            sb.AppendLine($"=== FILE: {relativePath} ===");
            sb.AppendLine(truncated);
            sb.AppendLine();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read {Path}", relativePath);
        }
    }
}