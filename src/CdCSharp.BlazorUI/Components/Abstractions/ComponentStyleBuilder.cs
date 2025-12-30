using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Core.Css;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace CdCSharp.BlazorUI.Components.Abstractions;

internal sealed class ComponentStyleBuilder
{
    private string? _originalUserStyles;
    private readonly Dictionary<string, string> _appliedFeatures = [];

    public Dictionary<string, object> ComputedAttributes { get; private set; } = [];

    public void BuildStyles(
        ComponentBase component,
        IReadOnlyDictionary<string, object>? additionalAttributes)
    {
        // Start with a fresh dictionary, copying from additionalAttributes if provided
        ComputedAttributes = additionalAttributes != null
            ? new Dictionary<string, object>(additionalAttributes)
            : [];

        // Handle inline styles from user
        string currentStyles = ComputedAttributes.TryGetValue("style", out object? existingStyle)
            ? existingStyle.ToString() ?? string.Empty
            : string.Empty;

        _originalUserStyles = currentStyles;

        // Dictionary for CSS variables
        Dictionary<string, string> cssVariables = [];

        // Process all interfaces
        ProcessInterfaces(component, cssVariables);

        // Build inline styles with CSS variables
        BuildInlineStyles(cssVariables);

        // Add debug info in development
#if DEBUG
        AddDebugInfo();
#endif
    }

    private void ProcessInterfaces(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        // Extract component name
        string componentName = ToKebabCaseComponentName(component.GetType().Name);
        ComputedAttributes["data-ui-component"] = componentName;

        // Track applied features for debugging
        _appliedFeatures.Clear();

        // IVariantComponent
        if (component is IVariantComponent uiComponent)
        {
            ComputedAttributes["data-ui-variant"] = uiComponent.CurrentVariant.Name.ToLowerInvariant();
            _appliedFeatures["variant"] = uiComponent.CurrentVariant.Name;
        }

        // IHasTransitions
        if (component is IHasTransitions hasTransitions && hasTransitions.Transitions?.HasTransitions == true)
        {
            ComputedAttributes["data-ui-transitions"] = hasTransitions.Transitions.GetDataAttributeValue();

            foreach ((string key, string value) in hasTransitions.Transitions.GetCssVariables())
            {
                cssVariables[key] = value;
            }
            _appliedFeatures["transitions"] = "enabled";
        }

        // IHasSize
        if (component is IHasSize hasSize)
        {
            ComputedAttributes["data-ui-size"] = hasSize.Size.ToString().ToLowerInvariant();
            _appliedFeatures["size"] = hasSize.Size.ToString();
        }

        // IHasFullWidth
        if (component is IHasFullWidth hasFullWidth && hasFullWidth.FullWidth)
        {
            ComputedAttributes["data-ui-fullwidth"] = "true";
            _appliedFeatures["fullwidth"] = "true";
        }

        // IHasLoading
        if (component is IHasLoading hasLoading && hasLoading.IsLoading)
        {
            ComputedAttributes["data-ui-loading"] = "true";
            _appliedFeatures["loading"] = "true";
        }

        // IHasElevation
        if (component is IHasElevation hasElevation && hasElevation.Elevation != null)
        {
            ComputedAttributes["data-ui-elevation"] = hasElevation.Elevation.Value.ToString();
            _appliedFeatures["elevation"] = hasElevation.Elevation.Value.ToString();
        }

        // IHasRipple
        if (component is IHasRipple hasRipple && !hasRipple.DisableRipple)
        {
            ComputedAttributes["data-ui-ripple"] = "true";
            _appliedFeatures["ripple"] = "true";

            if (hasRipple.RippleColor != null)
            {
                cssVariables["--ui-ripple-color"] = hasRipple.RippleColor.ToString(ColorOutputFormats.Rgba);
            }

            if (hasRipple.RippleDuration > 0)
            {
                cssVariables["--ui-ripple-duration"] = $"{hasRipple.RippleDuration}ms";
            }
        }

        // IHasDensity
        if (component is IHasDensity hasDensity)
        {
            ComputedAttributes["data-ui-density"] = hasDensity.Density.ToString().ToLowerInvariant();
            _appliedFeatures["density"] = hasDensity.Density.ToString();
        }

        // IHasBackgroundColor
        if (component is IHasBackgroundColor hasBackgroundColor && hasBackgroundColor.BackgroundColor != null)
        {
            cssVariables["--ui-bg-color"] = hasBackgroundColor.BackgroundColor.ToString(ColorOutputFormats.Rgba);
            _appliedFeatures["bgcolor"] = "custom";
        }

        // IHasColor
        if (component is IHasColor hasColor && hasColor.Color != null)
        {
            cssVariables["--ui-color"] = hasColor.Color.ToString(ColorOutputFormats.Rgba);
            _appliedFeatures["color"] = "custom";
        }

        // IHasBorder
        if (component is IHasBorder hasBorder)
        {
            ProcessBorderStyles(hasBorder, cssVariables);
        }

        // State attributes - always include
        if (component is IHasError hasError)
        {
            ComputedAttributes["data-ui-error"] = hasError.IsError ? "true" : "false";
            if (hasError.IsError) _appliedFeatures["error"] = "true";
        }

        if (component is IHasDisabled hasDisabled)
        {
            ComputedAttributes["data-ui-disabled"] = hasDisabled.IsDisabled ? "true" : "false";
            if (hasDisabled.IsDisabled) _appliedFeatures["disabled"] = "true";
        }

        if (component is IHasReadOnly hasReadOnly)
        {
            ComputedAttributes["data-ui-readonly"] = hasReadOnly.IsReadOnly ? "true" : "false";
            if (hasReadOnly.IsReadOnly) _appliedFeatures["readonly"] = "true";
        }

        if (component is IHasRequired hasRequired)
        {
            ComputedAttributes["data-ui-required"] = hasRequired.IsRequired ? "true" : "false";
            if (hasRequired.IsRequired) _appliedFeatures["required"] = "true";
        }
    }

