using CdCSharp.BlazorUI.Components.Abstractions;
using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Services;
using Microsoft.AspNetCore.Components;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        services.AddMemoryCache();

        // Un solo registry para todo
        services.AddSingleton<IUniversalVariantRegistry, UniversalVariantRegistry>();

        // JS interop
        services.AddScoped<IThemeJsInterop, ThemeJsInterop>();
        services.AddScoped<IBehaviorJsInterop, BehaviorJsInterop>();

        return services;
    }

    public static IServiceCollection AddBlazorUIVariants(
        this IServiceCollection services,
        Action<UniversalVariantBuilder> configure)
    {
        ServiceProvider sp = services.BuildServiceProvider();
        UniversalVariantRegistry registry = sp.GetService<IUniversalVariantRegistry>() as UniversalVariantRegistry
            ?? throw new InvalidOperationException("Must call AddBlazorUI before AddBlazorUIVariants");

        UniversalVariantBuilder builder = new(registry);
        configure(builder);
        return services;
    }
}

#region Variant Registry Builders

public sealed class UniversalVariantBuilder
{
    private readonly UniversalVariantRegistry _registry;

    public UniversalVariantBuilder(UniversalVariantRegistry registry)
    {
        _registry = registry;
    }

    public UniversalVariantBuilder Register<TComponent, TVariant>(
        TVariant variant,
        Func<TComponent, RenderFragment> template)
        where TComponent : ComponentBase
        where TVariant : Variant
    {
        _registry.Register(variant, template);
        return this;
    }
}

#endregion