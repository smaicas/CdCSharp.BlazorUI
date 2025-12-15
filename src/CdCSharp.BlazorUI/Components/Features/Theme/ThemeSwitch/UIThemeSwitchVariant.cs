using CdCSharp.BlazorUI.Core.Variants;

namespace CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;

public class UIThemeSwitchVariant : Variant
{
    public UIThemeSwitchVariant(string name) : base(name)
    {
    }
    public static readonly UIThemeSwitchVariant Default = new("Default");
    public static UIThemeSwitchVariant Custom(string name) => new(name);
}
