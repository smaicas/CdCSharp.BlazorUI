using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Css;
using System.Reflection;

namespace CdCSharp.BlazorUI.Core.Theming.Abstractions;

public abstract class BUIThemePaletteBase
{
    // Surface colors
    public CssColor Background { get; set; } = new("#0F172A");

    public CssColor BackgroundContrast { get; set; } = new("#F1F5F9");

    // Base colors for contrast calculations
    public CssColor Black { get; set; } = new("#010101");

    public CssColor Border { get; set; } = BUIColor.White.Default;

    // Status colors
    public CssColor Error { get; set; } = new("#EF4444");

    public CssColor ErrorContrast { get; set; } = new("#0F172A");
    public CssColor Highlight { get; set; } = BUIColor.Yellow.Default;
    public string Id { get; set; } = "default";
    public CssColor Info { get; set; } = new("#3B82F6");
    public CssColor InfoContrast { get; set; } = new("#0F172A");
    public string Name { get; set; } = "Default";

    // Main colors
    public CssColor Primary { get; set; } = new("#60A5FA");

    public CssColor PrimaryContrast { get; set; } = new("#0F172A");
    public CssColor Secondary { get; set; } = new("#A78BFA");
    public CssColor SecondaryContrast { get; set; } = new("#0F172A");
    public CssColor Shadow { get; set; } = BUIColor.White.Default;
    public CssColor Success { get; set; } = new("#10B981");
    public CssColor SuccessContrast { get; set; } = new("#0F172A");
    public CssColor Surface { get; set; } = new("#1E293B");
    public CssColor SurfaceContrast { get; set; } = new("#F1F5F9");
    public CssColor Warning { get; set; } = new("#F59E0B");
    public CssColor WarningContrast { get; set; } = new("#0F172A");
    public CssColor White { get; set; } = new("#e9e9e9");

    /// <summary>
    /// Gets palette mapping variables (e.g., --palette-background: var(--dark-background))
    /// </summary>
    public Dictionary<string, string> GetPaletteMapping()
    {
        Dictionary<string, string> variables = [];
        PropertyInfo[] properties = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .ToArray();

        foreach (PropertyInfo property in properties)
        {
            string cssName = ToCssVariableName(property.Name);
            variables[$"--palette-{cssName}"] = $"var(--{Id}-{cssName})";
        }

        return variables;
    }

    /// <summary>
    /// Gets theme-specific CSS variables (e.g., --dark-background, --light-background)
    /// </summary>
    public Dictionary<string, string> GetThemeVariables()
    {
        Dictionary<string, string> variables = [];
        PropertyInfo[] properties = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .ToArray();

        foreach (PropertyInfo property in properties)
        {
            string cssName = ToCssVariableName(property.Name);
            CssColor color = (CssColor)property.GetValue(this)!;
            variables[$"--{Id}-{cssName}"] = color.ToString(ColorOutputFormats.Rgba);
        }

        return variables;
    }

    private static string ToCssVariableName(string propertyName)
    {
        return propertyName.ToLowerInvariant();
    }
}