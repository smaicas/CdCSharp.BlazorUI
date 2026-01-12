using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ComponentsCssGenerator : IAssetGenerator
{
    public string Name => "Components CSS";
    public string FileName => "common-classes.css";

    public async Task<string> GetContent()
    {
        StringBuilder sb = new();

        sb.AppendLine("""
/* ========================================
   Common Component Classes
   Auto-generated - Do not edit manually
   ======================================== */

/* === BASE COMPONENT === */
bui-component {
    display: inline-flex;
    box-sizing: border-box;
    
    /* Base padding for density calculations */
    --bui-padding-base: 0.5rem;
    --bui-calculated-padding: calc(var(--bui-padding-y, --bui-padding-base) * var(--bui-density-spacing-multiplier, 1)) 
             calc(var(--bui-padding-x, --bui-padding-base) * var(--bui-density-spacing-multiplier, 1));

    /* Color variables with fallback */
    background-color: var(--bui-background-color, inherit);
    color: var(--bui-color, inherit);

    /* Border system */
    border-width: var(--bui-border-width, 0);
    border-style: var(--bui-border-style, solid);
    border-color: var(--bui-border-color, transparent);
    border-radius: var(--bui-border-radius, 0);
    
    border-top-width: var(--bui-border-top-width, var(--bui-border-width, 0));
    border-top-style: var(--bui-border-top-style, var(--bui-border-style, solid));
    border-top-color: var(--bui-border-top-color, var(--bui-border-color, transparent));
    
    border-right-width: var(--bui-border-right-width, var(--bui-border-width, 0));
    border-right-style: var(--bui-border-right-style, var(--bui-border-style, solid));
    border-right-color: var(--bui-border-right-color, var(--bui-border-color, transparent));
    
    border-bottom-width: var(--bui-border-bottom-width, var(--bui-border-width, 0));
    border-bottom-style: var(--bui-border-bottom-style, var(--bui-border-style, solid));
    border-bottom-color: var(--bui-border-bottom-color, var(--bui-border-color, transparent));
    
    border-left-width: var(--bui-border-left-width, var(--bui-border-width, 0));
    border-left-style: var(--bui-border-left-style, var(--bui-border-style, solid));
    border-left-color: var(--bui-border-left-color, var(--bui-border-color, transparent));
}

/* === CONTAINERS === */

bui-component .bui-stack-row{
    display: inline-flex;
    flex-direction: row;
    align-items: center;
    gap: calc(0.75rem * var(--bui-density-spacing-multiplier, 1));
    vertical-align: middle;
    padding: var(--bui-calculated-padding, 0);
}

bui-component .bui-stack-column{
    display: inline-flex;
    flex-direction: column;
    align-items: center;
    gap: calc(0.75rem * var(--bui-density-spacing-multiplier, 1));
    vertical-align: middle;
    padding: var(--bui-calculated-padding, 0);
}

/* === SIZE SYSTEM === */
bui-component[data-bui-size="small"] {
    --bui-size-scale: 0.875;
    --bui-padding-scale: 0.75;
    font-size: 0.875rem;
}

bui-component[data-bui-size="medium"] {
    --bui-size-scale: 1;
    --bui-padding-scale: 1;
    font-size: 1rem;
}

bui-component[data-bui-size="large"] {
    --bui-size-scale: 1.125;
    --bui-padding-scale: 1.25;
    font-size: 1.125rem;
}

/* === DENSITY SYSTEM === */
bui-component[data-bui-density="compact"] {
    --bui-density-spacing-multiplier: 0.5;
}

bui-component[data-bui-density="standard"] {
    --bui-density-spacing-multiplier: 1;
}

bui-component[data-bui-density="comfortable"] {
    --bui-density-spacing-multiplier: 1.5;
}

/* === UNIVERSAL STATES === */

/* Disabled */
bui-component[data-bui-disabled="true"] {
    cursor: not-allowed;
    opacity: var(--bui-disabled-opacity, 0.6);
    pointer-events: none;
}

/* Loading */
bui-component[data-bui-loading="true"] {
    pointer-events: none;
    position: relative;
}

bui-component[data-bui-loading="true"]::after {
    content: '';
    position: absolute;
    inset: 0;
    background-color: inherit;
    opacity: 0.3;
    pointer-events: none;
}

/* Full Width */
bui-component[data-bui-fullwidth="true"] {
    width: 100%;
}

/* Hidden */

bui-component .hidden {
    position: absolute;
    opacity: 0;
    width: 0;
    height: 0;
    pointer-events: none;
}

/* === GENERIC SVG STYLES === */
bui-component svg {
    fill: currentColor;
}

bui-component[data-bui-size="small"] svg:not([data-bui-component="svg-icon"] svg) {
    width: 1rem;
    height: 1rem;
}

bui-component[data-bui-size="medium"] svg:not([data-bui-component="svg-icon"] svg) {
    width: 1.25rem;
    height: 1.25rem;
}

bui-component[data-bui-size="large"] svg:not([data-bui-component="svg-icon"] svg) {
    width: 1.5rem;
    height: 1.5rem;
}

/* === RIPPLE EFFECT === */
bui-component[data-bui-ripple="true"] {
    position: relative;
    overflow: hidden;
}

bui-component[data-bui-ripple="true"] .bui-ripple {
    background-color: var(--bui-ripple-color, rgba(255, 255, 255, 0.5));
}

[data-theme="light"] bui-component[data-bui-ripple="true"] .bui-ripple {
    background-color: var(--bui-ripple-color, rgba(0, 0, 0, 0.5));
}

@keyframes bui-ripple-animation {
    to {
        transform: scale(4);
        opacity: 0;
    }
}

/* === COMMON BEM CLASSES FOR INPUTS === */
.bui-input__label {
    cursor: pointer;
    display: block;
    font-weight: 500;
    color: inherit;
}

.bui-input__required {
    color: var(--palette-error);
    margin-left: 0.125rem;
}

.bui-input__container {
    position: relative;
    display: flex;
    align-items: center;
}

.bui-input__helper {
    color: inherit;
    opacity: 0.7;
    font-size: 0.75rem;
    margin-top: 0.25rem;
}

.bui-input__validation {
    color: var(--palette-error);
    font-size: 0.75rem;
    margin-top: 0.25rem;
}
""");

        sb.AppendLine();
        sb.Append(GenerateElevationClasses());

        return sb.ToString();
    }

    private static string GenerateElevationClasses()
    {
        StringBuilder sb = new();
        sb.AppendLine("/* === ELEVATION SYSTEM === */");

        for (int i = 0; i <= 24; i++)
        {
            if (i == 0)
            {
                sb.AppendLine($"bui-component[data-bui-elevation=\"{i}\"] {{");
                sb.AppendLine("    box-shadow: none;");
                sb.AppendLine("}");
            }
            else
            {
                (string umbra, string penumbra, string ambient) = GetElevationValues(i);
                sb.AppendLine($"bui-component[data-bui-elevation=\"{i}\"] {{");
                sb.AppendLine($"    box-shadow: {umbra}, {penumbra}, {ambient};");
                sb.AppendLine("}");
            }

            if (i < 24) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static (string umbra, string penumbra, string ambient) GetElevationValues(int elevation)
    {
        if (elevation == 0) return ("none", "", "");

        string shadowColor = "var(--bui-elevation-shadow-color, rgba(0, 0, 0, 1))";

        double umbraOffset = Math.Round(elevation * 0.5, 1);
        double umbraBlur = elevation;
        double penumbraOffset = elevation;
        double penumbraBlur = elevation * 2;

        string umbra = FormattableString.Invariant(
            $"0px {umbraOffset}px {umbraBlur}px color-mix(in srgb, {shadowColor} 20%, transparent)");

        string penumbra = FormattableString.Invariant(
            $"0px {penumbraOffset}px {penumbraBlur}px color-mix(in srgb, {shadowColor} 14%, transparent)");

        string ambient = FormattableString.Invariant(
            $"0px 1px 3px color-mix(in srgb, {shadowColor} 12%, transparent)");

        return (umbra, penumbra, ambient);
    }
}