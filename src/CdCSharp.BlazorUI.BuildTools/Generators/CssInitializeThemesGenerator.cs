using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Themes;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class CssInitializeThemesGenerator : IAssetGenerator
{
    public string FileName => "_initialize-themes.css";
    public string Name => "Initialize Themes CSS";

    public Task<string> GetContent()
    {
        StringBuilder sb = new();
        sb.AppendLine("body {");
        sb.AppendLine($"  font-family: var({FeatureDefinitions.Typography.FontFamily});");
        sb.AppendLine($"  font-size: var({FeatureDefinitions.Typography.FontSizeBase});");
        sb.AppendLine($"  line-height: var({FeatureDefinitions.Typography.LineHeight});");
        sb.AppendLine("  background-color: var(--palette-background);");
        sb.AppendLine("  color: var(--palette-backgroundcontrast);");
        sb.AppendLine("}");
        sb.AppendLine();

        // Emit .bui-color-<key> and .bui-bg-<key> for every palette CssColor property.
        // Source of truth: the same reflection LightTheme/DarkTheme use for GetThemeVariables().
        string[] keys = typeof(BUIThemePaletteBase)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .Select(p => p.Name.ToLowerInvariant())
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToArray();

        foreach (string key in keys)
        {
            sb.AppendLine($".bui-color-{key} {{");
            sb.AppendLine($"  color: var(--palette-{key});");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine($".bui-bg-{key} {{");
            sb.AppendLine($"  background-color: var(--palette-{key});");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return Task.FromResult(sb.ToString().TrimEnd());
    }
}
