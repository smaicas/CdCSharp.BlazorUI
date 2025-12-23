using CdCSharp.BlazorUI.Core.Components.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Services;

public interface IVariantRegistry<TComponent, TVariant>
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    RenderFragment? GetTemplate(TVariant variant, TComponent component);

    void Register(TVariant variant, Func<TComponent, RenderFragment> template);
}

public sealed class VariantRegistry<TComponent, TVariant> : IVariantRegistry<TComponent, TVariant>
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    private readonly Dictionary<TVariant, Func<TComponent, RenderFragment>> _templates = [];

    public RenderFragment? GetTemplate(TVariant variant, TComponent component)
    {
        return _templates.TryGetValue(variant, out Func<TComponent, RenderFragment>? template)
            ? template(component)
            : null;
    }

    public void Register(TVariant variant, Func<TComponent, RenderFragment> template)
    {
        _templates[variant] = template;
    }
}