using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Core.Components.Attributes;

namespace CdCSharp.BlazorUI.Components.Attributes;

public class ButtonVariantAttribute : VariantAttribute<UIButton, UIButtonVariant>
{
    public ButtonVariantAttribute(string variantName) : base(variantName) { }
}

public class IconVariantAttribute : VariantAttribute<UISvgIcon, UISvgIconVariant>
{
    public IconVariantAttribute(string variantName) : base(variantName) { }
}

public class ThemeSwitchVariantAttribute : VariantAttribute<UIThemeSwitch, UIThemeSwitchVariant>
{
    public ThemeSwitchVariantAttribute(string variantName) : base(variantName) { }
}
