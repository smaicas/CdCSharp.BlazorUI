using CdCSharp.DocGen.Core;
using CdCSharp.DocGen.Core.Formatting;
using CdCSharp.DocGen.Core.Infrastructure;
using CdCSharp.DocGen.Core.Models;

namespace CdCSharp.DocGen.Cli;

public enum OutputMode { Both, Human, Llm }

public static class CommandRunner
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            PrintHelp();
            return 0;
        }

        string command = args[0].ToLowerInvariant();
        string[] commandArgs = args.Skip(1).ToArray();

        return command switch
        {
            "generate" => await GenerateCommand.RunAsync(commandArgs),
            "analyze" => await AnalyzeCommand.RunAsync(commandArgs),
            "test-ignore" => await TestIgnoreCommand.RunAsync(commandArgs),
            _ => PrintUnknownCommand(command)
        };
    }

    private static void PrintHelp()
    {
        Console.WriteLine("CdCSharp Documentation Generator");
        Console.WriteLine();
        Console.WriteLine("Usage: docgen <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  generate      Generate documentation (human-readable and/or LLM-optimized)");
        Console.WriteLine("  analyze       Analyze project structure without generating files");
        Console.WriteLine("  test-ignore   Test ignore patterns");
        Console.WriteLine();
        Console.WriteLine("Run 'docgen <command> --help' for more information on a command.");
    }

    private static int PrintUnknownCommand(string command)
    {
        Console.WriteLine($"❌ Unknown command '{command}'");
        Console.WriteLine();
        PrintHelp();
        return 1;
    }
}

public static class GenerateCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        string project = Directory.GetCurrentDirectory();
        string output = "docs";
        OutputMode mode = OutputMode.Both;
        string? grokKey = null;
        bool verbose = false;
        bool useGitignore = true;
        string? ignoreFile = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help":
                    PrintHelp();
                    return 0;
                case "-p" or "--project":
                    project = GetNextArg(args, ref i, "--project");
                    break;
                case "-o" or "--output":
                    output = GetNextArg(args, ref i, "--output");
                    break;
                case "-m" or "--mode":
                    string modeStr = GetNextArg(args, ref i, "--mode");
                    if (!Enum.TryParse(modeStr, true, out mode))
                    {
                        Console.WriteLine($"❌ Invalid mode '{modeStr}'. Use: Both, Human, or Llm");
                        return 1;
                    }
                    break;
                case "-g" or "--grok-key":
                    grokKey = GetNextArg(args, ref i, "--grok-key");
                    break;
                case "-v" or "--verbose":
                    verbose = true;
                    break;
                case "--no-gitignore":
                    useGitignore = false;
                    break;
                case "-i" or "--ignore-file":
                    ignoreFile = GetNextArg(args, ref i, "--ignore-file");
                    break;
                default:
                    Console.WriteLine($"❌ Unknown option '{args[i]}'");
                    return 1;
            }
        }

        return await ExecuteAsync(project, output, mode, grokKey, verbose, useGitignore, ignoreFile);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: docgen generate [options]");
        Console.WriteLine();
        Console.WriteLine("Generate documentation for a .NET project.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -p, --project <path>     Project directory (default: current directory)");
        Console.WriteLine("  -o, --output <name>      Output file base name (default: docs)");
        Console.WriteLine("  -m, --mode <mode>        Output mode: Both, Human, or Llm (default: Both)");
        Console.WriteLine("  -g, --grok-key <key>     Grok API key for AI enhancements (Human mode only)");
        Console.WriteLine("  -v, --verbose            Verbose output");
        Console.WriteLine("  --no-gitignore           Don't use .gitignore patterns");
        Console.WriteLine("  -i, --ignore-file <path> Custom ignore file path");
        Console.WriteLine("  -h, --help               Show this help");
        Console.WriteLine();
        Console.WriteLine("Output files:");
        Console.WriteLine("  <output>-human.md        Human-readable documentation");
        Console.WriteLine("  <output>-llm.txt         LLM-optimized documentation");
    }

    private static async Task<int> ExecuteAsync(
        string project,
        string output,
        OutputMode mode,
        string? grokKey,
        bool verbose,
        bool useGitignore,
        string? ignoreFile)
    {
        // Crear logger que escribe a Console
        ILogger logger = new ConsoleLogger(verbose, Console.WriteLine);

        Console.WriteLine("CdCSharp Documentation Generator");
        Console.WriteLine(new string('=', 40));
        Console.WriteLine();

        if (!Directory.Exists(project))
        {
            logger.Error($"Directory not found: {project}");
            return 1;
        }

        try
        {
            // Escanear proyecto con logger
            ProjectScanner scanner = new(logger);
            (ProjectInfo projectInfo, List<ComponentInfo> components) =
                await scanner.ScanAsync(project, useGitignore, ignoreFile);

            Console.WriteLine();
            Console.WriteLine("PROJECT SUMMARY");
            Console.WriteLine($"  Name:        {projectInfo.Name}");
            Console.WriteLine($"  Type:        {projectInfo.Type}");
            Console.WriteLine($"  Files:       {projectInfo.Files.Count}");
            Console.WriteLine($"  Public API:  {projectInfo.PublicTypes.Count} types");
            Console.WriteLine($"  Components:  {components.Count}");
            Console.WriteLine($"  Patterns:    {projectInfo.Patterns.Count}");

            int totalLines = projectInfo.Files.Sum(f => f.LineCount);
            int totalTokens = projectInfo.Files.Sum(f => f.TokenEstimate);
            Console.WriteLine($"  Lines:       {totalLines:N0}");
            Console.WriteLine($"  Est. tokens: {totalTokens:N0}");
            Console.WriteLine();

            // Configurar AI si hay key
            IAiClient? ai = null;
            if (!string.IsNullOrWhiteSpace(grokKey) && mode != OutputMode.Llm)
            {
                logger.Info("AI enhancement enabled (Grok)");
                ai = new GrokClient(grokKey, logger);
            }

            try
            {
                // Generar documentación
                if (mode is OutputMode.Both or OutputMode.Human)
                {
                    logger.Info("Generating human-readable documentation...");
                    HumanFormatter formatter = new(ai, logger);
                    string doc = await formatter.FormatAsync(projectInfo, components);
                    string path = $"{output}-human.md";
                    await File.WriteAllTextAsync(path, doc);

                    int tokens = doc.Length / 4;
                    Console.WriteLine($"✓ Generated: {path} ({tokens:N0} tokens)");
                }

                if (mode is OutputMode.Both or OutputMode.Llm)
                {
                    logger.Info("Generating LLM-optimized documentation...");
                    LlmFormatter formatter = new(logger);
                    string doc = await formatter.FormatAsync(projectInfo, components);
                    string path = $"{output}-llm.txt";
                    await File.WriteAllTextAsync(path, doc);

                    int tokens = doc.Length / 4;
                    Console.WriteLine($"✓ Generated: {path} ({tokens:N0} tokens)");
                }

                Console.WriteLine();
                Console.WriteLine("✓ Done!");
                return 0;
            }
            finally
            {
                ai?.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
            }
            return 1;
        }
    }

    private static string GetNextArg(string[] args, ref int i, string optionName)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Option '{optionName}' requires a value");
        return args[++i];
    }
}

