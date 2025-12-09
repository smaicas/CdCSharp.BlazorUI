using CdCSharp.BlazorUI.Core.Theming.Abstractions;

namespace CdCSharp.BlazorUI.Core.Theming.Services;

public interface IThemeService
{
    void RegisterTheme(UITheme theme);
    string GenerateThemeCss();
    Dictionary<string, string> GetCssVariables();
}
