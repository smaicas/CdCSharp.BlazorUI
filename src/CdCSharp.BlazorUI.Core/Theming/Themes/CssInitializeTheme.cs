namespace CdCSharp.BlazorUI.Core.Theming.Themes;

public static class CssInitializeTheme
{
    public static string GetCss() => """
        body {
          background-color: var(--palette-background);
          color: var(--palette-foreground);
        }
        """;
}
