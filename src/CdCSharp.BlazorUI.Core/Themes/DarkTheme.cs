using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Core.Theming.Abstractions;

namespace CdCSharp.BlazorUI.Core.Themes;

public class DarkTheme : UIThemePaletteBase
{
    public DarkTheme()
    {
        Id = "dark";
        Name = "Dark";

        Primary = new CssColor("#60A5FA");
        PrimaryContrast = new CssColor("#0F172A");

        Secondary = new CssColor("#A78BFA");
        SecondaryContrast = new CssColor("#0F172A");

        Background = new CssColor("#0F172A");
        Surface = new CssColor("#1E293B");
        Foreground = new CssColor("#F1F5F9");
    }
}