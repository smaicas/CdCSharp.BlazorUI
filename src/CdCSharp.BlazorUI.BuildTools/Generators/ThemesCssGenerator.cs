using CdCSharp.BlazorUI.Themes;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ThemesCssGenerator : IAssetGenerator
{
    public string FileName => "_themes.css";
    public string Name => "Themes CSS";

    public Task<string> GetContent() => Task.FromResult(CssThemeGenerator.Generate("dark",
            [new Themes.DarkTheme(), new Themes.LightTheme()]));
}

public static class CssThemeGenerator
{
    public static string Generate(
        string defaultTheme,
        IReadOnlyCollection<BUIThemePaletteBase> palettes)
    {
        StringBuilder sb = new();

        // 1. Definir el tema por defecto en :root con valores finales
        sb.AppendLine(":root {");
        sb.AppendLine("  /* === Base Palette (Default Theme) === */");

        BUIThemePaletteBase defaultPalette = palettes.First(p => p.Id == defaultTheme);
        // Suponiendo que GetThemeVariables devuelve el Diccionario con nombres limpios y valores RGBA
        foreach (KeyValuePair<string, string> variable in defaultPalette.GetThemeVariables())
        {
            // Forzamos que la variable se llame --palette-Nombre
            // Ajusta el replace según cómo devuelva tu clase los nombres
            string key = variable.Key.Replace("--dark-", "--palette-").Replace("--light-", "--palette-");
            sb.AppendLine($"  {key}: {variable.Value};");
        }
        sb.AppendLine("}");

        // 2. Generar sobreescrituras para los demás temas
        foreach (BUIThemePaletteBase palette in palettes)
        {
            if (palette.Id == defaultTheme) continue; // No repetimos el default

            sb.AppendLine($"\nhtml[data-theme=\"{palette.Id}\"] {{");
            foreach (KeyValuePair<string, string> variable in palette.GetThemeVariables())
            {
                string key = variable.Key.Replace("--dark-", "--palette-").Replace("--light-", "--palette-");
                sb.AppendLine($"  {key}: {variable.Value};");
            }
            sb.AppendLine("}");
        }

        return sb.ToString().TrimEnd();
    }
}