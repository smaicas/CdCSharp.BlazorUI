using CdCSharp.BlazorUI.Core.Theming.Css;

namespace CdCSharp.BlazorUI.Core.Theming.Abstractions;

public abstract class UIThemePaletteBase
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
}
