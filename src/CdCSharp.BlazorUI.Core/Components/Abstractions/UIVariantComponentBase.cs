using CdCSharp.BlazorUI.Core.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Core.Components.Abstractions;

public abstract class UIVariantComponentBase<TComponent, TVariant> : UIComponentBase
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    [Parameter] public TVariant? Variant { get; set; }

    [Inject] private IVariantRegistry<TComponent, TVariant>? VariantRegistry { get; set; }

    private RenderFragment? _resolvedTemplate;

    protected abstract TVariant DefaultVariant { get; }

    /// <summary>
    /// Built-in templates provided by the component itself.
    /// These have the lowest precedence.
    /// </summary>
    protected abstract IReadOnlyDictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }

    protected override void OnParametersSet()
    {
        Variant ??= DefaultVariant;
        _resolvedTemplate = ResolveTemplate();
        base.OnParametersSet();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        if (_resolvedTemplate is not null)
        {
            builder.AddContent(0, _resolvedTemplate);
        }
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
