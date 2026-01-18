using CdCSharp.Theon;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Core;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestration;
using Microsoft.Extensions.DependencyInjection;

string projectPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

if (!Directory.Exists(projectPath))
{
    Console.WriteLine($"Error: Directory not found: {projectPath}");
    return 1;
}

TheonOptions options = new()
{
    ProjectPath = Path.GetFullPath(projectPath)
};

ServiceCollection services = new();
services.AddTheon(options);
await using ServiceProvider provider = services.BuildServiceProvider();

ITheonLogger logger = provider.GetRequiredService<ITheonLogger>();
IProjectAnalysis analysis = provider.GetRequiredService<IProjectAnalysis>();
IOrchestrator orchestrator = provider.GetRequiredService<IOrchestrator>();
ILlmClient llmClient = provider.GetRequiredService<ILlmClient>();

logger.Info("═══════════════════════════════════════════════════");
logger.Info("  THEON - Code Analysis System");
logger.Info("═══════════════════════════════════════════════════");
logger.Info($"Project: {options.ProjectPath}");
logger.Info("");

await analysis.AnalyzeAsync();

if (analysis.Project == null)
{
    logger.Error("Failed to analyze project");
    return 1;
}

ModelInfo modelInfo = await llmClient.GetModelInfoAsync();

logger.Info("");
logger.Info($"Assemblies: {analysis.Project.Assemblies.Count}");
logger.Info($"Types: {analysis.Project.Assemblies.Sum(a => a.Types.Count)}");
logger.Info($"Files: {analysis.Project.Assemblies.Sum(a => a.Files.Count)}");
logger.Info("");
logger.Info("Commands:");
logger.Info("  @file:<path> <query>      - Query with file context");
logger.Info("  @folder:<path> <query>    - Query with folder context");
logger.Info("  @assembly:<name> <query>  - Query with assembly context");
logger.Info("  exit                      - Exit");
logger.Info("");
logger.Info("Ready. Enter your query:");
logger.Info("");

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("> ");
    Console.ResetColor();

    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    string command = input.Trim();

    if (command.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        command.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        logger.Info("Goodbye!");
        break;
    }

    if (command.Equals("help", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("""
            Commands:
              @file:<path> <query>      - Query with specific file context
              @folder:<path> <query>    - Query with folder context
              @assembly:<name> <query>  - Query with assembly context
              exit                      - Exit
              
            Or just type your query to analyze the project.
            """);
        continue;
    }

    try
    {
        OrchestratorResponse response = await orchestrator.ProcessAsync(input);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(response.Content);
        Console.ResetColor();
        Console.WriteLine();

        logger.Info($"Confidence: {response.Confidence:P0}");

        if (response.OutputFiles.Count > 0)
            logger.Info($"Generated files: {string.Join(", ", response.OutputFiles.Select(f => f.Name))}");

        if (response.ModifiedProjectFiles.Count > 0)
            logger.Info($"Modified: {string.Join(", ", response.ModifiedProjectFiles)}");

        Console.WriteLine();
    }
    catch (Exception ex)
    {
        logger.Error("Failed to process query", ex);
    }
}

return 0;