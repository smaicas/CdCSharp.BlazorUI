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
    public string FileName => "_base.css";
    public string Name => "Base Component";

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
}

/* ========================================
   GENERIC BUTTONS
   ======================================== */

.bui-action-btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 0.25rem;
    height: 1.75rem;
    padding-inline: 0.5rem;
    border: 1px solid var(--palette-border);
    border-radius: 4px;
    background: transparent;
    color: inherit;
    font: inherit;
    font-size: 0.75rem;
    cursor: pointer;
    transition: background-color 150ms ease, border-color 150ms ease;
}

.bui-action-btn:hover:not(:disabled) {
    background: color-mix(in srgb, var(--palette-surfacecontrast) 8%, transparent);
    border-color: color-mix(in srgb, var(--palette-surfacecontrast) 20%, transparent);
}

.bui-action-btn:disabled {
    cursor: not-allowed;
    opacity: 0.5;
}

.bui-action-btn:focus-visible {
    outline: 2px solid var(--palette-highlight);
    outline-offset: 2px;
}

.bui-action-btn--sm { height: 1.5rem; font-size: 0.625rem; padding-inline: 0.375rem; }
.bui-action-btn--lg { height: 2rem; font-size: 0.875rem; padding-inline: 0.75rem; }

.bui-icon-btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 2rem;
    height: 2rem;
    padding: 0;
    border: none;
    border-radius: 4px;
    background: transparent;
    color: inherit;
    cursor: pointer;
    opacity: 0.6;
    transition: opacity 150ms ease, background-color 150ms ease;
}

.bui-icon-btn:hover:not(:disabled) {
    opacity: 1;
    background: color-mix(in srgb, var(--palette-surfacecontrast) 8%, transparent);
}

.bui-icon-btn:disabled {
    cursor: not-allowed;
    opacity: 0.3;
}

.bui-icon-btn:focus-visible {
    outline: 2px solid var(--palette-highlight);
    outline-offset: 2px;
}

.bui-icon-btn--sm { width: 1.5rem; height: 1.5rem; }
.bui-icon-btn--lg { width: 2.5rem; height: 2.5rem; }

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

    private static string GetShadowSystem() => $$"""
/* ========================================
   SHADOW SYSTEM
   Activated by data-bui-shadow attribute.
   Values provided via inline CSS variables.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Shadow}}] {
    box-shadow: var({{FeatureDefinitions.InlineVariables.Shadow}});
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

    private static string GetUtilities() => $$"""
/* ========================================
   UTILITIES
   ======================================== */

{{FeatureDefinitions.Tags.Component}} .{{FeatureDefinitions.CssClasses.SrOnly}} {
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

/* ========================================
   KEYBOARD FOCUS INDICATORS (Universal)
   ======================================== */

/* Universal button focus inside bui-component */
bui-component button:focus-visible {
    outline: 2px solid var(--palette-highlight);
    outline-offset: 2px;
}

/* Close buttons (negative offset for internal buttons) */
bui-component [class*="__close"]:focus-visible {
    outline-offset: -2px;
}

/* ========================================
   ACCESSIBILITY: REDUCED MOTION
   ======================================== */

@media (prefers-reduced-motion: reduce) {
    {{FeatureDefinitions.Tags.Component}} {
        animation: none !important;
    }
}

""";
}