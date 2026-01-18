using CdCSharp.Theon;
using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using CdCSharp.Theon.Orchestrator;
using CdCSharp.Theon.Orchestrator.Models;
using Microsoft.Extensions.DependencyInjection;

string projectPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

if (!Directory.Exists(projectPath))
{
    Console.WriteLine($"Error: Directory not found: {projectPath}");
    return 1;
}

ShowBanner(projectPath);

ServiceCollection services = new();
services.AddTheon(options => options.ProjectPath = Path.GetFullPath(projectPath));

await using ServiceProvider provider = services.BuildServiceProvider();

ITheonLogger logger = provider.GetRequiredService<ITheonLogger>();
IOrchestrator orchestrator = provider.GetRequiredService<IOrchestrator>();

if (provider.GetRequiredService<IAIClient>() is LMStudioClient lmClient)
{
    await lmClient.ValidateCapabilities();
}

logger.Section("Ready");
logger.Info("Type your query or 'exit' to quit.");
Console.WriteLine();

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

    if (command.Equals("reset", StringComparison.OrdinalIgnoreCase))
    {
        orchestrator.Reset();
        logger.Info("Conversation reset.");
        Console.WriteLine();
        continue;
    }

    if (command.Equals("state", StringComparison.OrdinalIgnoreCase))
    {
        ShowStatus(orchestrator.State);
        continue;
    }

    try
    {
        OrchestratorResponse response = await orchestrator.ProcessAsync(command);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(response.Message);
        Console.ResetColor();
        Console.WriteLine();

        if (response.CreatedFiles.Count > 0)
        {
            logger.Success($"Created in project: {string.Join(", ", response.CreatedFiles)}");
        }

        if (response.GeneratedOutputs.Count > 0)
        {
            logger.Success($"Generated outputs: {string.Join(", ", response.GeneratedOutputs)}");
        }

        if (response.ProposedChanges.Count > 0)
        {
            ShowProposedChanges(response.ProposedChanges);
        }

        if (response.NeedsConfirmation)
        {
            bool confirmed = await HandleConfirmation(orchestrator, logger);
            if (confirmed)
            {
                Console.WriteLine();
            }
        }

        Console.WriteLine();
    }
    catch (Exception ex)
    {
        logger.Error("Failed to process query", ex);
        Console.WriteLine();
    }
}

return 0;

static void ShowStatus(OrchestratorState state)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("=== Status ===");
    Console.ResetColor();

    Console.WriteLine($"Conversation messages: {state.ConversationHistory.Count}");
    Console.WriteLine($"Active contexts: {string.Join(", ", state.ActiveContexts.Keys)}");
    Console.WriteLine($"Pending changes: {state.GetPendingChanges().Count()}");
    Console.WriteLine($"Estimated tokens: {state.EstimatedTokens}");
    Console.WriteLine();
}

static void ShowProposedChanges(List<ProposedChange> changes)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Proposed changes:");
    Console.ResetColor();

    foreach (ProposedChange change in changes.Where(c => c.Status == ChangeStatus.Pending))
    {
        Console.WriteLine($"  [{change.Id}] {change.ChangeType}: {change.Path}");
        Console.WriteLine($"       {change.Description}");
    }
}

static async Task<bool> HandleConfirmation(IOrchestrator orchestrator, ITheonLogger logger)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Apply changes? (y/n/id): ");
    Console.ResetColor();

    string? confirmation = Console.ReadLine()?.Trim().ToLowerInvariant();

    if (string.IsNullOrEmpty(confirmation) || confirmation == "n" || confirmation == "no")
    {
        await orchestrator.ConfirmChangesAsync(false);
        logger.Info("Changes rejected.");
        return false;
    }

    string? changeIds = null;
    if (confirmation is not "y" and not "yes" and not "all")
    {
        changeIds = confirmation;
    }

    OrchestratorResponse result = await orchestrator.ConfirmChangesAsync(true, changeIds);
    logger.Success(result.Message);
    return true;
}

static void ShowBanner(string projectPath)
{
    string[] ascii =
    [
        "       ████████╗██╗  ██╗███████╗ ██████╗ ███╗   ██╗",
        "       ╚══██╔══╝██║  ██║██╔════╝██╔═══██╗████╗  ██║",
        "          ██║   ███████║█████╗  ██║   ██║██╔██╗ ██║",
        "          ██║   ██╔══██║██╔══╝  ██║   ██║██║╚██╗██║",
        "          ██║   ██║  ██║███████╗╚██████╔╝██║ ╚████║",
        "          ╚═╝   ╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═╝  ╚═══╝"
    ];

    int consoleWidth = Console.WindowWidth;

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(new string('═', consoleWidth));
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (string line in ascii)
    {
        int pad = Math.Max(0, (consoleWidth - line.Length) / 2);
        Console.WriteLine(new string(' ', pad) + line);
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(new string('═', consoleWidth));
    Console.WriteLine();

    string subtitle = $"Project: {projectPath}";
    int subtitlePad = Math.Max(0, (consoleWidth - subtitle.Length) / 2);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(new string(' ', subtitlePad) + subtitle);

    string help = "Commands: exit, reset, state";
    int helpPad = Math.Max(0, (consoleWidth - help.Length) / 2);
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(new string(' ', helpPad) + help);

    Console.WriteLine();
    Console.ResetColor();
}