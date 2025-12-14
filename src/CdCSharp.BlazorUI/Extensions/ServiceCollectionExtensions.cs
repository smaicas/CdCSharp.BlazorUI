using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Core.Components.Configuration;
using CdCSharp.BlazorUI.Core.Components.Discovery;
using CdCSharp.BlazorUI.Core.Components.Services;
using CdCSharp.BlazorUI.Core.Theming.Interop;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        return services.AddBlazorUI(null);
    }

    public static IServiceCollection AddBlazorUI(
        this IServiceCollection services,
        Action<IBlazorUIOptions>? configureOptions)
    {
        services.AddMemoryCache();

        // Variant Registries
        services.AddSingleton<IVariantRegistry<UIButton, UIButtonVariant>>(sp => new VariantRegistry<UIButton, UIButtonVariant>());
        services.AddSingleton<IVariantRegistry<UISvgIcon, UISvgIconVariant>>(sp => new VariantRegistry<UISvgIcon, UISvgIconVariant>());
        services.AddSingleton<IVariantRegistry<UIThemeSwitch, UIThemeSwitchVariant>>(sp => new VariantRegistry<UIThemeSwitch, UIThemeSwitchVariant>());

        // JSInterop Registries
        services.AddScoped<IThemeJsInterop, ThemeJsInterop>();

        // Configure options if provided
        if (configureOptions != null)
        {
            BlazorUIOptions options = new();
            configureOptions(options);

            // Register a startup filter to apply configurations
            services.AddSingleton<IStartupFilter>(new BlazorUIStartupFilter(options));
        }

        return services;
    }

    public static IServiceCollection AddVariantsFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddSingleton<IStartupFilter>(new AssemblyVariantStartupFilter(assembly));
        return services;
    }

    public static IServiceCollection AddVariantsFromType<T>(this IServiceCollection services)
    {
        return services.AddVariantsFromAssembly(typeof(T).Assembly);
    }
}

internal class BlazorUIStartupFilter : IStartupFilter
{
    private readonly BlazorUIOptions _options;

    public BlazorUIStartupFilter(BlazorUIOptions options)
    {
        _options = options;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            _options.ApplyTo(builder.ApplicationServices);
            next(builder);
        };
    }
}

internal class AssemblyVariantStartupFilter : IStartupFilter
{
    private readonly Assembly _assembly;

    public AssemblyVariantStartupFilter(Assembly assembly)
    {
        _assembly = assembly;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            VariantDiscovery.DiscoverAndRegisterVariants(builder.ApplicationServices, _assembly);
            next(builder);
        };
    }
}