using CdCSharp.BuildTools.Discovery;

namespace CdCSharp.BuildTools;

public class BuildPipeline
{
    private readonly BuildContext _context;
    private readonly NodeToolsManager _node;

    public BuildPipeline(BuildContext context)
    {
        _context = context;
        _node = new NodeToolsManager(context);
    }

    public async Task ExecuteAsync()
    {
        await InitializeAsync();
        await GenerateAssetsAsync();
        await BuildAssetsAsync();
    }

    private async Task BuildAssetsAsync()
    {
        await _node.BuildCssAsync();
        await _node.BuildJsAsync();
    }

    private async Task GenerateAssetsAsync()
    {
        IReadOnlyList<IAssetGenerator> generators = AssetGeneratorDiscovery.Discover();

        foreach (IAssetGenerator gen in generators)
        {
            Console.WriteLine($"Generating {gen.Name}");
            await File.WriteAllTextAsync(_context.GetFullPath(Path.Combine(_context.CssBundlePath, gen.FileName)), await gen.GetContent());
        }
    }

    private async Task InitializeAsync()
    {
        await _node.VerifyNodeInstalledAsync();
        _context.EnsureDirectoriesFromConfig();

        TemplateManager templates = new(_context);
        await templates.EnsureTemplatesAsync();

        await _node.EnsurePackagesInstalledAsync();
    }
}