    private void ProcessBorderStyles(IHasBorder hasBorder, Dictionary<string, string> cssVariables)
    {
        bool hasBorderStyles = false;

        if (hasBorder.Border != null)
        {
            cssVariables["--ui-border-width"] = hasBorder.Border.Width;
            cssVariables["--ui-border-style"] = hasBorder.Border.Style.ToString().ToLowerInvariant();
            cssVariables["--ui-border-color"] = hasBorder.Border.Color.ToString(ColorOutputFormats.Rgba);

            if (hasBorder.Border.Radius.HasValue)
            {
                cssVariables["--ui-border-radius"] = $"{hasBorder.Border.Radius}px";
            }
            hasBorderStyles = true;
        }

        // Individual borders
        if (hasBorder.BorderTop != null)
        {
            cssVariables["--ui-border-top-width"] = hasBorder.BorderTop.Width;
            cssVariables["--ui-border-top-style"] = hasBorder.BorderTop.Style.ToString().ToLowerInvariant();
            cssVariables["--ui-border-top-color"] = hasBorder.BorderTop.Color.ToString(ColorOutputFormats.Rgba);
            hasBorderStyles = true;
        }

        if (hasBorder.BorderRight != null)
        {
            cssVariables["--ui-border-right-width"] = hasBorder.BorderRight.Width;
            cssVariables["--ui-border-right-style"] = hasBorder.BorderRight.Style.ToString().ToLowerInvariant();
            cssVariables["--ui-border-right-color"] = hasBorder.BorderRight.Color.ToString(ColorOutputFormats.Rgba);
            hasBorderStyles = true;
        }

        if (hasBorder.BorderBottom != null)
        {
            cssVariables["--ui-border-bottom-width"] = hasBorder.BorderBottom.Width;
            cssVariables["--ui-border-bottom-style"] = hasBorder.BorderBottom.Style.ToString().ToLowerInvariant();
            cssVariables["--ui-border-bottom-color"] = hasBorder.BorderBottom.Color.ToString(ColorOutputFormats.Rgba);
            hasBorderStyles = true;
        }

        if (hasBorder.BorderLeft != null)
        {
            cssVariables["--ui-border-left-width"] = hasBorder.BorderLeft.Width;
            cssVariables["--ui-border-left-style"] = hasBorder.BorderLeft.Style.ToString().ToLowerInvariant();
            cssVariables["--ui-border-left-color"] = hasBorder.BorderLeft.Color.ToString(ColorOutputFormats.Rgba);
            hasBorderStyles = true;
        }

        if (hasBorderStyles)
        {
            _appliedFeatures["border"] = "custom";
        }
    }

    private void BuildInlineStyles(Dictionary<string, string> cssVariables)
    {
        // Build styles string from CSS variables
        string cssVariablesString = string.Join("; ", cssVariables.Select(kv => $"{kv.Key}: {kv.Value}"));

        // Combine with original user styles
        string computedStyles = string.IsNullOrWhiteSpace(_originalUserStyles)
            ? cssVariablesString
            : string.IsNullOrWhiteSpace(cssVariablesString)
                ? _originalUserStyles
                : $"{cssVariablesString}; {_originalUserStyles}";

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

#if DEBUG
    private void AddDebugInfo()
    {
        if (_appliedFeatures.Any())
        {
            ComputedAttributes["data-ui-features"] = string.Join(",", _appliedFeatures.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        }
    }
#endif

    private static string ToKebabCaseComponentName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Remove UI prefix if present
        if (value.StartsWith("UI", StringComparison.InvariantCultureIgnoreCase))
        {
            value = value[2..];
        }
        // Remove BUI prefix if present
        else if (value.StartsWith("BUI", StringComparison.InvariantCultureIgnoreCase))
        {
            value = value[3..];
        }

        StringBuilder sb = new();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            if (char.IsUpper(c))
            {
                if (i > 0)
                    sb.Append('-');
                sb.Append(char.ToLower(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}