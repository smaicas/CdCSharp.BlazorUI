using CdCSharp.BlazorUI.BuildTools;
using CdCSharp.BlazorUI.Core.Theming.Themes;

if (args.Length == 0)
{
    PrintUsage();
    Environment.Exit(1);
}

string command = args[0].ToLower();

try
{
    switch (command)
    {
        case "init":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Project path required for init command");
                Environment.Exit(1);
            }
            await ConfigInitializer.InitializeProjectConfigs(args[1]);
            break;

        case "themes":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Output path required for themes command");
                Environment.Exit(1);
            }
            GenerateThemes(args[1]);
            break;

        case "npm":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Project path required for npm command");
                Environment.Exit(1);
            }
            await NpmManager.EnsureNpmInstalled(args[1]);
            break;

        case "js":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Project path required for js command");
                Environment.Exit(1);
            }
            await JavaScriptBuilder.Build(args[1]);
            break;

        case "css":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Project path required for css command");
                Environment.Exit(1);
            }
            await CssBuilder.Build(args[1]);
            break;

        case "all":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Project path required for all command");
                Environment.Exit(1);
            }
            await BuildAll(args[1]);
            break;

        default:
            Console.WriteLine($"Unknown command: {command}");
            PrintUsage();
            Environment.Exit(1);
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

void PrintUsage()
{
    Console.WriteLine("Usage: blazorui-build-tool <command> [options]");
    Console.WriteLine("Commands:");
    Console.WriteLine("  init <project-path>      - Initialize project configuration files");
    Console.WriteLine("  themes <output-path>     - Generate theme CSS");
    Console.WriteLine("  npm <project-path>       - Run npm install if needed");
    Console.WriteLine("  js <project-path>        - Build JavaScript with Vite");
    Console.WriteLine("  css <project-path>       - Build CSS with Vite");
    Console.WriteLine("  all <project-path>       - Run all build tasks");
}

void GenerateThemes(string outputPath)
{
    string cssContent = CssGenerator.Generate("dark", [new DarkTheme(), new LightTheme()]);

    string? directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.WriteAllText(outputPath, cssContent);
    Console.WriteLine($"CSS themes generated successfully at: {outputPath}");
}

void GenerateResetCss(string outputPath)
{
    string cssContent = CssReset.GetCss();

    string? directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.WriteAllText(outputPath, cssContent);
    Console.WriteLine($"CSS reset generated successfully at: {outputPath}");
}

void GenerateInitializeThemesCss(string outputPath)
{
    string cssContent = CssInitializeTheme.GetCss();

    string? directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.WriteAllText(outputPath, cssContent);
    Console.WriteLine($"CSS reset generated successfully at: {outputPath}");
}

async Task BuildAll(string projectPath)
{
    // 0. Generate CSS Reset
    string cssResetPath = Path.Combine(projectPath, "CssBundle", "reset.css");
    GenerateResetCss(cssResetPath);

    // 1. Generate themes
    string themesPath = Path.Combine(projectPath, "CssBundle", "themes.css");
    GenerateThemes(themesPath);

    string initializeThemesPath = Path.Combine(projectPath, "CssBundle", "initialize-themes.css");
    GenerateInitializeThemesCss(initializeThemesPath);

    // 2. Ensure npm packages are installed
    await NpmManager.EnsureNpmInstalled(projectPath);

    // 3. Build CSS
    await CssBuilder.Build(projectPath);

    // 4. Build JavaScript
    await JavaScriptBuilder.Build(projectPath);

    Console.WriteLine("All build tasks completed successfully!");
}