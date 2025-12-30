using CdCSharp.BlazorUI.BuildTools.Generators;
using CdCSharp.BlazorUI.BuildTools.Infrastructure;

namespace CdCSharp.BlazorUI.BuildTools.Pipeline;

public class BuildPipeline
{
    private readonly string _projectPath;
    private readonly BuildContext _context;
    private readonly NodeToolsManager _nodeTools;

    public BuildPipeline(string projectPath)
    {
        _projectPath = Path.GetFullPath(projectPath);
        _context = new BuildContext(_projectPath);
        _nodeTools = new NodeToolsManager(_context);
    }

    public async Task ExecuteAsync()
    {
        Console.WriteLine("=== BlazorUI Build Pipeline ===");
        Console.WriteLine($"Project: {_projectPath}");
        Console.WriteLine();

        try
        {
            await Step1_InitializeAsync();
            await Step2_GenerateAssetsAsync();
            await Step3_BuildAssetsAsync();

            Console.WriteLine();
            Console.WriteLine("=== Build completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"=== Build failed: {ex.Message} ===");
            throw;
        }
    }

    private async Task Step1_InitializeAsync()
    {
        Console.WriteLine("[1/3] Initializing...");

        // Verify Node.js is installed
        await _nodeTools.VerifyNodeInstalledAsync();

        // Create required directories
        _context.EnsureDirectory("CssBundle");
        _context.EnsureDirectory("wwwroot/css");
        _context.EnsureDirectory("wwwroot/js");
        _context.EnsureDirectory("Types");

        // Initialize configuration files
        BuildConfigurationManager configManager = new(_context);
        await configManager.InitializeAsync();

        // Install npm packages if needed
        await _nodeTools.EnsurePackagesInstalledAsync();

        Console.WriteLine("    ✓ Initialization complete");
    }

    private async Task Step2_GenerateAssetsAsync()
    {
        Console.WriteLine("[2/3] Generating assets...");

        // Generate CSS files from C# tokens
        List<IAssetGenerator> generators =
        [
            new CssResetGenerator(_context),
            new ThemesCssGenerator(_context),
            new ComponentsCssGenerator(_context),
            new TransitionsCssGenerator(_context),
            new CssInitializeThemesGenerator(_context)
        ];

        foreach (IAssetGenerator generator in generators)
        {
            Console.WriteLine($"    - Generating {generator.Name}...");
            await generator.GenerateAsync();
        }

        Console.WriteLine("    ✓ Asset generation complete");
    }

    private async Task Step3_BuildAssetsAsync()
    {
        Console.WriteLine("[3/3] Building assets...");

        // Build CSS bundle
        Console.WriteLine("    - Building CSS bundle...");
        await _nodeTools.BuildCssAsync();

        // Build JavaScript/TypeScript
        Console.WriteLine("    - Building JavaScript...");
        await _nodeTools.BuildJsAsync();

        Console.WriteLine("    ✓ Asset build complete");
    }
}