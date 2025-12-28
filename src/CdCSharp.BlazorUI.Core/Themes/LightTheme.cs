using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Core.Theming.Abstractions;

namespace CdCSharp.BlazorUI.Core.Themes;

public class LightTheme : UIThemePaletteBase
{
    public LightTheme()
    {
        Id = "light";
        Name = "Light";

        Background = new CssColor("#FFFFFF");
        BackgroundContrast = new CssColor("#1E293B");

        Surface = new CssColor("#F8FAFC");
        SurfaceContrast = new CssColor("#1E293B");

        Error = new CssColor("#EF4444");
        ErrorContrast = new CssColor("#FFFFFF");

        Success = new CssColor("#10B981");
        SuccessContrast = new CssColor("#FFFFFF");

        Warning = new CssColor("#F59E0B");
        WarningContrast = new CssColor("#FFFFFF");

        Info = new CssColor("#3B82F6");
        InfoContrast = new CssColor("#FFFFFF");

        Primary = new CssColor("#3B82F6");
        PrimaryContrast = new CssColor("#FFFFFF");

        Secondary = new CssColor("#8B5CF6");
        SecondaryContrast = new CssColor("#FFFFFF");
    }
}