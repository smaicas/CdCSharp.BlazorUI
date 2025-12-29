using CdCSharp.BlazorUI.Services;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Abstractions;

internal sealed class VariantHelper<TComponent, TVariant>
    where TComponent : ComponentBase
    where TVariant : Variant
{
    private readonly TComponent _component;
    private readonly IUniversalVariantRegistry? _registry;

    public VariantHelper(TComponent component, IUniversalVariantRegistry? registry)
    {
        _component = component;
        _registry = registry;
    }

    public RenderFragment? ResolveTemplate(
        TVariant variant,
        IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>>? builtInTemplates)
    {
        if (builtInTemplates?.TryGetValue(variant, out Func<TComponent, RenderFragment>? builtIn) == true)
        {
            return builtIn(_component);
        }

        return _registry?.GetTemplate(_component.GetType(), variant, _component);
    }
}