public static class AnalyzeCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        string project = Directory.GetCurrentDirectory();
        bool useGitignore = true;
        string? ignoreFile = null;
        bool verbose = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help":
                    PrintHelp();
                    return 0;
                case "-p" or "--project":
                    project = GetNextArg(args, ref i, "--project");
                    break;
                case "--no-gitignore":
                    useGitignore = false;
                    break;
                case "-i" or "--ignore-file":
                    ignoreFile = GetNextArg(args, ref i, "--ignore-file");
                    break;
                case "-v" or "--verbose":
                    verbose = true;
                    break;
                default:
                    Console.WriteLine($"❌ Unknown option '{args[i]}'");
                    return 1;
            }
        }

        return await ExecuteAsync(project, useGitignore, ignoreFile, verbose);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: docgen analyze [options]");
        Console.WriteLine();
        Console.WriteLine("Analyze project structure without generating documentation.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -p, --project <path>     Project directory (default: current directory)");
        Console.WriteLine("  --no-gitignore           Don't use .gitignore patterns");
        Console.WriteLine("  -i, --ignore-file <path> Custom ignore file path");
        Console.WriteLine("  -v, --verbose            Verbose output");
        Console.WriteLine("  -h, --help               Show this help");
    }

    private static async Task<int> ExecuteAsync(
        string project,
        bool useGitignore,
        string? ignoreFile,
        bool verbose)
    {
        // Crear logger verbose para análisis
        ILogger logger = new ConsoleLogger(verbose, Console.WriteLine);

        Console.WriteLine("CdCSharp Project Analyzer");
        Console.WriteLine(new string('=', 40));
        Console.WriteLine();

        if (!Directory.Exists(project))
        {
            logger.Error($"Directory not found: {project}");
            return 1;
        }

        try
        {
            ProjectScanner scanner = new(logger);
            (ProjectInfo projectInfo, List<ComponentInfo> components) =
                await scanner.ScanAsync(project, useGitignore, ignoreFile);

            Console.WriteLine();
            Console.WriteLine("PROJECT INFO");
            Console.WriteLine($"  Name: {projectInfo.Name}");
            Console.WriteLine($"  Type: {projectInfo.Type}");
            Console.WriteLine($"  Gitignore: {(useGitignore ? "enabled" : "disabled")}");
            if (!string.IsNullOrWhiteSpace(ignoreFile))
                Console.WriteLine($"  Custom ignore: {ignoreFile}");

            Console.WriteLine();
            Console.WriteLine("FILE STATISTICS");
            foreach (IGrouping<FileType, CdCSharp.DocGen.Core.Models.FileInfo> g in
                projectInfo.Files.GroupBy(f => f.Type).OrderByDescending(g => g.Count()))
            {
                int count = g.Count();
                int lines = g.Sum(f => f.LineCount);
                int tokens = g.Sum(f => f.TokenEstimate);
                Console.WriteLine($"  {g.Key,-12} {count,4} files  {lines,7:N0} lines  ~{tokens,7:N0} tokens");
            }

            int totalFiles = projectInfo.Files.Count;
            int totalLines = projectInfo.Files.Sum(f => f.LineCount);
            int totalTokens = projectInfo.Files.Sum(f => f.TokenEstimate);
            Console.WriteLine($"  {"TOTAL",-12} {totalFiles,4} files  {totalLines,7:N0} lines  ~{totalTokens,7:N0} tokens");

            Console.WriteLine();
            Console.WriteLine("IMPORTANCE");
            foreach (IGrouping<ImportanceLevel, CdCSharp.DocGen.Core.Models.FileInfo> g in
                projectInfo.Files.GroupBy(f => f.Importance).OrderByDescending(g => g.Key))
                Console.WriteLine($"  {g.Key,-10} {g.Count(),4} files");

            Console.WriteLine();
            Console.WriteLine("PUBLIC API");
            Console.WriteLine($"  Total types: {projectInfo.PublicTypes.Count}");
            foreach (IGrouping<TypeKind, TypeInfo> g in
                projectInfo.PublicTypes.GroupBy(t => t.Kind).OrderByDescending(g => g.Count()))
                Console.WriteLine($"  {g.Key,-10} {g.Count(),4}");

            List<TypeInfo> critical = projectInfo.PublicTypes
                .Where(t => t.Importance == ImportanceLevel.Critical)
                .ToList();

            if (critical.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine($"  Critical types: {critical.Count}");
                foreach (TypeInfo t in critical)
                    Console.WriteLine($"    - {t.Namespace}.{t.Name}");
            }

            if (components.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("BLAZOR COMPONENTS");
                Console.WriteLine($"  Total: {components.Count}");
                foreach (ComponentInfo c in components.Take(10))
                {
                    string pars = c.Parameters.Count > 0 ? $" ({c.Parameters.Count} params)" : "";
                    Console.WriteLine($"    - {c.Name}{pars}");
                }
                if (components.Count > 10)
                    Console.WriteLine($"    ... and {components.Count - 10} more");
            }

            if (projectInfo.Patterns.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("PATTERNS");
                foreach (PatternInfo p in projectInfo.Patterns)
                {
                    Console.WriteLine($"  {p.Name} ({p.Type})");
                    Console.WriteLine($"    Files: {p.AffectedFiles.Count}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("RECOMMENDATIONS");

            if (totalTokens > 100000)
                Console.WriteLine("  [!] Large codebase (>100k tokens) - AI summaries recommended");

            if (critical.Count > 0)
                Console.WriteLine("  [+] Critical types detected - AI enhancement recommended");

            if (projectInfo.Patterns.Count >= 3)
                Console.WriteLine("  [+] Multiple patterns detected - good for architectural overview");

            if (!useGitignore)
                Console.WriteLine("  [!] Gitignore disabled - may include unnecessary files");

            Console.WriteLine();
            Console.WriteLine("✓ Analysis complete");
            return 0;
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
            }
            return 1;
        }
    }

    private static string GetNextArg(string[] args, ref int i, string optionName)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Option '{optionName}' requires a value");
        return args[++i];
    }
}

