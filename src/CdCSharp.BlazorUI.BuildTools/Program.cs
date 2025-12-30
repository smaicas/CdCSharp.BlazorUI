using CdCSharp.BlazorUI.BuildTools.Pipeline;

if (args.Length == 0 || args[0].ToLowerInvariant() != "build")
{
    Console.WriteLine("Usage: blazorui-buildtools build [project-path]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  build    Build all BlazorUI assets (CSS, JS, themes)");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  project-path    Path to the BlazorUI project (default: current directory)");
    Environment.Exit(1);
}

string projectPath = args.Length > 1 ? args[1] : ".";

try
{
    BuildPipeline pipeline = new(projectPath);
    await pipeline.ExecuteAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Build failed: {ex.Message}");

    if (ex.InnerException != null)
    {
        Console.Error.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }

#if DEBUG
    Console.Error.WriteLine(ex.StackTrace);
#endif

    Environment.Exit(1);
}