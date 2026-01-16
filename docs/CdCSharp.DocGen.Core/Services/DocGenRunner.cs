using CdCSharp.DocGen.Core.Abstractions.Analysis;
using CdCSharp.DocGen.Core.Abstractions.Cache;
using CdCSharp.DocGen.Core.Abstractions.Formatting;
using CdCSharp.DocGen.Core.Abstractions.Orchestration;
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
    private readonly ISpecialistRunner _specialistRunner;
    private readonly IHumanDocComposer _humanComposer;
    private readonly ILlmDocComposer _llmComposer;
    private readonly ICacheManager _cache;
    private readonly DocGenOptions _options;
    private readonly ILogger<DocGenRunner> _logger;

    public DocGenRunner(
        IProjectAnalyzer analyzer,
        IOrchestrator orchestrator,
        ISpecialistRunner specialistRunner,
        IHumanDocComposer humanComposer,
        ILlmDocComposer llmComposer,
        ICacheManager cache,
        IOptions<DocGenOptions> options,
        ILogger<DocGenRunner> logger)
    {
        _analyzer = analyzer;
        _orchestrator = orchestrator;
        _specialistRunner = specialistRunner;
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

            if (_options.PromptTracer.Enabled)
            {
                _logger.LogInformation("TRACE MODE ENABLED - Detailed logging active");
            }

            AnalysisResult analysis = await _analyzer.AnalyzeAsync(_options.ProjectPath);

            string outputPath = GetOutputPath();
            Directory.CreateDirectory(outputPath);

            await SavePreanalysisAsync(outputPath, analysis);

            OrchestrationPlan plan = await _orchestrator.CreatePlanAsync(
                analysis.Structure,
                analysis.Destructured);

            List<SpecialistResult> results = await _specialistRunner.ExecuteAllAsync(
                plan,
                analysis.Destructured);

            GenerationContext context = new()
            {
                Structure = analysis.Structure,
                Destructured = analysis.Destructured,
                Plan = plan,
                Results = results
            };

            string humanDoc = _humanComposer.Compose(context);
            string humanPath = Path.Combine(outputPath, "docs-human.md");
            await File.WriteAllTextAsync(humanPath, humanDoc);
            _logger.LogInformation("Generated: {Path}", humanPath);

            string llmDoc = await _llmComposer.ComposeAsync(context);
            string llmPath = Path.Combine(outputPath, "docs-llm.txt");
            await File.WriteAllTextAsync(llmPath, llmDoc);
            _logger.LogInformation("Generated: {Path}", llmPath);

            _cache.PrintStatistics();

            _logger.LogInformation("");
            _logger.LogInformation("Documentation generated successfully!");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Documentation generation failed");
            return 1;
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