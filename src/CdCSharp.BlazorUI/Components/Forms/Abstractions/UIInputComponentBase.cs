using CdCSharp.BlazorUI.Components.Abstractions;
using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Core.Css;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Forms.Abstractions;

public abstract class UIInputComponentBase<TValue> : InputBase<TValue>, IAsyncDisposable
{
    private IJSObjectReference? _behaviorInstance;
    private readonly ComponentStyleBuilder _styleBuilder = new();

    [Inject] private IBehaviorJsInterop? BehaviorJsInterop { get; set; }

    public string ComputedCssClasses => _styleBuilder.ComputedCssClasses;

    // This is what components will use with @attributes
    protected Dictionary<string, object> ComputedAttributes => _styleBuilder.ComputedAttributes;

    // Now returns CssClass from InputBase as part of the collection
    public virtual IEnumerable<string> GetAdditionalCssClasses()
    {
        if (!string.IsNullOrWhiteSpace(CssClass))
        {
            yield return CssClass;
        }
    }

    public virtual Dictionary<string, string> GetAdditionalInlineStyles() => [];

    protected override void OnParametersSet()
    {
        _styleBuilder.BuildStyles(
            this,
            AdditionalAttributes, // This comes from InputBase as IReadOnlyDictionary
            GetAdditionalCssClasses(),
            GetAdditionalInlineStyles()
        );

        base.OnParametersSet();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && BehaviorJsInterop != null && this is IJsBehavior jsBehavior)
        {
            // Check if component is loading - don't attach behaviors during loading state
            if (this is IHasLoading hasLoading && hasLoading.IsLoading)
            {
                return; // Skip behavior attachment when loading
            }

            ElementReference rootElement = jsBehavior.GetRootElement();
            BehaviorConfiguration config = new();

            // Configure ripple if applicable
            if (this is IHasRipple hasRipple && !hasRipple.DisableRipple)
            {
                config.Ripple = new RippleConfiguration
                {
                    Color = hasRipple.RippleColor?.ToString(ColorOutputFormats.Rgba),
                    Duration = hasRipple.RippleDuration
                };
            }

            // Add more behaviors here as needed
            // if (this is IHasTooltip hasTooltip) { ... }

            // Attach behaviors if any configured
            if (config.HasAnyBehavior)
            {
                _behaviorInstance = await BehaviorJsInterop.AttachBehaviorsAsync(
                    rootElement, config);
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_behaviorInstance != null)
        {
            await _behaviorInstance.InvokeVoidAsync("dispose");
            await _behaviorInstance.DisposeAsync();
        }
    }
}