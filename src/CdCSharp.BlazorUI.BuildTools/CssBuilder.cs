namespace CdCSharp.BlazorUI.BuildTools;

public static class CssBuilder
{
    public static async Task Build(string projectPath)
    {
        await NpmManager.EnsureNpmInstalled(projectPath);
        Console.WriteLine("Building CSS with Vite...");
        await NpmManager.RunViteBuild(projectPath, "vite.config.css.js");

        Console.WriteLine("CSS build completed successfully!");
    }
}
