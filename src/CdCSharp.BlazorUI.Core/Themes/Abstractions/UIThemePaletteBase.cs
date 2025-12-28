using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Core.Theming.Abstractions;

public abstract class UIThemePaletteBase
{
    public string Id { get; set; } = "default";
    public string Name { get; set; } = "Default";

    // Surface colors
    public CssColor Background { get; set; } = new("#0F172A");
    public CssColor BackgroundContrast { get; set; } = new("#F1F5F9");

    public CssColor Surface { get; set; } = new("#1E293B");
    public CssColor SurfaceContrast { get; set; } = new("#F1F5F9");

    // Status colors
    public CssColor Error { get; set; } = new("#EF4444");
    public CssColor ErrorContrast { get; set; } = new("#0F172A");

    public CssColor Success { get; set; } = new("#10B981");
    public CssColor SuccessContrast { get; set; } = new("#0F172A");

    public CssColor Warning { get; set; } = new("#F59E0B");
    public CssColor WarningContrast { get; set; } = new("#0F172A");

    public CssColor Info { get; set; } = new("#3B82F6");
    public CssColor InfoContrast { get; set; } = new("#0F172A");

    // Main colors
    public CssColor Primary { get; set; } = new("#60A5FA");
    public CssColor PrimaryContrast { get; set; } = new("#0F172A");

    public CssColor Secondary { get; set; } = new("#A78BFA");
    public CssColor SecondaryContrast { get; set; } = new("#0F172A");
}