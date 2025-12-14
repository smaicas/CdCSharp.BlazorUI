using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Core.Components.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class BlazorUIOptionsExtensions
{
    public static IComponentVariantBuilder<UIButton, UIButtonVariant> ConfigureButton(
        this IBlazorUIOptions options)
    {
        return options.Configure<UIButton, UIButtonVariant>();
    }

    public static IComponentVariantBuilder<UISvgIcon, UISvgIconVariant> ConfigureIcon(
        this IBlazorUIOptions options)
    {
        return options.Configure<UISvgIcon, UISvgIconVariant>();
    }

    public static IComponentVariantBuilder<UIThemeSwitch, UIThemeSwitchVariant> ConfigureThemeSwitch(
        this IBlazorUIOptions options)
    {
        return options.Configure<UIThemeSwitch, UIThemeSwitchVariant>();
    }
}