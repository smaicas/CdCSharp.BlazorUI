namespace CdCSharp.BlazorUI.BuildTools;

public static class ConfigInitializer
{
    public static async Task InitializeProjectConfigs(string projectPath)
    {
        Console.WriteLine("Initializing project configuration files...");

        // Create package.json if it doesn't exist
        string packageJsonPath = Path.Combine(projectPath, "package.json");
        if (!File.Exists(packageJsonPath))
        {
            Console.WriteLine("Creating package.json...");
            await File.WriteAllTextAsync(packageJsonPath, ConfigTemplates.GetPackageJson());
        }

        // Create .npmrc to reduce npm noise
        string npmrcPath = Path.Combine(projectPath, ".npmrc");
        if (!File.Exists(npmrcPath))
        {
            Console.WriteLine("Creating .npmrc...");
            await File.WriteAllTextAsync(npmrcPath, "fund=false\naudit=false\n");
        }

        // Create tsconfig.json if it doesn't exist
        string tsConfigPath = Path.Combine(projectPath, "tsconfig.json");
        if (!File.Exists(tsConfigPath))
        {
            Console.WriteLine("Creating tsconfig.json...");
            await File.WriteAllTextAsync(tsConfigPath, ConfigTemplates.GetTsConfig());
        }

        // Create vite.config.js if it doesn't exist
        string viteConfigPath = Path.Combine(projectPath, "vite.config.js");
        if (!File.Exists(viteConfigPath))
        {
            Console.WriteLine("Creating vite.config.js...");
            await File.WriteAllTextAsync(viteConfigPath, ConfigTemplates.GetViteConfigJs());
        }

        // Create vite.config.css.js if it doesn't exist
        string viteCssConfigPath = Path.Combine(projectPath, "vite.config.css.js");
        if (!File.Exists(viteCssConfigPath))
        {
            Console.WriteLine("Creating vite.config.css.js...");
            await File.WriteAllTextAsync(viteCssConfigPath, ConfigTemplates.GetViteConfigCss());
        }

        // Create CssBundle directory and main.css if they don't exist
        string cssBundlePath = Path.Combine(projectPath, "CssBundle");
        if (!Directory.Exists(cssBundlePath))
        {
            Directory.CreateDirectory(cssBundlePath);
        }

        string mainCssPath = Path.Combine(cssBundlePath, "main.css");
        if (!File.Exists(mainCssPath))
        {
            Console.WriteLine("Creating CssBundle/main.css...");
            await File.WriteAllTextAsync(mainCssPath, ConfigTemplates.GetMainCss());
        }

        // Create Types directory if it doesn't exist
        string typesPath = Path.Combine(projectPath, "Types");
        if (!Directory.Exists(typesPath))
        {
            Directory.CreateDirectory(typesPath);
            Console.WriteLine("Created Types directory for TypeScript files");
        }

        // Create wwwroot directories if they don't exist
        string wwwrootPath = Path.Combine(projectPath, "wwwroot");
        string wwwrootCssPath = Path.Combine(wwwrootPath, "css");
        string wwwrootJsPath = Path.Combine(wwwrootPath, "js");

        if (!Directory.Exists(wwwrootPath))
        {
            Directory.CreateDirectory(wwwrootPath);
        }

        if (!Directory.Exists(wwwrootCssPath))
        {
            Directory.CreateDirectory(wwwrootCssPath);
        }

        if (!Directory.Exists(wwwrootJsPath))
        {
            Directory.CreateDirectory(wwwrootJsPath);
        }

        Console.WriteLine("Configuration files initialized successfully!");
    }
}