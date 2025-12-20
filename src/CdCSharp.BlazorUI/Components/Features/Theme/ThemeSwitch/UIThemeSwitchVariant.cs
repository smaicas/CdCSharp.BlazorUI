using CdCSharp.BlazorUI.Core.Components.Abstractions;

namespace CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;

public class UIThemeSwitchVariant : Variant
{
    public UIThemeSwitchVariant(string name) : base(name)
    {
    }

    public static readonly UIThemeSwitchVariant Default = new("Default");
    public static readonly UIThemeSwitchVariant SunMoon = new("SunMoon");

    public static UIThemeSwitchVariant Custom(string name) => new(name);
}