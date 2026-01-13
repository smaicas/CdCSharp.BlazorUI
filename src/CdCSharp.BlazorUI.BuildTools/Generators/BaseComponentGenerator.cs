using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

/// <summary>
/// Generates base styles for bui-component element.
/// Size system only sets the multiplier; components use it in their isolated CSS.
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class BaseComponentGenerator : IAssetGenerator
{
    public string Name => "Base Component";
    public string FileName => "_base.css";

    public async Task<string> GetContent()
    {
        StringBuilder sb = new();
        sb.AppendLine(GetBaseStyles());
        sb.AppendLine();
        sb.AppendLine(GetSizeSystem());
        sb.AppendLine();
        sb.AppendLine(GetDensitySystem());
        sb.AppendLine();
        sb.AppendLine(GetStateStyles());
        sb.AppendLine();
        sb.AppendLine(GetShadowSystem());
        sb.AppendLine();
        sb.AppendLine(GetUtilities());
        return sb.ToString();
    }

    private static string GetBaseStyles() => $$"""
/* ========================================
   Base Component Styles
   Auto-generated - Do not edit manually
   ======================================== */

{{FeatureDefinitions.Tags.Component}} {
    display: inline-flex;
    box-sizing: border-box;
    font-family: inherit;
    font-size: inherit;
    line-height: inherit;
    gap: var({{FeatureDefinitions.ComponentVariables.Density.Gap}}, 0.5rem);

    /* Inline color overrides */
    background-color: var({{FeatureDefinitions.InlineVariables.BackgroundColor}}, inherit);
    color: var({{FeatureDefinitions.InlineVariables.Color}}, inherit);

    /* =====================================
       Border system (from IHasBorder)
       ===================================== */

    border: var({{FeatureDefinitions.InlineVariables.Border}}, 0);
    border-radius: var({{FeatureDefinitions.InlineVariables.BorderRadius}}, 0);

    border-top: var(
        {{FeatureDefinitions.InlineVariables.BorderTop}},
        var({{FeatureDefinitions.InlineVariables.Border}}, 0)
    );

    border-right: var(
        {{FeatureDefinitions.InlineVariables.BorderRight}},
        var({{FeatureDefinitions.InlineVariables.Border}}, 0)
    );

    border-bottom: var(
        {{FeatureDefinitions.InlineVariables.BorderBottom}},
        var({{FeatureDefinitions.InlineVariables.Border}}, 0)
    );

    border-left: var(
        {{FeatureDefinitions.InlineVariables.BorderLeft}},
        var({{FeatureDefinitions.InlineVariables.Border}}, 0)
    );
}
""";

    private static string GetSizeSystem() => $$"""
/* ========================================
   SIZE SYSTEM
   Only sets the multiplier.
   Components use calc() with multiplier
   in their isolated CSS files.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="{{FeatureDefinitions.Values.Size.Small}}"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: 0.85;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="{{FeatureDefinitions.Values.Size.Medium}}"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: 1;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="{{FeatureDefinitions.Values.Size.Large}}"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: 1.25;
}
""";

    private static string GetDensitySystem() => $$"""
/* ========================================
   DENSITY SYSTEM
   Affects spacing between elements.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="{{FeatureDefinitions.Values.Density.Compact}}"] {
    {{FeatureDefinitions.ComponentVariables.Density.Gap}}: 0.25rem;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="{{FeatureDefinitions.Values.Density.Standard}}"] {
    {{FeatureDefinitions.ComponentVariables.Density.Gap}}: 0.5rem;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="{{FeatureDefinitions.Values.Density.Comfortable}}"] {
    {{FeatureDefinitions.ComponentVariables.Density.Gap}}: 0.75rem;
}
""";

    private static string GetStateStyles() => $$"""
/* ========================================
   UNIVERSAL STATES
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Disabled}}="true"] {
    opacity: var({{FeatureDefinitions.Tokens.Opacity.Disabled}});
    pointer-events: none;
    cursor: not-allowed;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Loading}}="true"] {
    pointer-events: none;
    position: relative;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.FullWidth}}="true"] {
    width: 100%;
}
""";

    private static string GetShadowSystem() => $$"""
/* ========================================
   SHADOW SYSTEM
   Activated by data-bui-shadow attribute.
   Values provided via inline CSS variables.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Shadow}}] {
    box-shadow: 
        var({{FeatureDefinitions.InlineVariables.ShadowInset}})
        var({{FeatureDefinitions.InlineVariables.ShadowOffsetX}}, 0)
        var({{FeatureDefinitions.InlineVariables.ShadowOffsetY}}, 4px)
        var({{FeatureDefinitions.InlineVariables.ShadowBlur}}, 8px)
        var({{FeatureDefinitions.InlineVariables.ShadowSpread}}, 0)
        color-mix(
            in srgb,
            var({{FeatureDefinitions.InlineVariables.ShadowColor}}, var(--palette-shadow, rgba(0,0,0,1)))
            calc(var({{FeatureDefinitions.InlineVariables.ShadowOpacity}}, 0.2) * 100%),
            transparent
        );
}
""";

    private static string GetUtilities() => $$"""
/* ========================================
   UTILITIES
   ======================================== */

{{FeatureDefinitions.Tags.Component}} .{{FeatureDefinitions.CssClasses.VisuallyHidden}} {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
}

/* ========================================
   RIPPLE EFFECT
   ======================================== */

.{{FeatureDefinitions.CssClasses.Ripple}} {
    position: absolute;
    border-radius: 50%;
    transform: scale(0);
    pointer-events: none;
    background-color: var({{FeatureDefinitions.InlineVariables.RippleColor}}, var(--palette-white));
    opacity: 0.3;
    animation: bui-ripple var({{FeatureDefinitions.InlineVariables.RippleDuration}}, 500ms) linear forwards;
}

@keyframes bui-ripple {
    to {
        transform: scale(4);
        opacity: 0;
    }
}
""";
}