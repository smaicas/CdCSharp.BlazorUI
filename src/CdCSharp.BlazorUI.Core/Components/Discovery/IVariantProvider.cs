using CdCSharp.BlazorUI.Core.Components.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Discovery;

public interface IVariantProvider<TComponent, TVariant>
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    IEnumerable<(TVariant variant, Func<TComponent, RenderFragment> template)> GetVariants();
}
