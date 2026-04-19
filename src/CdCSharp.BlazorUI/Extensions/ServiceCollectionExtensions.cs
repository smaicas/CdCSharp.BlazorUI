using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Core.Abstractions.Services;
using CdCSharp.BlazorUI.Core.Diagnostics;
using CdCSharp.BlazorUI.Services;
using Microsoft.AspNetCore.Components;

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
        services.AddScoped<IPatternJsInterop, PatternJsInterop>();
        services.AddScoped<IDropdownJsInterop, DropdownJsInterop>();
        services.AddScoped<IClipboardJsInterop, ClipboardJsInterop>();
        services.AddScoped<ITextAreaJsInterop, TextAreaJsInterop>();
        services.AddScoped<IDraggableJsInterop, DraggableJsInterop>();
        services.AddScoped<IColorPickerJsInterop, ColorPickerJsInterop>();

        // Modal
        services.AddScoped<IModalService, ModalService>();
        services.AddScoped<IModalJsInterop, ModalJsInterop>();

        // Toast
        services.AddScoped<IToastService, ToastService>();

#if DEBUG
        services.AddSingleton<IBUIPerformanceService, BUIPerformanceService>();
#endif

        return services;
    }

    public static IServiceCollection AddBlazorUIVariants(
    this IServiceCollection services,
    Action<VariantBuilder> configure)
    {
        services.AddSingleton<IVariantRegistryInitializer>(
            new VariantRegistryInitializer(configure));

        return services;
    }
}

#region Variant Registry Builders

/// <summary>
/// Builder for registering variants for a specific component type
/// </summary>
public sealed class ComponentVariantBuilder<TComponent>
    where TComponent : ComponentBase
{
    private readonly IVariantRegistry _registry;

    internal ComponentVariantBuilder(IVariantRegistry registry) => _registry = registry;

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

/// <summary>
/// Builder for registering custom component variants
/// </summary>
public sealed class VariantBuilder
{
    private readonly IVariantRegistry _registry;

    internal VariantBuilder(IVariantRegistry registry) => _registry = registry;

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
        where TComponent : ComponentBase => new(_registry);
}
#endregion