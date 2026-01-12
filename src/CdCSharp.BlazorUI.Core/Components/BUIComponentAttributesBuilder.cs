using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Families;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.State;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Transitions;
using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;
using CdCSharp.BlazorUI.Core.Css;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace CdCSharp.BlazorUI.Core.Abstractions.Components;

internal sealed class BUIComponentAttributesBuilder
{
    private string? _originalUserStyles;
    public Dictionary<string, object> ComputedAttributes { get; private set; } = [];

    public void BuildStyles(
        ComponentBase component,
        IReadOnlyDictionary<string, object>? additionalAttributes)
    {
        ComputedAttributes = additionalAttributes != null
            ? new Dictionary<string, object>(additionalAttributes)
            : [];

        _originalUserStyles = ComputedAttributes.TryGetValue("style", out object? style)
            ? style?.ToString()
            : null;

        Dictionary<string, string> cssVariables = [];

        // Nombre del componente
        string componentName = ToKebabCaseComponentName(component.GetType().Name);
        ComputedAttributes[FeatureDefinitions.DataAttributes.Component] = componentName;

        if (component is IVariantComponent variantComponent)
        {
            string dataAttr = FeatureDefinitions.DataAttributes.Variant;
            string value = variantComponent.CurrentVariant.Name.ToLowerInvariant();
            ComputedAttributes[dataAttr] = value;
        }

        // ===== FEATURES =====
        BuildFamilyAttributes(component);

        BuildSize(component);
        BuildDensity(component);
        BuildFullWidth(component);
        BuildElevation(component, cssVariables);
        BuildLoading(component);
        BuildError(component);
        BuildDisabled(component);
        BuildReadOnly(component);
        BuildRequired(component);
        BuildRipple(component, cssVariables);
        BuildColor(component, cssVariables);
        BuildBackgroundColor(component, cssVariables);
        BuildBorder(component, cssVariables);
        BuildTransitions(component, cssVariables);

        // ===== COMPONENT-SPECIFIC DATA ATTRIBUTES =====
        if (component is BUIComponentBase buiComponent)
        {
            buiComponent.BuildComponentDataAttributes(ComputedAttributes);
        }

        // ===== COMPONENT-SPECIFIC CSS VARIABLES =====
        if (component is BUIComponentBase buiComponentForVars)
        {
            buiComponentForVars.BuildComponentCssVariables(cssVariables);
        }

        BuildInlineStyles(cssVariables);
    }

