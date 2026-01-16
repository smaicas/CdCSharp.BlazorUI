using CdCSharp.DocGen.Core.Analysis;
using CdCSharp.DocGen.Core.Cache;
using CdCSharp.DocGen.Core.Formatting;
using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;
using CdCSharp.DocGen.Core.Orchestration;
using System.Text.Json;

namespace CdCSharp.DocGen.Cli;

public class GenerateCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        GenerateOptions options = ParseArgs(args);

        if (options.ShowHelp)
        {
            PrintHelp();
            return 0;
        }

        if (!Directory.Exists(options.ProjectPath))
        {
            Console.WriteLine($"Error: Directory not found: {options.ProjectPath}");
            return 1;
        }

        return await ExecuteAsync(options);
    }

    private static async Task<int> ExecuteAsync(GenerateOptions options)
    {
        ILogger logger = new ConsoleLogger(options.Verbose, options.Trace);

        logger.Info("CdCSharp Documentation Generator");
        logger.Info(new string('=', 40));
        logger.Info("");

        if (options.Trace)
        {
            logger.Info("🔍 TRACE MODE ENABLED - Detailed logging active");
            logger.Info("");
        }

        CacheOptions cacheOptions = new()
        {
            EnableAnalysisCache = options.EnableAnalysisCache,
            EnableQueryCache = options.EnableQueryCache
        };

        using CacheManager? cache = options.EnableAnalysisCache || options.EnableQueryCache
            ? new CacheManager(options.ProjectPath, cacheOptions, logger)
            : null;

        // CAMBIO PRINCIPAL: Usar LMStudioClient en lugar de GroqClient
        using IAiClient ai = options.UseLMStudio
            ? new LMStudioClient(
                baseUrl: options.LMStudioUrl,
                logger: logger,
                model: options.Model,
                trace: options.Trace)
            : new GroqClient(
                apiKey: options.ApiKey,
                logger: logger,
                model: options.Model,
                trace: options.Trace);

        try
        {
            ProjectAnalyzer analyzer = new(cache, logger);
            (ProjectStructure structure, Dictionary<string, DestructuredAssembly> destructured) =
                await analyzer.AnalyzeAsync(options.ProjectPath);

            Directory.CreateDirectory(options.OutputPath);
            await SavePreanalysisAsync(options.OutputPath, structure, destructured, logger);

            SpecialistRegistry registry = new(options.ProjectPath, logger);
            Orchestrator orchestrator = new(ai, registry, logger);
            OrchestrationPlan plan = await orchestrator.CreatePlanAsync(structure, destructured);

            SpecialistRunner runner = new(ai, options.ProjectPath, cache, logger);
            List<SpecialistResult> results = await runner.ExecuteAllAsync(plan, destructured);

            GenerationContext context = new()
            {
                Structure = structure,
                Destructured = destructured,
                Plan = plan,
                Results = results,
                CriticalContext = plan.CriticalContext
            };

            HumanDocComposer humanComposer = new(logger);
            string humanDoc = humanComposer.Compose(context);
            string humanPath = Path.Combine(options.OutputPath, "docs-human.md");
            await File.WriteAllTextAsync(humanPath, humanDoc);
            logger.Success($"Generated: {humanPath}");

            LlmDocComposer llmComposer = new(options.ProjectPath, logger);
            string llmDoc = await llmComposer.ComposeAsync(context);
            string llmPath = Path.Combine(options.OutputPath, "docs-llm.txt");
            await File.WriteAllTextAsync(llmPath, llmDoc);
            logger.Success($"Generated: {llmPath}");

            cache?.PrintStatistics();

            logger.Info("");
            logger.Success("Documentation generated successfully!");

            return 0;
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
            if (options.Verbose || options.Trace)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return 1;
        }
    }

    private static async Task SavePreanalysisAsync(
        string outputPath,
        ProjectStructure structure,
        Dictionary<string, DestructuredAssembly> destructured,
        ILogger logger)
    {
        string preanalysisPath = Path.Combine(outputPath, "preanalysis");
        Directory.CreateDirectory(preanalysisPath);

        JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        string structurePath = Path.Combine(preanalysisPath, "structure.json");
        await File.WriteAllTextAsync(structurePath, JsonSerializer.Serialize(structure, jsonOptions));
        logger.Verbose($"Saved: {structurePath}");

        foreach ((string name, DestructuredAssembly assembly) in destructured)
        {
            string assemblyPath = Path.Combine(preanalysisPath, $"{name}.json");
            await File.WriteAllTextAsync(assemblyPath, JsonSerializer.Serialize(assembly, jsonOptions));
            logger.Verbose($"Saved: {assemblyPath}");
        }

        logger.Success($"Preanalysis saved to: {preanalysisPath}");
    }

    private static GenerateOptions ParseArgs(string[] args)
    {
        GenerateOptions options = new()
        {
            ProjectPath = Directory.GetCurrentDirectory(),
            OutputPath = "docs",
            ApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty,
            LMStudioUrl = Environment.GetEnvironmentVariable("LMSTUDIO_URL") ?? "http://localhost:1234/v1/",
            Model = "local-model",
            Verbose = false,
            Trace = false,
            EnableAnalysisCache = true,
            EnableQueryCache = true,
            UseLMStudio = false
        };

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help":
                    options.ShowHelp = true;
                    break;
                case "-p" or "--project":
                    options.ProjectPath = GetNextArg(args, ref i);
                    break;
                case "-o" or "--output":
                    options.OutputPath = GetNextArg(args, ref i);
                    break;
                case "-k" or "--api-key":
                    options.ApiKey = GetNextArg(args, ref i);
                    break;
                case "--lmstudio":
                    options.UseLMStudio = true;
                    break;
                case "--lmstudio-url":
                    options.LMStudioUrl = GetNextArg(args, ref i);
                    options.UseLMStudio = true;
                    break;
                case "-m" or "--model":
                    options.Model = GetNextArg(args, ref i);
                    break;
                case "-v" or "--verbose":
                    options.Verbose = true;
                    break;
                case "--trace":
                    options.Trace = true;
                    options.Verbose = true;
                    break;
                case "--no-cache":
                    options.EnableAnalysisCache = false;
                    options.EnableQueryCache = false;
                    break;
                case "--no-analysis-cache":
                    options.EnableAnalysisCache = false;
                    break;
                case "--no-query-cache":
                    options.EnableQueryCache = false;
                    break;
            }
        }

        return options;
    }

    private static string GetNextArg(string[] args, ref int i)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Option {args[i]} requires a value");
        return args[++i];
    }

    private static void PrintHelp()
    {
        Console.WriteLine("CdCSharp Documentation Generator");
        Console.WriteLine();
        Console.WriteLine("Usage: docgen generate [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -p, --project <path>       Project directory (default: current)");
        Console.WriteLine("  -o, --output <path>        Output directory (default: docs)");
        Console.WriteLine("  -k, --api-key <key>        Groq API key (or set GROQ_API_KEY)");
        Console.WriteLine("  --lmstudio                 Use LM Studio instead of Groq");
        Console.WriteLine("  --lmstudio-url <url>       LM Studio URL (default: http://localhost:1234/v1/)");
        Console.WriteLine("  -m, --model <name>         Model name to use");
        Console.WriteLine("  -v, --verbose              Verbose output");
        Console.WriteLine("  --trace                    Trace mode (very detailed logging including prompts)");
        Console.WriteLine("  --no-cache                 Disable all caching");
        Console.WriteLine("  --no-analysis-cache        Disable analysis cache only");
        Console.WriteLine("  --no-query-cache           Disable query cache only");
        Console.WriteLine("  -h, --help                 Show this help");
        Console.WriteLine();
        Console.WriteLine("LM Studio Usage:");
        Console.WriteLine("  1. Start LM Studio and load a model");
        Console.WriteLine("  2. Go to Developer tab and start server");
        Console.WriteLine("  3. Run: docgen generate --lmstudio");
        Console.WriteLine();
        Console.WriteLine("Output:");
        Console.WriteLine("  docs/preanalysis/          JSON preanalysis files");
        Console.WriteLine("  docs/docs-human.md         Human-readable documentation");
        Console.WriteLine("  docs/docs-llm.txt          LLM-optimized documentation");
    }
}

internal class GenerateOptions
{
    public string ProjectPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string LMStudioUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool Verbose { get; set; }
    public bool Trace { get; set; }
    public bool EnableAnalysisCache { get; set; }
    public bool EnableQueryCache { get; set; }
    public bool ShowHelp { get; set; }
    public bool UseLMStudio { get; set; }
}