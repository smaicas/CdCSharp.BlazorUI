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
PreAnalyzer scanner = provider.GetRequiredService<PreAnalyzer>();
Orchestrator orchestrator = provider.GetRequiredService<Orchestrator>();

logger.Info("=".PadRight(50, '='));
logger.Info("  THEON - Multi-Agent Code Analysis System");
logger.Info("=".PadRight(50, '='));
logger.Info($"Project: {options.ProjectPath}");
logger.Info("");

logger.Info("Scanning project structure...");
PreAnalysisResult preAnalysis = await scanner.AnalyzeAsync(options.ProjectPath);
//ProjectStructure structure = preAnalysis.Structure;

logger.Info($"Found {preAnalysis.Structure.Assemblies.Count} assemblies");
logger.Info($"  Types: {preAnalysis.Structure.Summary.TotalTypes}");
logger.Info($"  Files: {preAnalysis.Structure.Summary.TotalFiles}");
if (preAnalysis.Structure.Summary.DetectedPatterns.Count > 0)
    logger.Info($"  Patterns: {string.Join(", ", preAnalysis.Structure.Summary.DetectedPatterns)}");

orchestrator.SetProjectStructure(preAnalysis);

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

    string command = input.Trim().ToLowerInvariant();

    // Comandos especiales
    if (command is "exit" or "quit")
    {
        logger.Info("Goodbye!");
        break;
    }

    if (command == "agents")
    {
        AgentVisualizer visualizer = provider.GetRequiredService<AgentVisualizer>();
        Console.WriteLine(visualizer.GenerateTextSummary());
        continue;
    }

    if (command == "metrics")
    {
        MetricsCollector metrics = provider.GetRequiredService<MetricsCollector>();
        Console.WriteLine(metrics.GenerateReport());
        continue;
    }

    if (command == "save")
    {
        string path = await orchestrator.SaveSessionAsync();
        logger.Info($"Session saved to: {path}");
        continue;
    }

    if (command.StartsWith("load "))
    {
        string sessionId = input[5..].Trim();
        bool loaded = await orchestrator.LoadSessionAsync(sessionId);
        if (loaded)
            logger.Info("Session loaded successfully");
        else
            logger.Warning("Failed to load session");
        continue;
    }

    if (command == "sessions")
    {
        SessionManager sessionMgr = provider.GetRequiredService<SessionManager>();
        List<SessionInfo> sessions = sessionMgr.ListSessions();

        if (sessions.Count == 0)
        {
            Console.WriteLine("No saved sessions found.");
        }
        else
        {
            Console.WriteLine("Available sessions:");
            foreach (SessionInfo session in sessions)
            {
                Console.WriteLine($"  - {session.SessionId} ({session.SavedAt:yyyy-MM-dd HH:mm}) - {session.AgentCount} agents");
            }
        }
        continue;
    }

    if (command == "help")
    {
        Console.WriteLine("""
            Available commands:
              agents    - Show all agents and their status
              metrics   - Show performance metrics
              save      - Save current session
              load <id> - Load a saved session
              sessions  - List available sessions
              exit      - Exit the application
              
            Or type any query to analyze your code.
            """);
        continue;
    }

    // Procesar query normal
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