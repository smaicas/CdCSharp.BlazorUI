using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;
using CdCSharp.BlazorUI.Core.Abstractions.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Services;

public sealed class VariantRegistry : IVariantRegistry
{
    private readonly Dictionary<(Type, Type, string), Delegate> _templates = [];
    private readonly bool _initialized;

    public VariantRegistry(IEnumerable<IVariantRegistryInitializer> initializers)
    {
        foreach (IVariantRegistryInitializer initializer in initializers)
        {
            initializer.Initialize(this);
        }

        _initialized = true;
    }

    public void Register<TComponent, TVariant>(
        TVariant variant,
        Func<TComponent, RenderFragment> template)
        where TComponent : ComponentBase
        where TVariant : Variant
    {
        if (_initialized)
            throw new InvalidOperationException(
                "Variants must be registered during startup");

        (Type, Type, string Name) key = (typeof(TComponent), typeof(TVariant), variant.Name);
        _templates[key] = template;
    }

    public RenderFragment? GetTemplate(
        Type componentType,
        Variant variant,
        ComponentBase component)
    {
        Type? currentType = componentType;

        while (currentType != null && currentType != typeof(ComponentBase))
        {
            (Type currentType, Type, string Name) key = (currentType, variant.GetType(), variant.Name);

            if (_templates.TryGetValue(key, out Delegate? template))
            {
                return template.DynamicInvoke(component) as RenderFragment;
            }

            currentType = currentType.BaseType;
        }

        return null;
    }
}

internal sealed class VariantRegistryInitializer : IVariantRegistryInitializer
{
    private readonly Action<VariantBuilder> _configure;

    public VariantRegistryInitializer(Action<VariantBuilder> configure)
    {
        _configure = configure;
    }

    public void Initialize(IVariantRegistry registry)
    {
        VariantBuilder builder = new(registry);
        _configure(builder);
    }
}