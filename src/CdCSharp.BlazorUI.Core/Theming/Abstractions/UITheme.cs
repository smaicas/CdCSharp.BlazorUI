using CdCSharp.BlazorUI.Core.Theming.Css;

namespace CdCSharp.BlazorUI.Core.Theming.Abstractions;

public abstract class UITheme
{
    public string Id { get; set; } = "default";
    public string Name { get; set; } = "Default";

    // Main colors
    public CssColor Primary { get; set; } = new("#3B82F6");
    public CssColor PrimaryContrast { get; set; } = new("#FFFFFF");

    public CssColor Secondary { get; set; } = new("#8B5CF6");
    public CssColor SecondaryContrast { get; set; } = new("#FFFFFF");

    // Surface colors
    public CssColor Background { get; set; } = new("#FFFFFF");
    public CssColor Surface { get; set; } = new("#F8FAFC");
    public CssColor Foreground { get; set; } = new("#1E293B");

    // Status colors
    public CssColor Error { get; set; } = new("#EF4444");
    public CssColor Success { get; set; } = new("#10B981");
    public CssColor Warning { get; set; } = new("#F59E0B");
    public CssColor Info { get; set; } = new("#3B82F6");

    // Typography
    public string BaseFontSize { get; set; } = "16px";
    public string BaseFontFamily { get; set; } = "'Inter', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
    public string HeadingFontFamily { get; set; } = "'Inter', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";

    // Spacing (8px system)
    public string SpacingXs { get; set; } = "0.25rem";  // 4px
    public string SpacingSm { get; set; } = "0.5rem";   // 8px
    public string SpacingMd { get; set; } = "1rem";     // 16px
    public string SpacingLg { get; set; } = "1.5rem";   // 24px
    public string SpacingXl { get; set; } = "2rem";     // 32px

    // Borders
    public string BorderRadius { get; set; } = "0.375rem"; // 6px
    public CssColor BorderColor { get; set; } = new("#E2E8F0");

    public Dictionary<string, string> GetCssVariables()
    {
        Dictionary<string, string> variables = new()
        {
            // Colors
            [$"--{Id}-primary"] = Primary.ToString(),
            [$"--{Id}-primary-contrast"] = PrimaryContrast.ToString(),
            [$"--{Id}-secondary"] = Secondary.ToString(),
            [$"--{Id}-secondary-contrast"] = SecondaryContrast.ToString(),
            [$"--{Id}-background"] = Background.ToString(),
            [$"--{Id}-surface"] = Surface.ToString(),
            [$"--{Id}-foreground"] = Foreground.ToString(),
            [$"--{Id}-error"] = Error.ToString(),
            [$"--{Id}-success"] = Success.ToString(),
            [$"--{Id}-warning"] = Warning.ToString(),
            [$"--{Id}-info"] = Info.ToString(),
            [$"--{Id}-border-color"] = BorderColor.ToString(),

            // Typography
            [$"--{Id}-font-size"] = BaseFontSize,
            [$"--{Id}-font-family"] = BaseFontFamily,
            [$"--{Id}-heading-font-family"] = HeadingFontFamily,

            // Spacing
            [$"--{Id}-spacing-xs"] = SpacingXs,
            [$"--{Id}-spacing-sm"] = SpacingSm,
            [$"--{Id}-spacing-md"] = SpacingMd,
            [$"--{Id}-spacing-lg"] = SpacingLg,
            [$"--{Id}-spacing-xl"] = SpacingXl,

            // Borders
            [$"--{Id}-border-radius"] = BorderRadius,
        };

        return variables;
    }
}
