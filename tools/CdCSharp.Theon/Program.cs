using CdCSharp.Theon;
using CdCSharp.Theon.Analysis;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Models;
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
ServiceProvider provider = services.BuildServiceProvider();

TheonLogger logger = provider.GetRequiredService<TheonLogger>();
ProjectScanner scanner = provider.GetRequiredService<ProjectScanner>();
Orchestrator orchestrator = provider.GetRequiredService<Orchestrator>();

logger.Info("=".PadRight(50, '='));
logger.Info("  THEON - Multi-Agent Code Analysis System");
logger.Info("=".PadRight(50, '='));
logger.Info($"Project: {options.ProjectPath}");
logger.Info("");

logger.Info("Scanning project structure...");
ProjectStructure structure = await scanner.ScanAsync(options.ProjectPath);

logger.Info($"Found {structure.Assemblies.Count} assemblies");
logger.Info($"  Types: {structure.Summary.TotalTypes}");
logger.Info($"  Files: {structure.Summary.TotalFiles}");
if (structure.Summary.DetectedPatterns.Count > 0)
    logger.Info($"  Patterns: {string.Join(", ", structure.Summary.DetectedPatterns)}");

orchestrator.SetProjectStructure(structure);

logger.Info("");
logger.Info("Ready. Type your query (or 'exit' to quit):");
logger.Info("");

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("> ");
    Console.ResetColor();

    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        logger.Info("Goodbye!");
        break;
    }

    if (input.Equals("agents", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine(provider.GetRequiredService<CdCSharp.Theon.Agents.AgentRegistry>().GetAgentsSummary());
        continue;
    }

    try
    {
        ResponseOutput response = await orchestrator.ProcessQueryAsync(input);

        logger.Info("");
        logger.Info($"Response saved to: {response.FolderPath}");
        logger.Info($"  Agents: {string.Join(", ", response.Metadata.AgentsInvolved)}");
        logger.Info($"  Confidence: {response.Metadata.FinalConfidence:P0}");
        logger.Info($"  Time: {response.Metadata.ProcessingTime.TotalSeconds:F1}s");

        if (response.Files.Count > 0)
            logger.Info($"  Generated files: {response.Files.Count}");

        logger.Info("");
    }
    catch (Exception ex)
    {
        logger.Error("Failed to process query", ex);
    }
}

return 0;