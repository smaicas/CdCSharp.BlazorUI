using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Abstractions;

internal sealed class ComponentStyleBuilder
{
    private string? _lastComputedCssClasses;
    private string? _originalUserClasses;
    private string? _lastComputedStyles;
    private string? _originalUserStyles;

    public string ComputedCssClasses { get; private set; } = string.Empty;
    public Dictionary<string, object> ComputedAttributes { get; private set; } = [];

    public void BuildStyles(
        ComponentBase component,
        IReadOnlyDictionary<string, object>? additionalAttributes,
        IEnumerable<string> additionalCssClasses,
        Dictionary<string, string> additionalInlineStyles)
    {
        // Start with a fresh dictionary, copying from additionalAttributes if provided
        ComputedAttributes = additionalAttributes != null
            ? new Dictionary<string, object>(additionalAttributes)
            : [];

        // Handle CSS Classes
        string currentClasses = ComputedAttributes.TryGetValue("class", out object? existingClass)
            ? existingClass.ToString() ?? string.Empty
            : string.Empty;

        // Detect if these are original user classes or our computed classes
        if (_lastComputedCssClasses == null || currentClasses != _lastComputedCssClasses)
        {
            _originalUserClasses = currentClasses;
        }

        // Handle Inline Styles
        string currentStyles = ComputedAttributes.TryGetValue("style", out object? existingStyle)
            ? existingStyle.ToString() ?? string.Empty
            : string.Empty;

        if (_lastComputedStyles == null || currentStyles != _lastComputedStyles)
        {
            _originalUserStyles = currentStyles;
        }

        // Get component classes directly from additionalCssClasses
        List<string> componentClasses = [.. additionalCssClasses];

        // Process all interfaces
        ProcessInterfaces(component, componentClasses, additionalInlineStyles);

        // Combine component classes with original user classes
        ComputedCssClasses = string.IsNullOrWhiteSpace(_originalUserClasses)
            ? string.Join(" ", componentClasses)
            : $"{string.Join(" ", componentClasses)} {_originalUserClasses}".Trim();

        // Store computed classes for next render
        _lastComputedCssClasses = ComputedCssClasses;

        // Update ComputedAttributes with classes
        if (!string.IsNullOrWhiteSpace(ComputedCssClasses))
        {
            ComputedAttributes["class"] = ComputedCssClasses;
        }
        else if (ComputedAttributes.ContainsKey("class"))
        {
            ComputedAttributes.Remove("class");
        }

        // Build inline styles
        BuildInlineStyles(component, additionalInlineStyles);
    }

    private void ProcessInterfaces(ComponentBase component, List<string> componentClasses, Dictionary<string, string> styles)
    {
        // Check if component implements IHasTransitions
        if (component is IHasTransitions hasTransitions && hasTransitions.Transitions?.HasTransitions == true)
        {
            componentClasses.Add(CssClassesReference.HasTransitions);
            foreach (string cssClass in hasTransitions.Transitions.GetCssClasses().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                componentClasses.Add(cssClass);
            }

            foreach ((string? key, string? value) in hasTransitions.Transitions.GetInlineStyles())
            {
                styles[key] = value;
            }
        }

        // Check if component implements IHasSize
        if (component is IHasSize hasSize)
        {
            componentClasses.Add(CssClassesReference.Size(hasSize.Size));
        }

        // Check if component implements IHasFullWidth
        if (component is IHasFullWidth hasFullWidth && hasFullWidth.FullWidth)
        {
            componentClasses.Add(CssClassesReference.FullWidth);
        }

        // Check if component implements IHasLoading
        if (component is IHasLoading hasLoading && hasLoading.IsLoading)
        {
            componentClasses.Add(CssClassesReference.Loading);
        }

        // Check if component implements IHasElevation
        if (component is IHasElevation hasElevation && hasElevation.Elevation != null)
        {
            componentClasses.Add(CssClassesReference.Elevation(Math.Clamp((int)hasElevation.Elevation, 0, 24)));
        }

        // Check if component implements IHasRipple
        if (component is IHasRipple hasRipple && !hasRipple.DisableRipple)
        {
            componentClasses.Add(CssClassesReference.HasRipple);

            if (hasRipple.RippleColor != null)
            {
                styles["--ui-ripple-color"] = hasRipple.RippleColor.ToString(ColorOutputFormats.Rgba);
            }

            if (hasRipple.RippleDuration > 0)
            {
                styles["--ui-ripple-duration"] = $"{hasRipple.RippleDuration}ms";
            }
        }

        // Check if component implements IHasDensity
        if (component is IHasDensity hasDensity)
        {
            componentClasses.Add(CssClassesReference.Density(hasDensity.Density));
        }
    }

    private void BuildInlineStyles(ComponentBase component, Dictionary<string, string> styles)
    {
        if (component is IHasBorder hasBorder)
        {
            if (hasBorder.Border?.Radius != null)
            {
                styles["border-radius"] = hasBorder.Border.GetRadiusCssValue();
            }

            if (hasBorder.Border != null)
                styles["border"] = hasBorder.Border.ToCssValue();

            if (hasBorder.BorderTop != null)
                styles["border-top"] = hasBorder.BorderTop.ToCssValue();

            if (hasBorder.BorderRight != null)
                styles["border-right"] = hasBorder.BorderRight.ToCssValue();

            if (hasBorder.BorderBottom != null)
                styles["border-bottom"] = hasBorder.BorderBottom.ToCssValue();

            if (hasBorder.BorderLeft != null)
                styles["border-left"] = hasBorder.BorderLeft.ToCssValue();
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

        // Update ComputedAttributes with styles
        if (!string.IsNullOrWhiteSpace(computedStyles))
        {
            ComputedAttributes["style"] = computedStyles;
        }
        else if (ComputedAttributes.ContainsKey("style"))
        {
            ComputedAttributes.Remove("style");
        }
    }
}