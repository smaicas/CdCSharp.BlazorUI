using CdCSharp.BlazorUI.Core.Theming.Abstractions;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ThemesCssGenerator : IAssetGenerator
{
    public string Name => "Themes CSS";

    public string FileName => "themes.css";

    public async Task<string> GetContent() => CssThemeGenerator.Generate("dark",
            [new Core.Themes.DarkTheme(), new Core.Themes.LightTheme()]);
}

public static class CssThemeGenerator
{
    public static string Generate(
        string defaultTheme,
        IReadOnlyCollection<BUIThemePaletteBase> palettes)
    {
        StringBuilder sb = new();

        sb.AppendLine(":root {");
        sb.AppendLine("  /* === Theme palettes === */");

        // 1. Generate theme-specific variables for all themes
        foreach (BUIThemePaletteBase palette in palettes)
        {
            Dictionary<string, string> themeVariables = palette.GetThemeVariables();
            foreach (KeyValuePair<string, string> variable in themeVariables)
            {
                sb.AppendLine($"  {variable.Key}: {variable.Value};");
            }
        }

        sb.AppendLine("  /* === Default palette mapping === */");

        // 2. Map --palette-* to the default theme
        BUIThemePaletteBase defaultPalette = palettes.First(p => p.Id == defaultTheme);
        Dictionary<string, string> defaultMapping = defaultPalette.GetPaletteMapping();
        foreach (KeyValuePair<string, string> variable in defaultMapping)
        {
            sb.AppendLine($"  {variable.Key}: {variable.Value};");
        }

        sb.AppendLine("}");

        // 3. Generate theme selectors
        foreach (BUIThemePaletteBase palette in palettes)
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