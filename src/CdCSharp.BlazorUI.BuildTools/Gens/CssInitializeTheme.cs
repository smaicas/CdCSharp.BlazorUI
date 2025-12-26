using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Gens;

[ExcludeFromCodeCoverage]
public static class CssInitializeTheme
{
    public static string GetCss() => """
        body {
          background-color: var(--palette-background);
          color: var(--palette-foreground);
        }
        """;
}