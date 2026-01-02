using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;
using CdCSharp.BlazorUI.Core.Abstractions.Services;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Services;

public sealed class VariantRegistry : IVariantRegistry
{
    private readonly Dictionary<(Type ComponentType, Type VariantType, string VariantName), Delegate> _templates = [];

    public void Register<TComponent, TVariant>(
        TVariant variant,
        Func<TComponent, RenderFragment> template)
        where TComponent : ComponentBase
        where TVariant : Variant
    {
        (Type, Type, string Name) key = (typeof(TComponent), typeof(TVariant), variant.Name);
        _templates[key] = template;
    }

    public RenderFragment? GetTemplate(Type componentType, Variant variant, ComponentBase component)
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