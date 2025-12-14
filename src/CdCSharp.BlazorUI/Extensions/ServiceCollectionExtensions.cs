using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Core.Components.Services;
using CdCSharp.BlazorUI.Core.Theming.Interop;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        services.AddMemoryCache();

        // Variant Registries
        services.AddSingleton<IVariantRegistry<UIButton, UIButtonVariant>>(sp => new VariantRegistry<UIButton, UIButtonVariant>());
        services.AddSingleton<IVariantRegistry<UISvgIcon, UISvgIconVariant>>(sp => new VariantRegistry<UISvgIcon, UISvgIconVariant>());
        services.AddSingleton<IVariantRegistry<UIThemeSwitch, UIThemeSwitchVariant>>(sp => new VariantRegistry<UIThemeSwitch, UIThemeSwitchVariant>());

        // JSInterop Registries
        services.AddScoped<IThemeJsInterop, ThemeJsInterop>();

        return services;
    }
}
