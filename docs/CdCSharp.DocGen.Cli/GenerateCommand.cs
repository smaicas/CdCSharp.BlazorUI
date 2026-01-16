using CdCSharp.DocGen.Cli.Logging;
using CdCSharp.DocGen.Core.Extensions;
using CdCSharp.DocGen.Core.Models.Options;
using CdCSharp.DocGen.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace CdCSharp.DocGen.Cli.Commands;

public static class GenerateCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        CommandLineOptions cliOptions = ParseArgs(args);

        if (cliOptions.ShowHelp)
        {
            PrintHelp();
            return 0;
        }

        if (!Directory.Exists(cliOptions.ProjectPath))
        {
            Console.WriteLine($"Error: Directory not found: {cliOptions.ProjectPath}");
            return 1;
        }

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole(options =>
                    {
                        options.FormatterName = "minimal";
                        options.TimestampFormat = "";
                        options.UseUtcTimestamp = false;
                        options.IncludeScopes = false;
                    })
                    .AddConsoleFormatter<MinimalConsoleFormatter, ConsoleFormatterOptions>();
                    logging.SetMinimumLevel(cliOptions.LogLevel);
                })
            .ConfigureServices((context, services) =>
            {
                services.AddDocGen(options =>
                {
                    options.ProjectPath = cliOptions.ProjectPath;
                    options.OutputPath = cliOptions.OutputPath;

                    options.Ai = new AiProviderOptions
                    {
                        Provider = cliOptions.UseLMStudio ? AiProviderType.LMStudio : AiProviderType.Groq,
                        ApiKey = cliOptions.ApiKey,
                        BaseUrl = cliOptions.LMStudioUrl,
                        Model = cliOptions.Model
                    };

                    options.Cache = new CacheOptions
                    {
                        Enabled = cliOptions.EnableCache,
                        EnableAnalysisCache = cliOptions.EnableAnalysisCache,
                        EnableQueryCache = cliOptions.EnableQueryCache
                    };
                    options.PromptTracer = new PromptTracerOptions
                    {
                        Enabled = cliOptions.Trace,
                        TraceOutputPath = Path.Combine(cliOptions.OutputPath, "trace")
                    };
                });
            })
            .Build();

        using IServiceScope scope = host.Services.CreateScope();
        DocGenRunner runner = scope.ServiceProvider.GetRequiredService<DocGenRunner>();

        return await runner.RunAsync();
    }

    private static CommandLineOptions ParseArgs(string[] args)
    {
        CommandLineOptions options = new()
        {
            ProjectPath = Directory.GetCurrentDirectory(),
            OutputPath = ".docgen",
            ApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? string.Empty,
            LMStudioUrl = Environment.GetEnvironmentVariable("LMSTUDIO_URL") ?? "http://localhost:1234/v1/",
            Model = "llama-3.3-70b-versatile"
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
                    options.LogLevel = LogLevel.Debug;
                    break;
                case "-tp" or "--trace-prompts":
                    options.Trace = true;
                    break;
                case "--no-cache":
                    options.EnableCache = false;
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
        Console.WriteLine("  -o, --output <path>        Output directory (default: .docgen)");
        Console.WriteLine("  -k, --api-key <key>        Groq API key (or set GROQ_API_KEY)");
        Console.WriteLine("  --lmstudio                 Use LM Studio instead of Groq");
        Console.WriteLine("  --lmstudio-url <url>       LM Studio URL (default: http://localhost:1234/v1/)");
        Console.WriteLine("  -m, --model <name>         Model name to use");
        Console.WriteLine("  -v, --verbose              Verbose output");
        Console.WriteLine("  --trace                    Trace mode (saves prompts to files)");
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
        Console.WriteLine("  .docgen/preanalysis/       JSON preanalysis files");
        Console.WriteLine("  .docgen/docs-human.md      Human-readable documentation");
        Console.WriteLine("  .docgen/docs-llm.txt       LLM-optimized documentation");
        Console.WriteLine("  .docgen/trace/             Prompt traces (when --trace is enabled)");
    }

    private sealed class CommandLineOptions
    {
        public string ProjectPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string LMStudioUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public LogLevel LogLevel { get; set; }
        public bool Trace { get; set; }
        public bool EnableCache { get; set; } = true;
        public bool EnableAnalysisCache { get; set; } = true;
        public bool EnableQueryCache { get; set; } = true;
        public bool ShowHelp { get; set; }
        public bool UseLMStudio { get; set; }
    }
}