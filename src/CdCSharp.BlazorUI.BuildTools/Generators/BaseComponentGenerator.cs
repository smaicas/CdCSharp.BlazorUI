using CdCSharp.BlazorUI.Components;
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

    public Task<string> GetContent()
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
        sb.AppendLine(GetFocusSystem());
        sb.AppendLine();
        sb.AppendLine(GetShadowSystem());
        sb.AppendLine();
        sb.AppendLine(GetUtilities());
        return Task.FromResult(sb.ToString());
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
    font-size: calc(1rem * var(--bui-size-multiplier, 1));
    line-height: inherit;
    gap: calc(0.5rem * var(--bui-density-multiplier, 1));
}
""";

    private static string GetDensitySystem() => $$"""
/* ========================================
   DENSITY SYSTEM
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="compact"] {
    {{FeatureDefinitions.ComponentVariables.Density.Multiplier}}: var(--bui-compact-multiplier, 0.5);
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="standard"] {
    {{FeatureDefinitions.ComponentVariables.Density.Multiplier}}: var(--bui-standard-multiplier, 1);
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="comfortable"] {
    {{FeatureDefinitions.ComponentVariables.Density.Multiplier}}: var(--bui-comfortable-multiplier, 1.5);
}
""";

    private static string GetSizeSystem() => $$"""
/* ========================================
   SIZE SYSTEM
   Uses CSS variable for consistent scaling.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="small"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: var(--bui-small-multiplier);
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="medium"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: var(--bui-medium-multiplier);
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="large"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: var(--bui-large-multiplier);
}
""";

    private static string GetStateStyles() => $$"""
/* ========================================
   STATE STYLES
   Disabled, loading, error, etc.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Disabled}}="true"] {
    opacity: var(--bui-opacity-disabled, 0.5);
    pointer-events: none;
    cursor: not-allowed;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.FullWidth}}="true"] {
    width: 100%;
}
""";

    private static string GetFocusSystem() => $$"""
/* ========================================
   FOCUS SYSTEM
   WCAG 2.4.7 Focus Visible.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}:focus-visible,
{{FeatureDefinitions.Tags.Component}} :focus-visible {
    outline: var(--bui-highlight-outline);
    outline-offset: var(--bui-highlight-outline-offset);
}
""";

    private static string GetShadowSystem() => $$"""
/* ========================================
   SHADOW SYSTEM
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Shadow}}="true"] {
    box-shadow: var({{FeatureDefinitions.InlineVariables.Shadow}});
}
""";

    private static string GetUtilities() => $$"""
/* ========================================
   UTILITY CLASSES
   ======================================== */

.bui-field__required {
    color: var(--palette-error, #d32f2f);
    margin-inline-start: 0.25em;
}

.bui-field__validation {
    font-size: 0.75rem;
    color: var(--palette-error, #d32f2f);
}

.bui-field__helper {
    font-size: 0.75rem;
    color: var(--palette-surface-contrast);
    opacity: 0.7;
}
""";
}