using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Abstractions;

public interface IVariantRegistry<TComponent, TVariant>
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    void Register(TVariant variant, Func<TComponent, RenderFragment> template);
    RenderFragment? GetTemplate(TVariant variant, TComponent component);
}
