using CdCSharp.DocGen.Core.Abstractions.Agents;
using CdCSharp.DocGen.Core.Abstractions.Analysis;
using CdCSharp.DocGen.Core.Abstractions.Cache;
using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models.Agents;
using CdCSharp.DocGen.Core.Models.Analysis;
using CdCSharp.DocGen.Core.Models.Generation;
using CdCSharp.DocGen.Core.Models.Options;
using CdCSharp.DocGen.Core.Models.Orchestration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CdCSharp.DocGen.Core.Services;

public class DocGenRunner
{
    private readonly IProjectAnalyzer _analyzer;
    private readonly IOrchestrator _orchestrator;
    private readonly IHumanDocComposer _humanComposer;
    private readonly ILlmDocComposer _llmComposer;
    private readonly ICacheManager _cache;
    private readonly DocGenOptions _options;
    private readonly ILogger<DocGenRunner> _logger;

    public DocGenRunner(
        IProjectAnalyzer analyzer,
        IOrchestrator orchestrator,
        IHumanDocComposer humanComposer,
        ILlmDocComposer llmComposer,
        ICacheManager cache,
        IOptions<DocGenOptions> options,
        ILogger<DocGenRunner> logger)
    {
        _analyzer = analyzer;
        _orchestrator = orchestrator;
        _humanComposer = humanComposer;
        _llmComposer = llmComposer;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> RunAsync()
    {
        try
        {
            _logger.LogInformation("CdCSharp Documentation Generator");
            _logger.LogInformation(new string('=', 40));

            ValidateOptions();

            if (_options.PromptTracer.Enabled)
            {
                _logger.LogInformation("TRACE MODE ENABLED - Prompts will be saved to {Path}",
    Path.Combine(_options.OutputPath, "trace"));
            }
            _logger.LogInformation("Analyzing project: {Path}", _options.ProjectPath);
            AnalysisResult analysis = await _analyzer.AnalyzeAsync(_options.ProjectPath);

            if (analysis.Structure.Assemblies.Count == 0)
            {
                _logger.LogError("No .NET projects found in {Path}", _options.ProjectPath);
                return 1;
            }

            _logger.LogInformation("Found {Count} assemblies ({NonTest} non-test)",
                analysis.Structure.Assemblies.Count,
                analysis.Structure.Assemblies.Count(a => !a.IsTestProject));

            string outputPath = GetOutputPath();
            Directory.CreateDirectory(outputPath);

            await SavePreanalysisAsync(outputPath, analysis);

            _logger.LogInformation("Creating documentation plan...");
            OrchestrationPlan plan = await _orchestrator.CreatePlanAsync(
                analysis.Structure,
                analysis.Destructured);

            if (plan.Tasks.Count == 0)
            {
                _logger.LogWarning("No documentation tasks generated. Check orchestrator output.");
                return 1;
            }

            _logger.LogInformation("Executing {Count} documentation tasks...", plan.Tasks.Count);
            List<AgentResult> results = await _orchestrator.ExecutePlanAsync(
                plan,
                analysis.Destructured);

            if (results.Count == 0)
            {
                _logger.LogWarning("No documentation content generated. Check agent outputs.");
            }

            GenerationContext context = new()
            {
                Structure = analysis.Structure,
                Destructured = analysis.Destructured,
                Plan = plan,
                Results = results
            };

            _logger.LogInformation("Composing final documentation...");

            string humanDoc = _humanComposer.Compose(context);
            string humanPath = Path.Combine(outputPath, "docs-human.md");
            await File.WriteAllTextAsync(humanPath, humanDoc);
            _logger.LogInformation("Generated: {Path} ({Size} chars)", humanPath, humanDoc.Length);

            string llmDoc = await _llmComposer.ComposeAsync(context);
            string llmPath = Path.Combine(outputPath, "docs-llm.txt");
            await File.WriteAllTextAsync(llmPath, llmDoc);
            _logger.LogInformation("Generated: {Path} ({Size} chars, ~{Tokens} tokens)",
                llmPath, llmDoc.Length, llmDoc.Length / 4);

            _cache.PrintStatistics();

            _logger.LogInformation("");
            _logger.LogInformation("Documentation generated successfully!");
            _logger.LogInformation("  Human-readable: {Path}", humanPath);
            _logger.LogInformation("  LLM-optimized:  {Path}", llmPath);

            return 0;
        }
        catch (OptionsValidationException ex)
        {
            _logger.LogError("Configuration error: {Message}", ex.Message);
            return 2;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Documentation generation failed");
            return 1;
        }
    }

    private void ValidateOptions()
    {
        DocGenOptionsValidator validator = new();
        ValidateOptionsResult result = validator.Validate(null, _options);
        if (result.Failed)
        {
            throw new OptionsValidationException(
                nameof(DocGenOptions),
                typeof(DocGenOptions),
                result.Failures);
        }
    }

    private string GetOutputPath()
    {
        if (Path.IsPathRooted(_options.OutputPath))
            return _options.OutputPath;

        return Path.Combine(_options.ProjectPath, _options.OutputPath);
    }

    private async Task SavePreanalysisAsync(string outputPath, AnalysisResult analysis)
    {
        string preanalysisPath = Path.Combine(outputPath, "preanalysis");
        Directory.CreateDirectory(preanalysisPath);

        JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        string structurePath = Path.Combine(preanalysisPath, "structure.json");
        await File.WriteAllTextAsync(structurePath, JsonSerializer.Serialize(analysis.Structure, jsonOptions));
        _logger.LogDebug("Saved: {Path}", structurePath);

        foreach ((string name, DestructuredAssembly assembly) in analysis.Destructured)
        {
            string assemblyPath = Path.Combine(preanalysisPath, $"{name}.json");
            await File.WriteAllTextAsync(assemblyPath, JsonSerializer.Serialize(assembly, jsonOptions));
            _logger.LogDebug("Saved: {Path}", assemblyPath);
        }

        _logger.LogInformation("Preanalysis saved to: {Path}", preanalysisPath);
    }
}