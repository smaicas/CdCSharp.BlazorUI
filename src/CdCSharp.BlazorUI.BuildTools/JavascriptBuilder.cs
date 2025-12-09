namespace CdCSharp.BlazorUI.BuildTools;

public static class JavaScriptBuilder
{
    public static async Task Build(string projectPath)
    {
        await NpmManager.EnsureNpmInstalled(projectPath);
        Console.WriteLine("Building JavaScript/TypeScript with Vite...");
        await NpmManager.RunViteBuild(projectPath, "vite.config.js");
        Console.WriteLine("JavaScript build completed successfully!");
    }
}
