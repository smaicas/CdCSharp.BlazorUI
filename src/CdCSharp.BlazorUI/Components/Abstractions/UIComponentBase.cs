// Components/Abstractions/UIComponentBase.cs
using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Abstractions;

public abstract class UIComponentBase : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _behaviorInstance;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    [Inject] private IBehaviorJsInterop? BehaviorJsInterop { get; set; }

    public string ComputedCssClasses { get; private set; } = string.Empty;

    public virtual IEnumerable<string> GetAdditionalCssClasses() => [];

    public virtual Dictionary<string, string> GetAdditionalInlineStyles() => [];

    protected override void OnParametersSet()
    {
        // Get component classes
        List<string> componentClasses = [.. GetAdditionalCssClasses()];

        // Check if component implements IHasTransitions
        if (this is IHasTransitions hasTransitions && hasTransitions.Transitions?.HasTransitions == true)
        {
            componentClasses.Add(CssClassesReference.HasTransitions);
            foreach (string cssClass in hasTransitions.Transitions.GetCssClasses().Split(' '))
            {
                componentClasses.Add(cssClass);
            }
        }

        // Check if component implements IHasSize
        if (this is IHasSize hasSize)
        {
            componentClasses.Add(CssClassesReference.Size(hasSize.Size));
        }

        // Check if component implements IHasFullWidth
        if (this is IHasFullWidth hasFullWidth && hasFullWidth.FullWidth)
        {
            componentClasses.Add(CssClassesReference.FullWidth);
        }

        // Check if component implements IHasLoading
        if (this is IHasLoading hasLoading && hasLoading.IsLoading)
        {
            componentClasses.Add(CssClassesReference.Loading);
        }

        // Check if component implements IHasElevation
        if (this is IHasElevation hasElevation && hasElevation.Elevation > 0)
        {
            componentClasses.Add(CssClassesReference.Elevation(Math.Clamp(hasElevation.Elevation, 0, 24)));
        }

        // Check if component implements IHasRipple
        if (this is IHasRipple hasRipple && !hasRipple.DisableRipple)
        {
            componentClasses.Add(CssClassesReference.HasRipple);
        }

        // Check if component implements IHasDensity
        if (this is IHasDensity hasDensity)
        {
            componentClasses.Add(CssClassesReference.Density(hasDensity.Density));
        }

        // Get user classes
        string userClasses = AdditionalAttributes.TryGetValue("class", out object? existingClass)
            ? existingClass.ToString() ?? string.Empty
            : string.Empty;

        // Combine all classes
        ComputedCssClasses = string.IsNullOrWhiteSpace(userClasses)
            ? string.Join(" ", componentClasses)
            : $"{string.Join(" ", componentClasses)} {userClasses}".Trim();

        // Update AdditionalAttributes with classes
        if (!string.IsNullOrWhiteSpace(ComputedCssClasses))
        {
            AdditionalAttributes["class"] = ComputedCssClasses;
        }

        // Build styles
        Dictionary<string, string> styles = GetAdditionalInlineStyles();

        // Add transition styles if applicable
        if (this is IHasTransitions hasTransitionsForStyles && hasTransitionsForStyles.Transitions?.HasTransitions == true)
        {
            foreach ((string? key, string? value) in hasTransitionsForStyles.Transitions.GetInlineStyles())
            {
                styles[key] = value;
            }
        }

        // Add ripple styles if applicable
        if (this is IHasRipple hasRippleForStyles && !hasRippleForStyles.DisableRipple)
        {
            if (hasRippleForStyles.RippleColor != null)
            {
                styles["--ui-ripple-color"] = hasRippleForStyles.RippleColor.ToString(ColorOutputFormats.Rgba);
            }
            if (hasRippleForStyles.RippleDuration > 0)
            {
                styles["--ui-ripple-duration"] = $"{hasRippleForStyles.RippleDuration}ms";
            }
        }

        // Merge styles
        MergeAttribute("style", string.Join(";", styles.Select(kv => $"{kv.Key}: {kv.Value}")), ";");

        base.OnParametersSet();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && BehaviorJsInterop != null && this is IJsBehavior jsBehavior)
        {
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

    private void MergeAttribute(string key, string newValue, string separator)
    {
        if (string.IsNullOrWhiteSpace(newValue)) { return; }

        if (AdditionalAttributes.TryGetValue(key, out object? existing))
        {
            AdditionalAttributes[key] = $"{newValue}{separator}{existing}";
        }
        else
        {
            AdditionalAttributes[key] = newValue;
        }
    }
}