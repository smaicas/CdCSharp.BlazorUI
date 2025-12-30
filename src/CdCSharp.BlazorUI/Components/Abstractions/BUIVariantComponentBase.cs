using CdCSharp.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Components.Abstractions;

public abstract class BUIVariantComponentBase<TComponent, TVariant> : BUIComponentBase, IVariantComponent<TVariant>
    where TComponent : BUIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    private RenderFragment? _resolvedTemplate;
    private VariantHelper<TComponent, TVariant>? _variantHelper;

    [Parameter] public TVariant? Variant { get; set; }

    protected abstract IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }
    public abstract TVariant DefaultVariant { get; }

    [Inject] private IUniversalVariantRegistry? VariantRegistry { get; set; }

    Variant IVariantComponent.CurrentVariant => CurrentVariant;
    Type IVariantComponent.VariantType => typeof(TVariant);
    public TVariant CurrentVariant => Variant ?? DefaultVariant;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        if (_resolvedTemplate is not null)
        {
            builder.AddContent(0, _resolvedTemplate);
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _variantHelper ??= new VariantHelper<TComponent, TVariant>(
            (TComponent)this,
            VariantRegistry);

        Variant ??= DefaultVariant;
        _resolvedTemplate = _variantHelper.ResolveTemplate(Variant, BuiltInTemplates);
    }
}