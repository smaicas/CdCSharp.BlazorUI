using CdCSharp.BlazorUI.Core.Components.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Variants.Services;

public interface IVariantRegistry<TComponent, TVariant>
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    void Register(TVariant variant, Func<TComponent, RenderFragment> template);
    RenderFragment? GetTemplate(TVariant variant, TComponent component);
}

public sealed class VariantRegistry<TComponent, TVariant> : IVariantRegistry<TComponent, TVariant>
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    private readonly Dictionary<TVariant, Func<TComponent, RenderFragment>> _templates = [];

    public void Register(TVariant variant, Func<TComponent, RenderFragment> template)
    {
        _templates[variant] = template;
    }

    public RenderFragment? GetTemplate(TVariant variant, TComponent component)
    {
        return _templates.TryGetValue(variant, out Func<TComponent, RenderFragment>? template)
            ? template(component)
            : null;
    }
}
