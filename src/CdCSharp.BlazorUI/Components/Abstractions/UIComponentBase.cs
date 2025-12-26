using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Abstractions;

public abstract class UIComponentBase : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

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
        if (this is IHasSize<Enum> hasSize)
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
        if (this is IHasDensity<Enum> hasDensity)
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