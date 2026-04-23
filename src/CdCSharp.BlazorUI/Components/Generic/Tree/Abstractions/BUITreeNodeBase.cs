using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Components;

/// <summary>
/// Registration-only base for tree nodes (<c>BUITreeMenuItem</c>, <c>BUITreeSelectorItem</c>).
/// Intentionally inherits <see cref="ComponentBase"/> (not <c>BUIComponentBase</c>) because the
/// node registers itself with the enclosing tree container via cascading parameters and the
/// container renders the flattened structure. The node does not emit its own
/// <c>&lt;bui-component&gt;</c> root.
/// </summary>
public abstract class BUITreeNodeBase<TRegistration> : ComponentBase
    where TRegistration : TreeNodeRegistration
{
    private bool _registered;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public object? Data { get; set; }

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public string? Icon { get; set; }

    [Parameter] public bool InitiallyExpanded { get; set; }

    [Parameter] public string? Key { get; set; }

    [Parameter] public RenderFragment? NodeContent { get; set; }

    [Parameter] public string? Text { get; set; }

    [CascadingParameter(Name = "ParentNodeKey")]
    internal string? ParentNodeKey { get; set; }

    [CascadingParameter(Name = "TreeNodeRegistry")]
    internal ITreeNodeRegistry<TRegistration>? Registry { get; set; }

    protected string ResolvedKey { get; private set; } = string.Empty;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Registry != null && ChildContent != null)
        {
            builder.OpenComponent<CascadingValue<string>>(0);
            builder.AddComponentParameter(1, "Value", ResolvedKey);
            builder.AddComponentParameter(2, "Name", "ParentNodeKey");
            builder.AddComponentParameter(3, "ChildContent", ChildContent);
            builder.CloseComponent();
        }
    }

    protected abstract TRegistration CreateRegistration();

    protected abstract string GenerateDefaultKey();

    protected override void OnInitialized() => ResolvedKey = Key ?? GenerateDefaultKey();

    protected override void OnParametersSet()
    {
        if (Registry != null && !_registered)
        {
            TRegistration registration = CreateRegistration();
            Registry.Register(registration);
            _registered = true;
        }
    }
}