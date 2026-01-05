using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;
using CdCSharp.BlazorUI.Core.Css;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Core.Components;

internal sealed class BUIComponentJsBehaviorBuilder
{
    private readonly ComponentBase _component;
    private readonly IBehaviorJsInterop _jsInterop;
    private readonly BehaviorConfiguration _config = new();

    private BUIComponentJsBehaviorBuilder(
        ComponentBase component,
        IBehaviorJsInterop jsInterop)
    {
        _component = component;
        _jsInterop = jsInterop;
    }

    public static BUIComponentJsBehaviorBuilder For(
        ComponentBase component,
        IBehaviorJsInterop jsInterop)
    {
        return new(component, jsInterop);
    }

    public async Task<IJSObjectReference?> BuildAndAttachAsync()
    {
        if (_jsInterop == null || _component is not IJsBehavior)
            return null;

        ConfigureRipple();

        if (!_config.HasAnyBehavior)
            return null;

        return await _jsInterop.AttachBehaviorsAsync(_config);
    }

    // ─────────────────────────────────────────────
    // Behavior configurations
    // ─────────────────────────────────────────────

    private void ConfigureRipple()
    {
        if (_component is not IHasRipple hasRipple || hasRipple.DisableRipple)
            return;

        _config.Ripple = new RippleConfiguration
        {
            Color = hasRipple.RippleColor?.ToString(ColorOutputFormats.Rgba),
            Duration = hasRipple.RippleDuration,
            RippleContainer = hasRipple.GetRippleContainer(),
        };
    }
}