    // -------- Feature Methods --------
    private void BuildSize(ComponentBase component)
    {
        if (component is IHasSize size)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Size] = size.Size.ToString().ToLowerInvariant();
    }

    private void BuildDensity(ComponentBase component)
    {
        if (component is IHasDensity density)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Density] = density.Density.ToString().ToLowerInvariant();
    }

    private void BuildFullWidth(ComponentBase component)
    {
        if (component is IHasFullWidth fullWidth)
            ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth] = fullWidth.FullWidth.ToString().ToLowerInvariant();
    }

    private void BuildElevation(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasElevation elevation)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Elevation] = elevation.Elevation?.ToString() ?? "0";

            if (elevation.ElevationColor != null)
            {
                cssVariables[FeatureDefinitions.CssVariables.ElevationShadowColor] =
                    elevation.ElevationColor.ToString(ColorOutputFormats.Rgba);
            }
        }
    }

    private void BuildLoading(ComponentBase component)
    {
        if (component is IHasLoading loading)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Loading] = loading.IsLoading.ToString().ToLowerInvariant();
    }

    private void BuildError(ComponentBase component)
    {
        if (component is IHasError error)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Error] = error.IsError.ToString().ToLowerInvariant();
    }

    private void BuildDisabled(ComponentBase component)
    {
        if (component is IHasDisabled disabled)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled] = disabled.IsDisabled.ToString().ToLowerInvariant();
    }

    private void BuildReadOnly(ComponentBase component)
    {
        if (component is IHasReadOnly readOnly)
            ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly] = readOnly.IsReadOnly.ToString().ToLowerInvariant();
    }

    private void BuildRequired(ComponentBase component)
    {
        if (component is IHasRequired required)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Required] = required.IsRequired.ToString().ToLowerInvariant();
    }

    private void BuildRipple(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasRipple ripple)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Ripple] = (!ripple.DisableRipple).ToString().ToLowerInvariant();

            if (!ripple.DisableRipple)
            {
                if (ripple.RippleColor != null)
                    cssVariables[FeatureDefinitions.CssVariables.RippleColor] = ripple.RippleColor.ToString(ColorOutputFormats.Rgba);
                if (ripple.RippleDuration.HasValue)
                    cssVariables[FeatureDefinitions.CssVariables.RippleDuration] = $"{ripple.RippleDuration}ms";
            }
        }
    }

    private void BuildColor(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasColor color && color.Color != null)
            cssVariables[FeatureDefinitions.CssVariables.Color] = color.Color.ToString(ColorOutputFormats.Rgba);
    }

    private void BuildBackgroundColor(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasBackgroundColor bg && bg.BackgroundColor != null)
            cssVariables[FeatureDefinitions.CssVariables.BackgroundColor] = bg.BackgroundColor.ToString(ColorOutputFormats.Rgba);
    }

    private void BuildBorder(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasBorder border)
        {
            // Shorthand
            if (border.Border != null)
            {
                cssVariables[FeatureDefinitions.CssVariables.BorderWidth] = border.Border.Width;
                cssVariables[FeatureDefinitions.CssVariables.BorderStyle] = border.Border.Style.ToString().ToLowerInvariant();
                cssVariables[FeatureDefinitions.CssVariables.BorderColor] = border.Border.Color.ToString(ColorOutputFormats.Rgba);

                if (border.Border.Radius.HasValue)
                    cssVariables[FeatureDefinitions.CssVariables.BorderRadius] = $"{border.Border.Radius}px";
            }

            // Individual borders override shorthand
            if (border.BorderTop != null)
            {
                cssVariables[FeatureDefinitions.CssVariables.BorderTopWidth] = border.BorderTop.Width;
                cssVariables[FeatureDefinitions.CssVariables.BorderTopStyle] = border.BorderTop.Style.ToString().ToLowerInvariant();
                cssVariables[FeatureDefinitions.CssVariables.BorderTopColor] = border.BorderTop.Color.ToString(ColorOutputFormats.Rgba);
            }

            if (border.BorderRight != null)
            {
                cssVariables[FeatureDefinitions.CssVariables.BorderRightWidth] = border.BorderRight.Width;
                cssVariables[FeatureDefinitions.CssVariables.BorderRightStyle] = border.BorderRight.Style.ToString().ToLowerInvariant();
                cssVariables[FeatureDefinitions.CssVariables.BorderRightColor] = border.BorderRight.Color.ToString(ColorOutputFormats.Rgba);
            }

            if (border.BorderBottom != null)
            {
                cssVariables[FeatureDefinitions.CssVariables.BorderBottomWidth] = border.BorderBottom.Width;
                cssVariables[FeatureDefinitions.CssVariables.BorderBottomStyle] = border.BorderBottom.Style.ToString().ToLowerInvariant();
                cssVariables[FeatureDefinitions.CssVariables.BorderBottomColor] = border.BorderBottom.Color.ToString(ColorOutputFormats.Rgba);
            }

            if (border.BorderLeft != null)
            {
                cssVariables[FeatureDefinitions.CssVariables.BorderLeftWidth] = border.BorderLeft.Width;
                cssVariables[FeatureDefinitions.CssVariables.BorderLeftStyle] = border.BorderLeft.Style.ToString().ToLowerInvariant();
                cssVariables[FeatureDefinitions.CssVariables.BorderLeftColor] = border.BorderLeft.Color.ToString(ColorOutputFormats.Rgba);
            }
        }
    }

    private void BuildTransitions(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasTransitions transitions && transitions.Transitions?.HasTransitions == true)
        {
            foreach (KeyValuePair<string, string> kv in transitions.Transitions.GetCssVariables())
                cssVariables[kv.Key] = kv.Value;

            ComputedAttributes[FeatureDefinitions.DataAttributes.Transitions] = transitions.Transitions.GetDataAttributeValue();
        }
    }

    private void BuildFamilyAttributes(ComponentBase component)
    {
        // Input family - applies shared input styles
        if (component is IInputFamilyComponent)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.InputBase] = "true";
        }
    }

    private void BuildInlineStyles(Dictionary<string, string> cssVariables)
    {
        string cssVars = string.Join("; ", cssVariables.Select(kv => $"{kv.Key}: {kv.Value}"));
        string computedStyles = string.IsNullOrWhiteSpace(_originalUserStyles)
            ? cssVars
            : string.IsNullOrWhiteSpace(cssVars)
                ? _originalUserStyles
                : $"{cssVars}; {_originalUserStyles}";

        if (!string.IsNullOrWhiteSpace(computedStyles))
            ComputedAttributes["style"] = computedStyles;
        else
            ComputedAttributes.Remove("style");
    }

    private static string ToKebabCaseComponentName(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // Clean generic notation (e.g., InputCheckbox`1 -> InputCheckbox)
        int tickIndex = value.IndexOf('`');
        if (tickIndex != -1)
        {
            value = value[..tickIndex];
        }

        // Clean "BUI" prefix
        if (value.StartsWith("BUI", StringComparison.InvariantCultureIgnoreCase))
        {
            value = value[3..];
        }

        // Convert to kebab-case
        StringBuilder sb = new();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            // Only add hyphen if uppercase, not the first letter, and previous char is not already a hyphen
            if (char.IsUpper(c) && i > 0 && value[i - 1] != '-')
            {
                sb.Append('-');
            }
            sb.Append(char.ToLower(c));
        }

        return sb.ToString();
    }
}