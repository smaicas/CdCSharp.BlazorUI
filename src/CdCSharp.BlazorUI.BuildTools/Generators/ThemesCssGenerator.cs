using CdCSharp.BlazorUI.BuildTools.Pipeline;
using CdCSharp.BlazorUI.Core.Theming.Abstractions;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

public class ThemesCssGenerator : IAssetGenerator
{
    private readonly BuildContext _context;

    public string Name => "Themes CSS";

    public ThemesCssGenerator(BuildContext context)
    {
        _context = context;
    }

    public async Task GenerateAsync()
    {
        string css = CssThemeGenerator.Generate("dark",
            [new Core.Themes.DarkTheme(), new Core.Themes.LightTheme()]);

        string outputPath = _context.GetFullPath("CssBundle/themes.css");
        await File.WriteAllTextAsync(outputPath, css);
    }
}

public static class CssThemeGenerator
{
    public static string Generate(
        string defaultTheme,
        IReadOnlyCollection<UIThemePaletteBase> palettes)
    {
        StringBuilder sb = new();

        sb.AppendLine(":root {");
        sb.AppendLine("  /* === Theme palettes === */");

        // 1. Generate theme-specific variables for all themes
        foreach (UIThemePaletteBase palette in palettes)
        {
            Dictionary<string, string> themeVariables = palette.GetThemeVariables();
            foreach (KeyValuePair<string, string> variable in themeVariables)
            {
                sb.AppendLine($"  {variable.Key}: {variable.Value};");
            }
        }

        sb.AppendLine("  /* === Default palette mapping === */");

        // 2. Map --palette-* to the default theme
        UIThemePaletteBase defaultPalette = palettes.First(p => p.Id == defaultTheme);
        Dictionary<string, string> defaultMapping = defaultPalette.GetPaletteMapping();
        foreach (KeyValuePair<string, string> variable in defaultMapping)
        {
            sb.AppendLine($"  {variable.Key}: {variable.Value};");
        }

        sb.AppendLine("}");

        // 3. Generate theme selectors
        foreach (UIThemePaletteBase palette in palettes)
        {
            sb.AppendLine($"html[data-theme=\"{palette.Id}\"] {{");

            Dictionary<string, string> mapping = palette.GetPaletteMapping();
            foreach (KeyValuePair<string, string> variable in mapping)
            {
                sb.AppendLine($"  {variable.Key}: {variable.Value};");
            }

            sb.AppendLine("}");
        }

        // Remove the last empty line
        return sb.ToString().TrimEnd();
    }
}