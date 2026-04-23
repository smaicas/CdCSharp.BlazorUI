using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Abstractions;

public abstract class BUIComponentBase : ComponentBase, IAsyncDisposable, IBuiltComponent
{
    private readonly BUIComponentPipeline _pipeline = new();

#if DEBUG
    [Inject] private IBUIPerformanceService? PerformanceService { get; set; }

    [Parameter]
    public bool TrackPerformanceEnabled { get; set; } = true;
#endif

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // Exposed as `public`: variant templates are `RenderFragment`s authored *outside* the
    // component's own .razor, so they need cross-assembly access to spread `@attributes` onto the
    // `<bui-component>` root. Protected would block the custom-variant pattern that is part of the
    // framework contract.
    public Dictionary<string, object> ComputedAttributes => _pipeline.ComputedAttributes;

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

    protected override void OnInitialized()
    {
        _pipeline.BeginInit();
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        _pipeline.BeginParametersSet();
        base.OnParametersSet();
        _pipeline.BuildStyles(this, AdditionalAttributes);
#if DEBUG
        _pipeline.EndParametersSet(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
#if DEBUG
            _pipeline.EndInit(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
            await _pipeline.AttachBehaviorAsync(this, BehaviorJsInterop);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        _pipeline.BeginRenderTree();
        _pipeline.PatchVolatileAttributes(this);
        base.BuildRenderTree(builder);
#if DEBUG
        _pipeline.EndRenderTree(GetType().Name, PerformanceService, TrackPerformanceEnabled);
#endif
    }

    public virtual ValueTask DisposeAsync() => _pipeline.DisposeBehaviorAsync();
}
