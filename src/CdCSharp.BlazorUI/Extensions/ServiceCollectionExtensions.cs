using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Core.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        services.AddMemoryCache();

        // Variant registries (one per component)
        RegisterVariantRegistry<UIButton, UIButtonVariant>(services);
        RegisterVariantRegistry<UISvgIcon, UISvgIconVariant>(services);
        RegisterVariantRegistry<UIThemeSwitch, UIThemeSwitchVariant>(services);

        // JS interop
        services.AddScoped<IThemeJsInterop, ThemeJsInterop>();

        return services;
    }

    public static IServiceCollection AddBlazorUIVariants(
        this IServiceCollection services,
        Action<VariantRegistryBuilder> configure)
    {
        VariantRegistryBuilder builder = new(services);
        configure(builder);
        return services;
    }

    private static void RegisterVariantRegistry<TComponent, TVariant>(IServiceCollection services)
        where TComponent : UIVariantComponentBase<TComponent, TVariant>
        where TVariant : Variant
    {
        services.TryAddSingleton<VariantRegistry<TComponent, TVariant>>();
        services.TryAddSingleton<IVariantRegistry<TComponent, TVariant>>(sp =>
            sp.GetRequiredService<VariantRegistry<TComponent, TVariant>>());
    }
}

#region Variant Registry Builders

public sealed class VariantRegistryBuilder
{
    private readonly IServiceCollection _services;

    public VariantRegistryBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public VariantRegistryBuilder<TComponent, TVariant> For<TComponent, TVariant>()
        where TComponent : UIVariantComponentBase<TComponent, TVariant>
        where TVariant : Variant
    {
        VariantRegistry<TComponent, TVariant> registry = GetOrCreateRegistry<TComponent, TVariant>();
        return new VariantRegistryBuilder<TComponent, TVariant>(_services, registry);
    }

    private VariantRegistry<TComponent, TVariant> GetOrCreateRegistry<TComponent, TVariant>()
        where TComponent : UIVariantComponentBase<TComponent, TVariant>
        where TVariant : Variant
    {
        ServiceDescriptor? descriptor = _services.FirstOrDefault(d =>
            d.ServiceType == typeof(VariantRegistry<TComponent, TVariant>));

        if (descriptor?.ImplementationInstance is VariantRegistry<TComponent, TVariant> existing)
        {
            return existing;
        }

        VariantRegistry<TComponent, TVariant> registry = new();

        _services.AddSingleton(registry);
        _services.AddSingleton<IVariantRegistry<TComponent, TVariant>>(registry);

        return registry;
    }
}

public sealed class VariantRegistryBuilder<TComponent, TVariant>
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    private readonly VariantRegistry<TComponent, TVariant> _registry;
    private readonly IServiceCollection _services;
    public VariantRegistryBuilder(
        IServiceCollection services,
        VariantRegistry<TComponent, TVariant> registry)
    {
        _services = services;
        _registry = registry;
    }

    public VariantRegistryBuilder<TComponent, TVariant> Register(
        TVariant variant,
        Func<TComponent, RenderFragment> template)
    {
        _registry.Register(variant, template);
        return this;
    }
}

#endregion