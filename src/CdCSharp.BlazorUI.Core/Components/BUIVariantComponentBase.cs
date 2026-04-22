using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Abstractions;

public abstract class BUIVariantComponentBase<TComponent, TVariant> : BUIComponentBase, IVariantComponent<TVariant>
    where TComponent : BUIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    private RenderFragment? _resolvedTemplate;
    private VariantHelper<TComponent, TVariant>? _variantHelper;

    Variant IVariantComponent.CurrentVariant => CurrentVariant;
    public TVariant CurrentVariant => Variant ?? DefaultVariant;
    public abstract TVariant DefaultVariant { get; }
    [Parameter] public TVariant? Variant { get; set; }

    Type IVariantComponent.VariantType => typeof(TVariant);
    protected abstract IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }
    [Inject] private IVariantRegistry? VariantRegistry { get; set; }

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