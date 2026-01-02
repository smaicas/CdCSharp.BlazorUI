using CdCSharp.BlazorUI.Abstractions.Behaviors.Javascript;
using CdCSharp.BlazorUI.Abstractions.Components.Variants;
using CdCSharp.BlazorUI.Components.Layout.ThemeSelector;
using CdCSharp.BlazorUI.Localization.Abstractions;
using CdCSharp.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorUI(this IServiceCollection services)
    {
        services.AddMemoryCache();

        // Un solo registry para todo
        services.AddSingleton<IVariantRegistry, VariantRegistry>();

        // JS interop
        services.AddScoped<IThemeJsInterop, ThemeJsInterop>();
        services.AddScoped<IBehaviorJsInterop, BehaviorJsInterop>();

        services.TryAddSingleton<IBlazorRuntime, MissingBlazorRuntime>();

        return services;
    }

    public static IServiceCollection AddBlazorUIVariants(
        this IServiceCollection services,
        Action<VariantBuilder> configure)
    {
        ServiceProvider sp = services.BuildServiceProvider();
        VariantRegistry registry = sp.GetService<IVariantRegistry>() as VariantRegistry
            ?? throw new InvalidOperationException("Must call AddBlazorUI before AddBlazorUIVariants");

        VariantBuilder builder = new(registry);
        configure(builder);

        return services;
    }
}

#region Variant Registry Builders

/// <summary>
/// Builder for registering custom component variants
/// </summary>
public sealed class VariantBuilder
{
    private readonly VariantRegistry _registry;

    internal VariantBuilder(VariantRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Start configuration for a specific component type
    /// </summary>
    /// <typeparam name="TComponent">The component type to configure variants for</typeparam>
    /// <returns>A builder for the component variants</returns>
    /// <example>
    /// <code>
    /// builder.ForComponent&lt;UIButton&gt;()
    ///     .AddVariant(MyCustomVariants.Gradient, button => ...)
    ///     .AddVariant(MyCustomVariants.Ghost, button => ...);
    /// </code>
    /// </example>
    public ComponentVariantBuilder<TComponent> ForComponent<TComponent>()
        where TComponent : ComponentBase
    {
        return new ComponentVariantBuilder<TComponent>(_registry);
    }
}

/// <summary>
/// Builder for registering variants for a specific component type
/// </summary>
public sealed class ComponentVariantBuilder<TComponent>
    where TComponent : ComponentBase
{
    private readonly VariantRegistry _registry;

    internal ComponentVariantBuilder(VariantRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Registers a custom variant with its render template
    /// </summary>
    /// <typeparam name="TVariant">The variant type</typeparam>
    /// <param name="variant">The variant instance</param>
    /// <param name="template">The render function for this variant</param>
    /// <returns>This builder for chaining</returns>
    public ComponentVariantBuilder<TComponent> AddVariant<TVariant>(
        TVariant variant,
        Func<TComponent, RenderFragment> template)
        where TVariant : Variant
    {
        _registry.Register(variant, template);
        return this;
    }
}

#endregion