using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Generation;
using CdCSharp.DocGen.Core.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace CdCSharp.DocGen.Core.Formatting;

public class LlmDocComposer : ILlmDocComposer
{
    private readonly IPlainTextFormatter _formatter;
    private readonly ILogger<LlmDocComposer> _logger;
    private readonly string _projectRoot;

    public LlmDocComposer(
        IPlainTextFormatter formatter,
        IOptions<DocGenOptions> options,
        ILogger<LlmDocComposer> logger)
    {
        _formatter = formatter;
        _projectRoot = options.Value.ProjectPath;
        _logger = logger;
    }

    public async Task<string> ComposeAsync(GenerationContext context)
    {
        _logger.LogInformation("Composing LLM-optimized documentation...");

        StringBuilder sb = new();

        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine($"PROJECT: {context.Structure.Solution}");
        sb.AppendLine($"TYPE: {context.Structure.Summary.ProjectType}");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();

        sb.AppendLine(_formatter.GetLegend());
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(context.Plan.CriticalContext))
        {
            sb.AppendLine("CRITICAL CONTEXT:");
            sb.AppendLine(context.Plan.CriticalContext);
            sb.AppendLine();
        }

        sb.AppendLine(_formatter.FormatStructure(context.Structure));
        sb.AppendLine();

        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine("DETAILED STRUCTURE");
        sb.AppendLine("-".PadRight(80, '-'));
        sb.AppendLine();

        foreach ((string name, DestructuredAssembly assembly) in context.Destructured)
        {
            if (context.Structure.Assemblies.FirstOrDefault(a => a.Name == name)?.IsTestProject == true)
                continue;

            sb.AppendLine(_formatter.FormatDestructured(assembly));
        }

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
        _logger.LogInformation("LLM documentation: {Length} chars (~{Tokens} tokens)", doc.Length, doc.Length / 4);

        return doc;
    }

    private async Task AppendFileContentAsync(StringBuilder sb, string relativePath)
    {
        string fullPath = Path.Combine(_projectRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Key file not found: {Path}", relativePath);
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
            _logger.LogWarning(ex, "Failed to read {Path}", relativePath);
        }
    }
}