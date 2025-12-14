using CdCSharp.BlazorUI.Core.Theming.Abstractions;
using CdCSharp.BlazorUI.Core.Theming.Css;

namespace CdCSharp.BlazorUI.Core.Theming.Themes;

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
