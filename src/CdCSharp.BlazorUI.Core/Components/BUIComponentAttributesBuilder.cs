using CdCSharp.BlazorUI.Components;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace CdCSharp.BlazorUI.Core.Abstractions.Components;

internal sealed class BUIComponentAttributesBuilder
{
    private string? _originalUserStyles;
    private readonly Dictionary<string, string> _cssVariables = [];
    private readonly StringBuilder _styleBuilder = new();
    public Dictionary<string, object> ComputedAttributes { get; } = [];

    public void BuildStyles(
        ComponentBase component,
        IReadOnlyDictionary<string, object>? additionalAttributes)
    {
        ComputedAttributes.Clear();
        if (additionalAttributes != null)
        {
            foreach (KeyValuePair<string, object> kv in additionalAttributes)
            {
                ComputedAttributes[kv.Key] = kv.Value;
            }
        }

        _originalUserStyles = ComputedAttributes.TryGetValue("style", out object? style)
            ? style?.ToString()
            : null;

        Dictionary<string, string> cssVariables = _cssVariables;
        cssVariables.Clear();

        string componentName = ToKebabCaseComponentName(component.GetType().Name);
        ComputedAttributes[FeatureDefinitions.DataAttributes.Component] = componentName;

        if (component is IVariantComponent variantComponent)
        {
            string dataAttr = FeatureDefinitions.DataAttributes.Variant;
            string value = variantComponent.CurrentVariant.Name.ToLowerInvariant();
            ComputedAttributes[dataAttr] = value;
        }

        BuildFamilyAttributes(component);

        BuildSize(component);
        BuildDensity(component);
        BuildFullWidth(component);
        BuildLoading(component);
        BuildError(component);
        BuildDisabled(component);
        BuildActive(component);
        BuildReadOnly(component);
        BuildRequired(component);
        BuildPrefix(component, cssVariables);
        BuildSuffix(component, cssVariables);
        BuildShadow(component, cssVariables);
        BuildRipple(component, cssVariables);
        BuildColor(component, cssVariables);
        BuildBackgroundColor(component, cssVariables);
        BuildBorder(component, cssVariables);
        BuildTransitions(component, cssVariables);

        if (component is IBuiltComponent buiComponent)
        {
            buiComponent.BuildComponentDataAttributes(ComputedAttributes);
        }

        if (component is IBuiltComponent buiComponentForVars)
        {
            buiComponentForVars.BuildComponentCssVariables(cssVariables);
        }

        BuildInlineStyles(cssVariables);
    }

    public void PatchVolatileAttributes(ComponentBase component)
    {
        if (component is IHasActive active)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Active] = active.IsActive.ToString().ToLowerInvariant();

