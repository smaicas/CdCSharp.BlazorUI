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
""";

    private static string GetDensitySystem() => $$"""
/* ========================================
   DENSITY SYSTEM
   Affects spacing between elements.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="compact"] {
    {{FeatureDefinitions.ComponentVariables.Density.Gap}}: 0.25rem;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="standard"] {
    {{FeatureDefinitions.ComponentVariables.Density.Gap}}: 0.5rem;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Density}}="comfortable"] {
    {{FeatureDefinitions.ComponentVariables.Density.Gap}}: 0.75rem;
}
""";

    private static string GetSizeSystem() => $$"""
/* ========================================
   SIZE SYSTEM
   Uses CSS variable for consistent scaling.
   ======================================== */

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="small"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: 0.75;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="medium"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: 1;
}

{{FeatureDefinitions.Tags.Component}}[{{FeatureDefinitions.DataAttributes.Size}}="large"] {
    {{FeatureDefinitions.ComponentVariables.Size.Multiplier}}: 1.25;
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
    color: var(--palette-surfacecontrast);
    opacity: 0.7;
}
""";
}