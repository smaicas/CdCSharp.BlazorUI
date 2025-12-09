using CdCSharp.BlazorUI.Core.Theming.Abstractions;
using CdCSharp.BlazorUI.Core.Theming.Themes;
using System.Reflection;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools;

public class CssGenerator
{
    public static string GenerateThemesCss()
    {
        StringBuilder sb = new();

        // CSS Reset
        sb.AppendLine(CssReset.GetResetCss());
        sb.AppendLine();

        // Search for types that implement UITheme
        List<UITheme> themes = DiscoverThemes();

        // Generate CSS variables for each theme
        foreach (UITheme theme in themes)
        {
            GenerateThemeVariables(sb, theme);
            sb.AppendLine();
        }

        // Generate utility classes for themes
        GenerateThemeUtilities(sb, themes);

        return sb.ToString();
    }

    private static List<UITheme> DiscoverThemes()
    {
        List<UITheme> themes = [];

        // Obtain the assembly where UITheme is defined
        Assembly coreAssembly = typeof(UITheme).Assembly;

        // Search for all non-abstract classes that inherit from UITheme
        IEnumerable<Type> themeTypes = coreAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(UITheme))
                && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (Type themeType in themeTypes)
        {
            if (Activator.CreateInstance(themeType) is UITheme instance)
            {
                themes.Add(instance);
            }
        }

        return themes.OrderBy(t => t.Id).ToList();
    }

    private static void GenerateThemeVariables(StringBuilder sb, UITheme theme)
    {
        sb.AppendLine($"/* Theme: {theme.Name} */");

        // Generate CSS variables for the theme
        if (theme.Id == "light")
        {
            // The light theme is the default and uses :root
            sb.AppendLine(":root {");
        }
        else
        {
            // Other themes use data attributes
            sb.AppendLine($"[data-theme=\"{theme.Id}\"] {{");
        }

        Dictionary<string, string> variables = theme.GetCssVariables();
        foreach (KeyValuePair<string, string> variable in variables)
        {
            sb.AppendLine($"    {variable.Key}: {variable.Value};");
        }

        sb.AppendLine("}");
    }

    private static void GenerateThemeUtilities(StringBuilder sb, List<UITheme> themes)
    {
        sb.AppendLine("/* Theme Utilities */");
        sb.AppendLine();

        // Generate transition for theme changes
        sb.AppendLine("* {");
        sb.AppendLine("    transition: background-color 0.3s ease, color 0.3s ease, border-color 0.3s ease;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Classes to force a specific theme inside an element
        foreach (UITheme theme in themes)
        {
            sb.AppendLine($".theme-{theme.Id} {{");

            Dictionary<string, string> variables = theme.GetCssVariables();
            foreach (KeyValuePair<string, string> variable in variables)
            {
                // Convert specific variable to generic by removing theme prefix
                string genericVarName = variable.Key.Replace($"--{theme.Id}-", "--");
                sb.AppendLine($"    {genericVarName}: {variable.Value};");
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }
    }
}
