using CdCSharp.BlazorUI.Components;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using System.Text;

namespace CdCSharp.BlazorUI.Core.Abstractions.Components;

internal sealed class BUIComponentAttributesBuilder
{
    [Flags]
    private enum ComponentFeatures : uint
    {
        None = 0,
        Variant = 1u << 0,
        Size = 1u << 1,
        Density = 1u << 2,
        FullWidth = 1u << 3,
        Loading = 1u << 4,
        Error = 1u << 5,
        Disabled = 1u << 6,
        Active = 1u << 7,
        ReadOnly = 1u << 8,
        Required = 1u << 9,
        Prefix = 1u << 10,
        Suffix = 1u << 11,
        Shadow = 1u << 12,
        Ripple = 1u << 13,
        Color = 1u << 14,
        BackgroundColor = 1u << 15,
        Border = 1u << 16,
        Transitions = 1u << 17,
        InputFamily = 1u << 18,
        PickerFamily = 1u << 19,
        DataCollectionFamily = 1u << 20,
        BuiltComponent = 1u << 21,
    }

    private readonly record struct TypeInfo(string ComponentName, ComponentFeatures Features);

    private static readonly ConcurrentDictionary<Type, TypeInfo> _typeInfoCache = new();

    private static TypeInfo GetTypeInfo(Type type) =>
        _typeInfoCache.GetOrAdd(type, ComputeTypeInfo);

    private static TypeInfo ComputeTypeInfo(Type type)
    {
        ComponentFeatures flags = ComponentFeatures.None;

        if (typeof(IVariantComponent).IsAssignableFrom(type)) flags |= ComponentFeatures.Variant;
        if (typeof(IHasSize).IsAssignableFrom(type)) flags |= ComponentFeatures.Size;
        if (typeof(IHasDensity).IsAssignableFrom(type)) flags |= ComponentFeatures.Density;
        if (typeof(IHasFullWidth).IsAssignableFrom(type)) flags |= ComponentFeatures.FullWidth;
        if (typeof(IHasLoading).IsAssignableFrom(type)) flags |= ComponentFeatures.Loading;
        if (typeof(IHasError).IsAssignableFrom(type)) flags |= ComponentFeatures.Error;
        if (typeof(IHasDisabled).IsAssignableFrom(type)) flags |= ComponentFeatures.Disabled;
        if (typeof(IHasActive).IsAssignableFrom(type)) flags |= ComponentFeatures.Active;
        if (typeof(IHasReadOnly).IsAssignableFrom(type)) flags |= ComponentFeatures.ReadOnly;
        if (typeof(IHasRequired).IsAssignableFrom(type)) flags |= ComponentFeatures.Required;
        if (typeof(IHasPrefix).IsAssignableFrom(type)) flags |= ComponentFeatures.Prefix;
        if (typeof(IHasSuffix).IsAssignableFrom(type)) flags |= ComponentFeatures.Suffix;
        if (typeof(IHasShadow).IsAssignableFrom(type)) flags |= ComponentFeatures.Shadow;
        if (typeof(IHasRipple).IsAssignableFrom(type)) flags |= ComponentFeatures.Ripple;
        if (typeof(IHasColor).IsAssignableFrom(type)) flags |= ComponentFeatures.Color;
        if (typeof(IHasBackgroundColor).IsAssignableFrom(type)) flags |= ComponentFeatures.BackgroundColor;
        if (typeof(IHasBorder).IsAssignableFrom(type)) flags |= ComponentFeatures.Border;
        if (typeof(IHasTransitions).IsAssignableFrom(type)) flags |= ComponentFeatures.Transitions;
        if (typeof(IInputFamilyComponent).IsAssignableFrom(type)) flags |= ComponentFeatures.InputFamily;
        if (typeof(IPickerFamilyComponent).IsAssignableFrom(type)) flags |= ComponentFeatures.PickerFamily;
        if (typeof(IDataCollectionFamilyComponent).IsAssignableFrom(type)) flags |= ComponentFeatures.DataCollectionFamily;
        if (typeof(IBuiltComponent).IsAssignableFrom(type)) flags |= ComponentFeatures.BuiltComponent;

        return new TypeInfo(ToKebabCaseComponentName(type.Name), flags);
    }

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

        TypeInfo typeInfo = GetTypeInfo(component.GetType());
        ComponentFeatures flags = typeInfo.Features;

        ComputedAttributes[FeatureDefinitions.DataAttributes.Component] = typeInfo.ComponentName;

        if ((flags & ComponentFeatures.Variant) != 0)
        {
            IVariantComponent variantComponent = (IVariantComponent)component;
            ComputedAttributes[FeatureDefinitions.DataAttributes.Variant] = variantComponent.CurrentVariant.Name.ToLowerInvariant();
        }

        if ((flags & ComponentFeatures.InputFamily) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.InputBase] = "true";
        if ((flags & ComponentFeatures.PickerFamily) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.PickerBase] = "true";
        if ((flags & ComponentFeatures.DataCollectionFamily) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.DataCollectionBase] = "true";

