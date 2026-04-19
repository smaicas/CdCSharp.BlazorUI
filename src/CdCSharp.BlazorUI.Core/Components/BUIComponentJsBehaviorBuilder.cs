using CdCSharp.BlazorUI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Core.Components;

internal sealed class BUIComponentJsBehaviorBuilder
{
    private readonly ComponentBase _component;
    private readonly BehaviorConfiguration _config = new();
    private readonly IBehaviorJsInterop _jsInterop;

    private BUIComponentJsBehaviorBuilder(
        ComponentBase component,
        IBehaviorJsInterop jsInterop)
    {
        _component = component;
        _jsInterop = jsInterop;
    }

    public static BUIComponentJsBehaviorBuilder For(
        ComponentBase component,
        IBehaviorJsInterop jsInterop) => new(component, jsInterop);

    public async Task<IJSObjectReference?> BuildAndAttachAsync()
    {
        if (_jsInterop == null || _component is not IJsBehavior)
            return null;

        ConfigureRipple();

        if (!_config.HasAnyBehavior)
            return null;

        return await _jsInterop.AttachBehaviorsAsync(_config);
    }

    // ───────────────────────────────────────────── Behavior configurations ─────────────────────────────────────────────

    private void ConfigureRipple()
    {
        if (_component is not IHasRipple hasRipple || hasRipple.DisableRipple)
            return;

        _config.Ripple = new RippleConfiguration
        {
            Color = hasRipple.RippleColor,
            Duration = hasRipple.RippleDurationMs,
            RippleContainer = hasRipple.GetRippleContainer(),
        };
    }
}