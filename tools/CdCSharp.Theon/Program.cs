using CdCSharp.Theon;
using CdCSharp.Theon.AI;
using CdCSharp.Theon.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

string projectPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

if (!Directory.Exists(projectPath))
{
    Console.WriteLine($"Error: Directory not found: {projectPath}");
    return 1;
}
Banner.ShowTheonBanner(projectPath);

ServiceCollection services = new();
services.AddTheon(options => options.ProjectPath = Path.GetFullPath(projectPath));

await using ServiceProvider provider = services.BuildServiceProvider();

ITheonLogger logger = provider.GetRequiredService<ITheonLogger>();

if (provider.GetRequiredService<IAIClient>() is LMStudioClient lmClient)
{
    await lmClient.ValidateCapabilities();
}

//while (true)
//{
//    Console.ForegroundColor = ConsoleColor.Green;
//    Console.Write("> ");
//    Console.ResetColor();

//    string? input = Console.ReadLine();

//    if (string.IsNullOrWhiteSpace(input))
//        continue;

//    string command = input.Trim();

//    if (command.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
//        command.Equals("quit", StringComparison.OrdinalIgnoreCase))
//    {
//        logger.Info("Peace!");
//        break;
//    }

//    try
//    {
//        OrchestratorResponse response = await orchestrator.ProcessAsync(input);

//        Console.WriteLine();
//        Console.ForegroundColor = ConsoleColor.White;
//        Console.WriteLine(response.Content);
//        Console.ResetColor();
//        Console.WriteLine();

//        logger.Info($"Confidence: {response.Confidence:P0}");

//        if (response.OutputFiles.Count > 0)
//            logger.Info($"Generated files: {string.Join(", ", response.OutputFiles.Select(f => f.Name))}");

//        if (response.ModifiedProjectFiles.Count > 0)
//            logger.Info($"Modified: {string.Join(", ", response.ModifiedProjectFiles)}");

//        Console.WriteLine();
//    }
//    catch (Exception ex)
//    {
//        logger.Error("Failed to process query", ex);
//    }

//}
return 0;

class Banner
{
    public static void ShowTheonBanner(string projectPath)
    {
        string[] ascii =
        {
        "       ████████╗██╗  ██╗███████╗ ██████╗ ███╗   ██╗",
        "       ╚══██╔══╝██║  ██║██╔════╝██╔═══██╗████╗  ██║",
        "          ██║   ███████║█████╗  ██║   ██║██╔██╗ ██║",
        "          ██║   ██╔══██║██╔══╝  ██║   ██║██║╚██╗██║",
        "          ██║   ██║  ██║███████╗╚██████╔╝██║ ╚████║",
        "          ╚═╝   ╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═╝  ╚═══╝"
    };

        int consoleWidth = Console.WindowWidth;
        int asciiWidth = ascii[0].Length;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', consoleWidth));
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.DarkRed;
        foreach (string line in ascii)
        {
            int pad = (consoleWidth - line.Length) / 2;
            Console.WriteLine(new string(' ', pad) + line);
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', consoleWidth));
        Console.WriteLine();

        string subtitle = $"Project: {projectPath}";
        int subtitlePad = (consoleWidth - subtitle.Length) / 2;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(new string(' ', subtitlePad) + subtitle);
        Console.WriteLine(new string(' ', subtitlePad) + "Type your query to analyze the project.");

        Console.WriteLine();
        Console.ResetColor();
    }
}