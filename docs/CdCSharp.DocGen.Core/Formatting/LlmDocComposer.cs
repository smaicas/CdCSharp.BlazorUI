using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using System.Text;

namespace CdCSharp.DocGen.Core.Formatting;

public class LlmDocComposer
{
    private readonly PlainTextFormatter _formatter;
    private readonly ILogger _logger;
    private readonly string _projectRoot;

    public LlmDocComposer(string projectRoot, ILogger? logger = null)
    {
        _projectRoot = projectRoot;
        _formatter = new PlainTextFormatter();
        _logger = logger ?? NullLogger.Instance;
    }

    public async Task<string> ComposeAsync(GenerationContext context)
    {
        _logger.Progress("Composing LLM-optimized documentation...");

        StringBuilder sb = new();

        // Header
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine($"PROJECT: {context.Structure.Solution}");
        sb.AppendLine($"TYPE: {context.Structure.GlobalSummary.ProjectType}");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();

        // Legend
        sb.AppendLine(PlainTextFormatter.GetLegend());
        sb.AppendLine();

        // Critical context
        if (!string.IsNullOrWhiteSpace(context.CriticalContext))
        {
            sb.AppendLine("CRITICAL CONTEXT:");
            sb.AppendLine(context.CriticalContext);
            sb.AppendLine();
        }

        // Project structure
        sb.AppendLine(_formatter.FormatStructure(context.Structure));
        sb.AppendLine();

        // Detailed structure by assembly
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine("DETAILED STRUCTURE");
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine();

        foreach ((string name, DestructuredAssembly assembly) in context.Destructured)
        {
            // Skip test projects
            if (context.Structure.Assemblies.FirstOrDefault(a => a.Name == name)?.IsTestProject == true)
                continue;

            sb.AppendLine(_formatter.FormatDestructured(assembly));
        }

        // Key files (full content)
        if (context.Plan.KeyFiles.Count > 0)
        {
            sb.AppendLine("-".PadRight(80, '-'));
            sb.AppendLine("KEY FILES (FULL CONTENT)");
            sb.AppendLine("-".PadRight(80, '-'));
            sb.AppendLine();

            foreach (string filePath in context.Plan.KeyFiles)
            {
                await AppendFileContentAsync(sb, filePath);
            }
        }

        string doc = sb.ToString();
        _logger.Success($"LLM documentation: {doc.Length} chars (~{doc.Length / 4} tokens)");

        return doc;
    }

    private async Task AppendFileContentAsync(StringBuilder sb, string relativePath)
    {
        string fullPath = Path.Combine(_projectRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            _logger.Warning($"Key file not found: {relativePath}");
            return;
        }

        try
        {
            string content = await File.ReadAllTextAsync(fullPath);

            sb.AppendLine($"=== {relativePath} ===");
            sb.AppendLine(content);
            sb.AppendLine();
        }
        catch (Exception ex)
        {
            _logger.Warning($"Failed to read {relativePath}: {ex.Message}");
        }
    }
}