using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

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
        sb.AppendLine(GetElevationSystem());
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
    gap: var({{FeatureDefinitions.DensityVariables.Gap}}, var({{FeatureDefinitions.Tokens.Spacing.Space2}}));

    /* Inline color overrides */
    background-color: var({{FeatureDefinitions.InlineVariables.BackgroundColor}}, inherit);
    color: var({{FeatureDefinitions.InlineVariables.Color}}, inherit);

    /* =====================================
       Border system (shorthand)
       ===================================== */

    border: var({{FeatureDefinitions.InlineVariables.Border}}, 0);
    border-radius: var({{FeatureDefinitions.InlineVariables.BorderRadius}}, 0);

    /* Individual border sides override shorthand */
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
/* === SIZE SYSTEM === */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="{{FeatureDefinitions.SizeValues.Small}}"] {
    {{FeatureDefinitions.SizeVariables.Font}}: var({{FeatureDefinitions.Tokens.Typography.FontSizeSm}});
    {{FeatureDefinitions.SizeVariables.Icon}}: 1rem;
    {{FeatureDefinitions.SizeVariables.Height}}: 2rem;
    {{FeatureDefinitions.SizeVariables.PaddingX}}: var({{FeatureDefinitions.Tokens.Spacing.Space2}});
    {{FeatureDefinitions.SizeVariables.PaddingY}}: var({{FeatureDefinitions.Tokens.Spacing.Space1}});
    font-size: var({{FeatureDefinitions.SizeVariables.Font}});
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="{{FeatureDefinitions.SizeValues.Medium}}"] {
    {{FeatureDefinitions.SizeVariables.Font}}: var({{FeatureDefinitions.Tokens.Typography.FontSizeMd}});
    {{FeatureDefinitions.SizeVariables.Icon}}: 1.25rem;
    {{FeatureDefinitions.SizeVariables.Height}}: 2.5rem;
    {{FeatureDefinitions.SizeVariables.PaddingX}}: var({{FeatureDefinitions.Tokens.Spacing.Space3}});
    {{FeatureDefinitions.SizeVariables.PaddingY}}: var({{FeatureDefinitions.Tokens.Spacing.Space2}});
    font-size: var({{FeatureDefinitions.SizeVariables.Font}});
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="{{FeatureDefinitions.SizeValues.Large}}"] {
    {{FeatureDefinitions.SizeVariables.Font}}: var({{FeatureDefinitions.Tokens.Typography.FontSizeLg}});
    {{FeatureDefinitions.SizeVariables.Icon}}: 1.5rem;
    {{FeatureDefinitions.SizeVariables.Height}}: 3rem;
    {{FeatureDefinitions.SizeVariables.PaddingX}}: var({{FeatureDefinitions.Tokens.Spacing.Space4}});
    {{FeatureDefinitions.SizeVariables.PaddingY}}: var({{FeatureDefinitions.Tokens.Spacing.Space3}});
    font-size: var({{FeatureDefinitions.SizeVariables.Font}});
}

{{FeatureDefinitions.Tags.Component}} svg {
    width: var({{FeatureDefinitions.SizeVariables.Icon}}, 1.25rem);
    height: var({{FeatureDefinitions.SizeVariables.Icon}}, 1.25rem);
    fill: currentColor;
}
""";

    private static string GetDensitySystem() => $$"""
/* === DENSITY SYSTEM === */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="{{FeatureDefinitions.DensityValues.Compact}}"] {
    {{FeatureDefinitions.DensityVariables.Gap}}: var({{FeatureDefinitions.Tokens.Spacing.Space1}});
    {{FeatureDefinitions.DensityVariables.Padding}}: var({{FeatureDefinitions.Tokens.Spacing.Space1}});
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="{{FeatureDefinitions.DensityValues.Standard}}"] {
    {{FeatureDefinitions.DensityVariables.Gap}}: var({{FeatureDefinitions.Tokens.Spacing.Space2}});
    {{FeatureDefinitions.DensityVariables.Padding}}: var({{FeatureDefinitions.Tokens.Spacing.Space2}});
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="{{FeatureDefinitions.DensityValues.Comfortable}}"] {
    {{FeatureDefinitions.DensityVariables.Gap}}: var({{FeatureDefinitions.Tokens.Spacing.Space3}});
    {{FeatureDefinitions.DensityVariables.Padding}}: var({{FeatureDefinitions.Tokens.Spacing.Space3}});
}
""";

    private static string GetStateStyles() => $$"""
/* === UNIVERSAL STATES === */

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

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Error}}="true"] {
    --bui-state-color: var(--palette-error);
}
""";

    private static string GetElevationSystem()
    {
        StringBuilder sb = new();
        sb.AppendLine("/* === ELEVATION SYSTEM === */");
        sb.AppendLine();

        string tag = FeatureDefinitions.Tags.Component;
        string attr = FeatureDefinitions.DataAttributes.Elevation;

        for (int i = 0; i <= 24; i++)
        {
            string shadow = GetShadowForElevation(i);
            sb.AppendLine($"{tag}[{attr}=\"{i}\"] {{");
            sb.AppendLine($"    box-shadow: {shadow};");
            sb.AppendLine("}");
            if (i < 24) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string GetShadowForElevation(int elevation)
    {
        if (elevation == 0) return "none";

        string shadowColor = $"var({FeatureDefinitions.InlineVariables.ElevationShadowColor}, var(--palette-shadow))";

        double umbraY = Math.Round(elevation * 0.5, 1);
        double umbraBlur = elevation;
        double penumbraY = elevation;
        double penumbraBlur = elevation * 2;

        string umbra = FormattableString.Invariant(
            $"0 {umbraY}px {umbraBlur}px color-mix(in srgb, {shadowColor} 20%, transparent)");
        string penumbra = FormattableString.Invariant(
            $"0 {penumbraY}px {penumbraBlur}px color-mix(in srgb, {shadowColor} 14%, transparent)");
        string ambient = FormattableString.Invariant(
            $"0 1px 3px color-mix(in srgb, {shadowColor} 12%, transparent)");

        return $"{umbra}, {penumbra}, {ambient}";
    }

    private static string GetUtilities() => $$"""
/* === UTILITIES === */

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

/* === RIPPLE EFFECT === */
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