public static class TestIgnoreCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        string project = Directory.GetCurrentDirectory();
        string? testPath = null;
        bool verbose = false;
        bool useGitignore = true;
        string? ignoreFile = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help":
                    PrintHelp();
                    return 0;
                case "-p" or "--project":
                    project = GetNextArg(args, ref i, "--project");
                    break;
                case "-t" or "--test":
                    testPath = GetNextArg(args, ref i, "--test");
                    break;
                case "--no-gitignore":
                    useGitignore = false;
                    break;
                case "-i" or "--ignore-file":
                    ignoreFile = GetNextArg(args, ref i, "--ignore-file");
                    break;
                case "-v" or "--verbose":
                    verbose = true;
                    break;
                default:
                    Console.WriteLine($"❌ Unknown option '{args[i]}'");
                    return 1;
            }
        }

        return await ExecuteAsync(project, testPath, useGitignore, ignoreFile, verbose);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: docgen test-ignore [options]");
        Console.WriteLine();
        Console.WriteLine("Test ignore patterns to see what files would be excluded.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -p, --project <path>     Project directory (default: current directory)");
        Console.WriteLine("  -t, --test <path>        Test specific file/directory path");
        Console.WriteLine("  --no-gitignore           Don't use .gitignore patterns");
        Console.WriteLine("  -i, --ignore-file <path> Custom ignore file path");
        Console.WriteLine("  -v, --verbose            Verbose output");
        Console.WriteLine("  -h, --help               Show this help");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  docgen test-ignore");
        Console.WriteLine("  docgen test-ignore -t src/bin/Debug");
        Console.WriteLine("  docgen test-ignore -t MyFile.cs -v");
    }

    private static async Task<int> ExecuteAsync(
        string project,
        string? testPath,
        bool useGitignore,
        string? ignoreFile,
        bool verbose)
    {
        ILogger logger = new ConsoleLogger(verbose, Console.WriteLine);

        Console.WriteLine("CdCSharp Ignore Pattern Tester");
        Console.WriteLine(new string('=', 40));
        Console.WriteLine();

        if (!Directory.Exists(project))
        {
            logger.Error($"Directory not found: {project}");
            return 1;
        }

        try
        {
            // Cargar patrones
            (GitignorePatternMatcher matcher, string ignoreSource) =
                await IgnoreFileLoader.LoadAsync(project, ignoreFile, useGitignore, logger);

            Console.WriteLine($"Project: {project}");
            Console.WriteLine($"Patterns: {ignoreSource}");
            Console.WriteLine();

            if (!string.IsNullOrWhiteSpace(testPath))
            {
                // Probar un path específico
                string fullPath = Path.IsPathRooted(testPath)
                    ? testPath
                    : Path.Combine(project, testPath);

                Console.WriteLine($"Testing: {testPath}");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine();

                bool ignored = matcher.IsIgnored(fullPath);
                Console.WriteLine($"Result: {(ignored ? "❌ IGNORED" : "✅ INCLUDED")}");

                if (verbose)
                {
                    Console.WriteLine();
                    Console.WriteLine("Debug info:");
                    Console.WriteLine(matcher.DebugMatch(fullPath));
                }
            }
            else
            {
                // Escanear todos los archivos y mostrar estadísticas
                Console.WriteLine("Scanning all files...");
                Console.WriteLine();

                string[] allFiles = Directory.GetFiles(project, "*.*", SearchOption.AllDirectories);
                int totalFiles = allFiles.Length;
                int ignoredCount = 0;
                int includedCount = 0;

                List<string> ignoredSamples = [];
                List<string> includedSamples = [];

                foreach (string file in allFiles)
                {
                    bool ignored = matcher.IsIgnored(file);
                    if (ignored)
                    {
                        ignoredCount++;
                        if (ignoredSamples.Count < 10)
                            ignoredSamples.Add(Path.GetRelativePath(project, file));
                    }
                    else
                    {
                        includedCount++;
                        if (includedSamples.Count < 10)
                            includedSamples.Add(Path.GetRelativePath(project, file));
                    }
                }

                Console.WriteLine($"Total files:    {totalFiles:N0}");
                Console.WriteLine($"Ignored:        {ignoredCount:N0} ({100.0 * ignoredCount / totalFiles:F1}%)");
                Console.WriteLine($"Included:       {includedCount:N0} ({100.0 * includedCount / totalFiles:F1}%)");
                Console.WriteLine();

                if (ignoredSamples.Count > 0)
                {
                    Console.WriteLine("Sample ignored files:");
                    foreach (string f in ignoredSamples)
                        Console.WriteLine($"  ❌ {f}");
                    Console.WriteLine();
                }

                if (includedSamples.Count > 0)
                {
                    Console.WriteLine("Sample included files:");
                    foreach (string f in includedSamples)
                        Console.WriteLine($"  ✅ {f}");
                    Console.WriteLine();
                }

                // Mostrar archivos bin/obj que NO fueron ignorados (posible problema)
                List<string> problematic = allFiles
                    .Where(f => !matcher.IsIgnored(f))
                    .Where(f => f.Contains("/bin/") || f.Contains("/obj/") ||
                               f.Contains("\\bin\\") || f.Contains("\\obj\\"))
                    .Select(f => Path.GetRelativePath(project, f))
                    .Take(20)
                    .ToList();

                if (problematic.Count > 0)
                {
                    Console.WriteLine("⚠️  WARNING: bin/obj files NOT ignored:");
                    foreach (string f in problematic)
                        Console.WriteLine($"  ⚠️  {f}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("✅ Test complete");
            return 0;
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
            }
            return 1;
        }
    }

    private static string GetNextArg(string[] args, ref int i, string optionName)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Option '{optionName}' requires a value");
        return args[++i];
    }
}