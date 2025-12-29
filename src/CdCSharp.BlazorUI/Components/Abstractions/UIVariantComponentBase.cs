using CdCSharp.BlazorUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Components.Abstractions;

public interface IVariantComponent
{
    Variant CurrentVariant { get; }
}

public abstract class UIVariantComponentBase<TComponent, TVariant> : UIComponentBase, IVariantComponent
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    private RenderFragment? _resolvedTemplate;
    [Parameter] public TVariant? Variant { get; set; }

    /// <summary>
    /// Built-in templates provided by the component itself. These have the lowest precedence.
    /// </summary>
    protected abstract IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }

    protected abstract TVariant DefaultVariant { get; }
    [Inject] private IVariantRegistry<TComponent, TVariant>? VariantRegistry { get; set; }

    Variant IVariantComponent.CurrentVariant => Variant ?? DefaultVariant;

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
        Variant ??= DefaultVariant;
        _resolvedTemplate = ResolveTemplate();
    }

    private RenderFragment? ResolveTemplate()
    {
        // Built-in templates
        if (BuiltInTemplates.TryGetValue(Variant!, out Func<TComponent, RenderFragment>? builtIn))
        {
            return builtIn((TComponent)this);
        }

        // Registered variants
        return VariantRegistry?.GetTemplate(Variant!, (TComponent)this);
    }
}