        if (component is IHasDisabled disabled)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled] = disabled.IsDisabled.ToString().ToLowerInvariant();

        if (component is IHasLoading loading)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Loading] = loading.Loading.ToString().ToLowerInvariant();

        if (component is IHasError error)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Error] = error.Error.ToString().ToLowerInvariant();

        if (component is IHasReadOnly readOnly)
            ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly] = readOnly.ReadOnly.ToString().ToLowerInvariant();

        if (component is IHasRequired required)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Required] = required.Required.ToString().ToLowerInvariant();

        if (component is IHasFullWidth fullWidth)
            ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth] = fullWidth.FullWidth.ToString().ToLowerInvariant();
    }

    private static string ToKebabCaseComponentName(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        int tickIndex = value.IndexOf('`');
        if (tickIndex != -1)
        {
            value = value[..tickIndex];
        }

        if (value.StartsWith("BUI", StringComparison.InvariantCultureIgnoreCase))
        {
            value = value[3..];
        }

        StringBuilder sb = new();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsUpper(c) && i > 0 && value[i - 1] != '-')
            {
                sb.Append('-');
            }
            sb.Append(char.ToLower(c));
        }

        return sb.ToString();
    }

    private void BuildBorder(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasBorder hasBorder && hasBorder.Border != null)
        {
            BorderCssValues values = hasBorder.Border.GetCssValues();

            if (values.All != null)
                cssVariables[FeatureDefinitions.InlineVariables.Border] = values.All;
            if (values.Top != null)
                cssVariables[FeatureDefinitions.InlineVariables.BorderTop] = values.Top;
            if (values.Right != null)
                cssVariables[FeatureDefinitions.InlineVariables.BorderRight] = values.Right;
            if (values.Bottom != null)
                cssVariables[FeatureDefinitions.InlineVariables.BorderBottom] = values.Bottom;
            if (values.Left != null)
                cssVariables[FeatureDefinitions.InlineVariables.BorderLeft] = values.Left;
            if (values.Radius != null)
                cssVariables[FeatureDefinitions.InlineVariables.BorderRadius] = values.Radius;
        }
    }

    private void BuildPrefix(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasPrefix prefix)
        {
            if (prefix.PrefixColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.PrefixColor] = prefix.PrefixColor;
            if (prefix.PrefixBackgroundColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.PrefixBackgroundColor] = prefix.PrefixBackgroundColor;
        }
    }

    private void BuildSuffix(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasSuffix suffix)
        {
            if (suffix.SuffixColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.SuffixColor] = suffix.SuffixColor;
            if (suffix.SuffixBackgroundColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.SuffixBackgroundColor] = suffix.SuffixBackgroundColor;
        }
    }

    private void BuildColor(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasColor color && color.Color != null)
            cssVariables[FeatureDefinitions.InlineVariables.Color] = color.Color;
    }

    private void BuildBackgroundColor(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasBackgroundColor bg && bg.BackgroundColor != null)
            cssVariables[FeatureDefinitions.InlineVariables.BackgroundColor] = bg.BackgroundColor;
    }

    private void BuildDensity(ComponentBase component)
    {
        if (component is IHasDensity density)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Density] = density.Density.ToString().ToLowerInvariant();
    }

    private void BuildDisabled(ComponentBase component)
    {
        if (component is IHasDisabled disabled)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled] = disabled.IsDisabled.ToString().ToLowerInvariant();
    }

    private void BuildActive(ComponentBase component)
    {
        if (component is IHasActive active)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Active] = active.IsActive.ToString().ToLowerInvariant();
    }

    private void BuildShadow(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasShadow shadow && shadow.Shadow != null)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Shadow] = "true";
            cssVariables[FeatureDefinitions.InlineVariables.Shadow] = shadow.Shadow.ToCss();
        }
    }

    private void BuildError(ComponentBase component)
    {
        if (component is IHasError error)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Error] = error.Error.ToString().ToLowerInvariant();
    }

    private void BuildFamilyAttributes(ComponentBase component)
    {
        if (component is IInputFamilyComponent)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.InputBase] = "true";
        }

        if (component is IPickerFamilyComponent)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.PickerBase] = "true";
        }

        if (component is IDataCollectionFamilyComponent)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.DataCollectionBase] = "true";
        }
    }

    private void BuildFullWidth(ComponentBase component)
    {
        if (component is IHasFullWidth fullWidth)
            ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth] = fullWidth.FullWidth.ToString().ToLowerInvariant();
    }

    private void BuildInlineStyles(Dictionary<string, string> cssVariables)
    {
        _styleBuilder.Clear();

        bool first = true;
        foreach (KeyValuePair<string, string> kv in cssVariables)
        {
            if (!first) _styleBuilder.Append("; ");
            _styleBuilder.Append(kv.Key).Append(": ").Append(kv.Value);
            first = false;
        }

        bool hasUserStyles = !string.IsNullOrWhiteSpace(_originalUserStyles);
        if (hasUserStyles)
        {
            if (_styleBuilder.Length > 0) _styleBuilder.Append("; ");
            _styleBuilder.Append(_originalUserStyles);
        }

        if (_styleBuilder.Length > 0)
            ComputedAttributes["style"] = _styleBuilder.ToString();
        else
            ComputedAttributes.Remove("style");
    }

    private void BuildLoading(ComponentBase component)
    {
        if (component is IHasLoading loading)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Loading] = loading.Loading.ToString().ToLowerInvariant();
    }

    private void BuildReadOnly(ComponentBase component)
    {
        if (component is IHasReadOnly readOnly)
            ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly] = readOnly.ReadOnly.ToString().ToLowerInvariant();
    }

    private void BuildRequired(ComponentBase component)
    {
        if (component is IHasRequired required)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Required] = required.Required.ToString().ToLowerInvariant();
    }

    private void BuildRipple(ComponentBase component, Dictionary<string, string> cssVariables)
    {
        if (component is IHasRipple ripple)
        {
            ComputedAttributes[FeatureDefinitions.DataAttributes.Ripple] = (!ripple.DisableRipple).ToString().ToLowerInvariant();

            if (!ripple.DisableRipple)
            {
                if (ripple.RippleColor != null)
                    cssVariables[FeatureDefinitions.InlineVariables.RippleColor] = ripple.RippleColor;
                if (ripple.RippleDurationMs.HasValue)
                    cssVariables[FeatureDefinitions.InlineVariables.RippleDuration] = $"{ripple.RippleDurationMs}ms";
            }
        }
    }

    private void BuildSize(ComponentBase component)
    {
        if (component is IHasSize size)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Size] = size.Size.ToString().ToLowerInvariant();
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
}