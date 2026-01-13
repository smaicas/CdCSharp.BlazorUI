namespace CdCSharp.BuildTools;

public static class BuildToolsManager
{
    public static async Task Build(string? projectPath = null)
    {
        projectPath ??= Directory.GetCurrentDirectory();

        BuildContext context = new(projectPath);

        BuildPipeline pipeline = new(context);
        await pipeline.ExecuteAsync();
    }
}