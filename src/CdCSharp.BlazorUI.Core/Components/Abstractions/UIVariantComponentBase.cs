using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Core.Components.Abstractions;

public abstract class UIVariantComponentBase<TComponent, TVariant> : UIComponentBase
    where TComponent : UIVariantComponentBase<TComponent, TVariant>
    where TVariant : Variant
{
    [Parameter] public TVariant Variant { get; set; } = default!;
    [Inject] private IVariantRegistry<TComponent, TVariant>? Registry { get; set; }

    protected abstract TVariant DefaultVariant { get; }
    protected abstract Dictionary<TVariant, Func<TComponent, RenderFragment>> BuiltInTemplates { get; }

    protected override void OnInitialized()
    {
        Variant ??= DefaultVariant;
        base.OnInitialized();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (GetTemplate() is { } template)
        {
            builder.AddContent(0, template);
        }
        base.BuildRenderTree(builder);
    }

    private RenderFragment? GetTemplate()
    {
        // First search in built-in templates
        if (BuiltInTemplates.TryGetValue(Variant, out Func<TComponent, RenderFragment>? builtIn))
        {
            return builtIn((TComponent)this);
        }

        // Then search in registry
        return Registry?.GetTemplate(Variant, (TComponent)this);
    }
}