        if ((flags & ComponentFeatures.Size) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Size] = ((IHasSize)component).Size.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Density) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Density] = ((IHasDensity)component).Density.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.FullWidth) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth] = ((IHasFullWidth)component).FullWidth.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Loading) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Loading] = ((IHasLoading)component).Loading.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Error) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Error] = ((IHasError)component).Error.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Disabled) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled] = ((IHasDisabled)component).IsDisabled.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Active) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Active] = ((IHasActive)component).IsActive.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.ReadOnly) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly] = ((IHasReadOnly)component).ReadOnly.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Required) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Required] = ((IHasRequired)component).Required.ToString().ToLowerInvariant();

        if ((flags & ComponentFeatures.Prefix) != 0)
        {
            IHasPrefix prefix = (IHasPrefix)component;
            if (prefix.PrefixColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.PrefixColor] = prefix.PrefixColor;
            if (prefix.PrefixBackgroundColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.PrefixBackgroundColor] = prefix.PrefixBackgroundColor;
        }
        if ((flags & ComponentFeatures.Suffix) != 0)
        {
            IHasSuffix suffix = (IHasSuffix)component;
            if (suffix.SuffixColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.SuffixColor] = suffix.SuffixColor;
            if (suffix.SuffixBackgroundColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.SuffixBackgroundColor] = suffix.SuffixBackgroundColor;
        }
        if ((flags & ComponentFeatures.Shadow) != 0)
        {
            IHasShadow shadow = (IHasShadow)component;
            if (shadow.Shadow != null)
            {
                ComputedAttributes[FeatureDefinitions.DataAttributes.Shadow] = "true";
                cssVariables[FeatureDefinitions.InlineVariables.Shadow] = shadow.Shadow.ToCss();
            }
        }
        if ((flags & ComponentFeatures.Ripple) != 0)
        {
            IHasRipple ripple = (IHasRipple)component;
            ComputedAttributes[FeatureDefinitions.DataAttributes.Ripple] = (!ripple.DisableRipple).ToString().ToLowerInvariant();
            if (!ripple.DisableRipple)
            {
                if (ripple.RippleColor != null)
                    cssVariables[FeatureDefinitions.InlineVariables.RippleColor] = ripple.RippleColor;
                if (ripple.RippleDurationMs.HasValue)
                    cssVariables[FeatureDefinitions.InlineVariables.RippleDuration] = $"{ripple.RippleDurationMs}ms";
            }
        }
        if ((flags & ComponentFeatures.Color) != 0)
        {
            IHasColor color = (IHasColor)component;
            if (color.Color != null)
                cssVariables[FeatureDefinitions.InlineVariables.Color] = color.Color;
        }
        if ((flags & ComponentFeatures.BackgroundColor) != 0)
        {
            IHasBackgroundColor bg = (IHasBackgroundColor)component;
            if (bg.BackgroundColor != null)
                cssVariables[FeatureDefinitions.InlineVariables.BackgroundColor] = bg.BackgroundColor;
        }
        if ((flags & ComponentFeatures.Border) != 0)
        {
            IHasBorder hasBorder = (IHasBorder)component;
            if (hasBorder.Border != null)
            {
                BorderCssValues values = hasBorder.Border.GetCssValues();
                if (values.All != null) cssVariables[FeatureDefinitions.InlineVariables.Border] = values.All;
                if (values.Top != null) cssVariables[FeatureDefinitions.InlineVariables.BorderTop] = values.Top;
                if (values.Right != null) cssVariables[FeatureDefinitions.InlineVariables.BorderRight] = values.Right;
                if (values.Bottom != null) cssVariables[FeatureDefinitions.InlineVariables.BorderBottom] = values.Bottom;
                if (values.Left != null) cssVariables[FeatureDefinitions.InlineVariables.BorderLeft] = values.Left;
                if (values.Radius != null) cssVariables[FeatureDefinitions.InlineVariables.BorderRadius] = values.Radius;
            }
        }
        if ((flags & ComponentFeatures.Transitions) != 0)
        {
            IHasTransitions transitions = (IHasTransitions)component;
            if (transitions.Transitions?.HasTransitions == true)
            {
                foreach (KeyValuePair<string, string> kv in transitions.Transitions.GetCssVariables())
                    cssVariables[kv.Key] = kv.Value;
                ComputedAttributes[FeatureDefinitions.DataAttributes.Transitions] = transitions.Transitions.GetDataAttributeValue();
            }
        }

        if ((flags & ComponentFeatures.BuiltComponent) != 0)
        {
            IBuiltComponent built = (IBuiltComponent)component;
            built.BuildComponentDataAttributes(ComputedAttributes);
            built.BuildComponentCssVariables(cssVariables);
        }

        BuildInlineStyles(cssVariables);
    }

    public void PatchVolatileAttributes(ComponentBase component)
    {
        ComponentFeatures flags = GetTypeInfo(component.GetType()).Features;

        if ((flags & ComponentFeatures.Active) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Active] = ((IHasActive)component).IsActive.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Disabled) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Disabled] = ((IHasDisabled)component).IsDisabled.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Loading) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Loading] = ((IHasLoading)component).Loading.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Error) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Error] = ((IHasError)component).Error.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.ReadOnly) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.ReadOnly] = ((IHasReadOnly)component).ReadOnly.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.Required) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.Required] = ((IHasRequired)component).Required.ToString().ToLowerInvariant();
        if ((flags & ComponentFeatures.FullWidth) != 0)
            ComputedAttributes[FeatureDefinitions.DataAttributes.FullWidth] = ((IHasFullWidth)component).FullWidth.ToString().ToLowerInvariant();
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

}