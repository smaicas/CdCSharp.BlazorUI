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
    }

    private void ProcessInterfaces(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        // Extraer nombre del componente
        string componentName = ToKebabCaseComponentName(component.GetType().Name);
        ComputedAttributes["data-ui-component"] = componentName;

        // IHasVariant
        if (component is IVariantComponent uiComponent)
        {
            ComputedAttributes["data-ui-variant"] = uiComponent.CurrentVariant.Name.ToLowerInvariant();
        }

        // IHasTransitions
        if (component is IHasTransitions hasTransitions && hasTransitions.Transitions?.HasTransitions == true)
        {
            ComputedAttributes["data-ui-transitions"] = hasTransitions.Transitions.GetDataAttributeValue();

            foreach ((string key, string value) in hasTransitions.Transitions.GetCssVariables())
            {
                cssVariables[key] = value;
            }
        }

        // IHasSize
        if (component is IHasSize hasSize)
        {
            ComputedAttributes["data-ui-size"] = hasSize.Size.ToString().ToLowerInvariant();
        }

        // IHasFullWidth
        if (component is IHasFullWidth hasFullWidth && hasFullWidth.FullWidth)
        {
            ComputedAttributes["data-ui-fullwidth"] = "true";
        }

        // IHasLoading
        if (component is IHasLoading hasLoading && hasLoading.IsLoading)
        {
            ComputedAttributes["data-ui-loading"] = "true";
        }

        // IHasElevation
        if (component is IHasElevation hasElevation && hasElevation.Elevation != null)
        {
            ComputedAttributes["data-ui-elevation"] = hasElevation.Elevation.Value.ToString();
        }

        // IHasRipple
        if (component is IHasRipple hasRipple && !hasRipple.DisableRipple)
        {
            ComputedAttributes["data-ui-ripple"] = "true";

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
        }

        // IHasBackgroundColor
        if (component is IHasBackgroundColor hasBackgroundColor && hasBackgroundColor.BackgroundColor != null)
        {
            cssVariables["--ui-bg-color"] = hasBackgroundColor.BackgroundColor.ToString(ColorOutputFormats.Rgba);
        }

        // IHasColor
        if (component is IHasColor hasColor && hasColor.Color != null)
        {
            cssVariables["--ui-color"] = hasColor.Color.ToString(ColorOutputFormats.Rgba);
        }

        // IHasBorder
        if (component is IHasBorder hasBorder)
        {
            if (hasBorder.Border != null)
            {
                cssVariables["--ui-border-width"] = hasBorder.Border.Width;
                cssVariables["--ui-border-style"] = hasBorder.Border.Style.ToString().ToLowerInvariant();
                cssVariables["--ui-border-color"] = hasBorder.Border.Color.ToString(ColorOutputFormats.Rgba);

                if (hasBorder.Border.Radius.HasValue)
                {
                    cssVariables["--ui-border-radius"] = $"{hasBorder.Border.Radius}px";
                }
            }

            // Individual borders
            if (hasBorder.BorderTop != null)
            {
                cssVariables["--ui-border-top-width"] = hasBorder.BorderTop.Width;
                cssVariables["--ui-border-top-style"] = hasBorder.BorderTop.Style.ToString().ToLowerInvariant();
                cssVariables["--ui-border-top-color"] = hasBorder.BorderTop.Color.ToString(ColorOutputFormats.Rgba);
            }

            if (hasBorder.BorderRight != null)
            {
                cssVariables["--ui-border-right-width"] = hasBorder.BorderRight.Width;
                cssVariables["--ui-border-right-style"] = hasBorder.BorderRight.Style.ToString().ToLowerInvariant();
                cssVariables["--ui-border-right-color"] = hasBorder.BorderRight.Color.ToString(ColorOutputFormats.Rgba);
            }

            if (hasBorder.BorderBottom != null)
            {
                cssVariables["--ui-border-bottom-width"] = hasBorder.BorderBottom.Width;
                cssVariables["--ui-border-bottom-style"] = hasBorder.BorderBottom.Style.ToString().ToLowerInvariant();
                cssVariables["--ui-border-bottom-color"] = hasBorder.BorderBottom.Color.ToString(ColorOutputFormats.Rgba);
            }

            if (hasBorder.BorderLeft != null)
            {
                cssVariables["--ui-border-left-width"] = hasBorder.BorderLeft.Width;
                cssVariables["--ui-border-left-style"] = hasBorder.BorderLeft.Style.ToString().ToLowerInvariant();
                cssVariables["--ui-border-left-color"] = hasBorder.BorderLeft.Color.ToString(ColorOutputFormats.Rgba);
            }
        }

        if (component is IHasError hasError)
        {
            ComputedAttributes["data-ui-error"] = hasError.IsError ? "true" : "false";
        }

        // IHasDisabled - siempre incluir
        if (component is IHasDisabled hasDisabled)
        {
            ComputedAttributes["data-ui-disabled"] = hasDisabled.IsDisabled ? "true" : "false";
        }

        // IHasReadOnly - siempre incluir
        if (component is IHasReadOnly hasReadOnly)
        {
            ComputedAttributes["data-ui-readonly"] = hasReadOnly.IsReadOnly ? "true" : "false";
        }

        // IHasRequired - siempre incluir
        if (component is IHasRequired hasRequired)
        {
            ComputedAttributes["data-ui-required"] = hasRequired.IsRequired ? "true" : "false";
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

    private static string ToKebabCaseComponentName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.StartsWith("UI", StringComparison.InvariantCultureIgnoreCase))
        {
            value = value[2..];
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