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
    private string? _lastComputedCssClasses;
    private string? _originalUserClasses;
    private string? _lastComputedStyles;
    private string? _originalUserStyles;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    [Inject] private IBehaviorJsInterop? BehaviorJsInterop { get; set; }

    public string ComputedCssClasses { get; private set; } = string.Empty;

    public virtual IEnumerable<string> GetAdditionalCssClasses() => [];

    public virtual Dictionary<string, string> GetAdditionalInlineStyles() => [];

    protected override void OnParametersSet()
    {
        // Handle CSS Classes
        string currentClasses = AdditionalAttributes.TryGetValue("class", out object? existingClass)
            ? existingClass.ToString() ?? string.Empty
            : string.Empty;

        // Detect if these are original user classes or our computed classes
        if (_lastComputedCssClasses == null || currentClasses != _lastComputedCssClasses)
        {
            // This is either first render or user changed the classes externally
            _originalUserClasses = currentClasses;
        }

        // Handle Inline Styles
        string currentStyles = AdditionalAttributes.TryGetValue("style", out object? existingStyle)
            ? existingStyle.ToString() ?? string.Empty
            : string.Empty;

        // Detect if these are original user styles or our computed styles
        if (_lastComputedStyles == null || currentStyles != _lastComputedStyles)
        {
            // This is either first render or user changed the styles externally
            _originalUserStyles = currentStyles;
        }

        // Get component classes
        List<string> componentClasses = [.. GetAdditionalCssClasses()];

        // Check if component implements IHasTransitions
        if (this is IHasTransitions hasTransitions && hasTransitions.Transitions?.HasTransitions == true)
        {
            componentClasses.Add(CssClassesReference.HasTransitions);
            foreach (string cssClass in hasTransitions.Transitions.GetCssClasses().Split(' ', StringSplitOptions.RemoveEmptyEntries))
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
        if (this is IHasElevation hasElevation && hasElevation.Elevation != null)
        {
            componentClasses.Add(CssClassesReference.Elevation(Math.Clamp((int)hasElevation.Elevation, 0, 24)));
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

        // Combine component classes with original user classes
        ComputedCssClasses = string.IsNullOrWhiteSpace(_originalUserClasses)
            ? string.Join(" ", componentClasses)
            : $"{string.Join(" ", componentClasses)} {_originalUserClasses}".Trim();

        // Store computed classes for next render
        _lastComputedCssClasses = ComputedCssClasses;

        // Update AdditionalAttributes with classes
        if (!string.IsNullOrWhiteSpace(ComputedCssClasses))
        {
            AdditionalAttributes["class"] = ComputedCssClasses;
        }
        else if (AdditionalAttributes.ContainsKey("class"))
        {
            AdditionalAttributes.Remove("class");
        }

        // Build component styles
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

        if (this is IHasBorder hasBorder)
        {
            // Procesar el radius del borde completo (tiene prioridad)
            if (hasBorder.Border?.Radius != null)
            {
                styles["border-radius"] = hasBorder.Border.GetRadiusCssValue();
            }

            // Border completo
            if (hasBorder.Border != null)
            {
                styles["border"] = hasBorder.Border.ToCssValue();
            }

            // Bordes individuales
            if (hasBorder.BorderTop != null)
            {
                styles["border-top"] = hasBorder.BorderTop.ToCssValue();
            }
            if (hasBorder.BorderRight != null)
            {
                styles["border-right"] = hasBorder.BorderRight.ToCssValue();
            }
            if (hasBorder.BorderBottom != null)
            {
                styles["border-bottom"] = hasBorder.BorderBottom.ToCssValue();
            }
            if (hasBorder.BorderLeft != null)
            {
                styles["border-left"] = hasBorder.BorderLeft.ToCssValue();
            }
        }

        // Build component styles string
        string componentStylesString = string.Join("; ", styles.Select(kv => $"{kv.Key}: {kv.Value}"));

        // Combine with original user styles
        string computedStyles = string.IsNullOrWhiteSpace(_originalUserStyles)
            ? componentStylesString
            : string.IsNullOrWhiteSpace(componentStylesString)
                ? _originalUserStyles
                : $"{componentStylesString}; {_originalUserStyles}";

        // Store computed styles for next render
        _lastComputedStyles = computedStyles;

        // Update AdditionalAttributes with styles
        if (!string.IsNullOrWhiteSpace(computedStyles))
        {
            AdditionalAttributes["style"] = computedStyles;
        }
        else if (AdditionalAttributes.ContainsKey("style"))
        {
            AdditionalAttributes.Remove("style");
        }

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