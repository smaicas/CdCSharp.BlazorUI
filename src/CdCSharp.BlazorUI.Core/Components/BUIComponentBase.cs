using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Core.Abstractions.Components;

public abstract class BUIComponentBase : ComponentBase, IAsyncDisposable, IBuiltComponent
{
    private readonly BUIComponentAttributesBuilder _styleBuilder = new();
    private IJSObjectReference? _behaviorInstance;

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // This is what components will use with @attributes
    public Dictionary<string, object> ComputedAttributes => _styleBuilder.ComputedAttributes;

    [Inject] private IBehaviorJsInterop BehaviorJsInterop { get; set; } = default!;

    /// <summary>
    /// Override this method in derived components to add custom CSS variables. These will be merged
    /// with the standard behavior CSS variables. This method is called from
    /// BUIComponentAttributesBuilder during the style building process.
    /// </summary>
    /// <param name="cssVariables">
    /// Dictionary to add CSS variables to
    /// </param>
    public virtual void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
    {
        // Default implementation does nothing Derived classes can override to add their custom variables
    }

    /// <summary>
    /// Override this method to add component-specific data attributes. Called during attribute
    /// building process.
    /// </summary>
    public virtual void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
    {
        // Default: no custom data attributes
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_behaviorInstance != null)
        {
            await _behaviorInstance.InvokeVoidAsync("dispose");
            await _behaviorInstance.DisposeAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _behaviorInstance = await BUIComponentJsBehaviorBuilder
                .For(this, BehaviorJsInterop)
                .BuildAndAttachAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _styleBuilder.BuildStyles(this, AdditionalAttributes);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        _styleBuilder.PatchVolatileAttributes(this);
        base.BuildRenderTree(builder);
    }
}