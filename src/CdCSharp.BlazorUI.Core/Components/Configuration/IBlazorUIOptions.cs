using CdCSharp.BlazorUI.Core.Components.Abstractions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Configuration;

public interface IBlazorUIOptions
{
    IComponentVariantBuilder<TComponent, TVariant> Configure<TComponent, TVariant>()
        where TComponent : UIVariantComponentBase<TComponent, TVariant>
        where TVariant : Variant;
}

public interface IComponentVariantBuilder<TComponent, TVariant>
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    IComponentVariantBuilder<TComponent, TVariant> AddVariant(
        TVariant variant,
        Func<TComponent, RenderFragment> template);

    IComponentVariantBuilder<TComponent, TVariant> AddVariant(
        string variantName,
        Func<TComponent, RenderFragment> template);
}
