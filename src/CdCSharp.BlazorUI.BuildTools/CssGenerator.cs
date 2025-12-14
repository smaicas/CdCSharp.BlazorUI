using CdCSharp.BlazorUI.Core.Theming.Abstractions;
using CdCSharp.BlazorUI.Core.Theming.Css;
using System.Reflection;
using System.Text;

public static class CssGenerator
{
    private static readonly PropertyInfo[] PaletteProperties =
        typeof(UIThemePaletteBase)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .ToArray();

    public static string Generate(
        string defaultTheme,
        IReadOnlyCollection<UIThemePaletteBase> palettes)
    {
        StringBuilder sb = new();

        sb.AppendLine(":root {");
        sb.AppendLine("  /* === Theme palettes === */");

        // 1️⃣ Variables por paleta
        foreach (UIThemePaletteBase palette in palettes)
        {
            string themeId = palette.Id;

            foreach (PropertyInfo prop in PaletteProperties)
            {
                string cssName = CssNameHelper.ToCssVariable(prop.Name);
                CssColor color = (CssColor)prop.GetValue(palette)!;

                sb.AppendLine($"  --{themeId}-{cssName}: {color};");
            }
        }

        sb.AppendLine();
        sb.AppendLine("  /* === Default palette mapping === */");

        // 2️⃣ Mapping por defecto
        UIThemePaletteBase defaultPalette = palettes.First(p => p.Id == defaultTheme);

        foreach (PropertyInfo prop in PaletteProperties)
        {
            string cssName = CssNameHelper.ToCssVariable(prop.Name);
            sb.AppendLine($"  --palette-{cssName}: var(--{defaultTheme}-{cssName});");
        }

        sb.AppendLine("}");
        sb.AppendLine();

        // 3️⃣ Mapping por selector
        foreach (UIThemePaletteBase palette in palettes)
        {
            sb.AppendLine($"html[data-theme=\"{palette.Id}\"] {{");

            foreach (PropertyInfo prop in PaletteProperties)
            {
                string cssName = CssNameHelper.ToCssVariable(prop.Name);
                sb.AppendLine($"  --palette-{cssName}: var(--{palette.Id}-{cssName});");
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
internal static class CssNameHelper
{
    public static string ToCssVariable(string name)
        => name.ToLowerInvariant();
}

