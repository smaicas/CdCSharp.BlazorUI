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
    font-family: inherit;
    gap: var(--bui-density-gap);

    /* Color variables with fallback */
    background-color: var(--bui-inline-background-color, inherit);
    color: var(--bui-inline-color, inherit);

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

/* === SIZE SYSTEM === */
bui-component[data-bui-size="small"] {
    --bui-font-size: 0.875rem;
    --bui-font-size-small: 0.75rem;
    --bui-icon-size: 1rem;
    font-size: var(--bui-font-size);
}

bui-component[data-bui-size="medium"] {
    --bui-font-size: 1rem;
    --bui-font-size-small: 0.875rem;
    --bui-icon-size: 1.25rem;
    font-size: var(--bui-font-size);
}

bui-component[data-bui-size="large"] {
    --bui-font-size: 1.125rem;
    --bui-font-size-small: 1rem;
    --bui-icon-size: 1.5rem;
    font-size: var(--bui-font-size);
}

/* === DENSITY SYSTEM === */
bui-component[data-bui-density="compact"] {
    --bui-density-gap: 0.25rem;
}

bui-component[data-bui-density="standard"] {
    --bui-density-gap: 0.5rem;
}

bui-component[data-bui-density="comfortable"] {
    --bui-density-gap: 0.75rem;
}

/* === UNIVERSAL STATES === */

bui-component[data-bui-disabled="true"] {
    cursor: not-allowed;
    opacity: 0.6;
    pointer-events: none;
}

bui-component[data-bui-loading="true"] {
    pointer-events: none;
    position: relative;
}

bui-component[data-bui-fullwidth="true"] {
    width: 100%;
}

/* === GENERIC SVG STYLES === */
bui-component svg {
    fill: currentColor;
}

bui-component svg:not([data-bui-component="svg-icon"] svg) {
    width: var(--bui-icon-size, 1.25rem);
    height: var(--bui-icon-size, 1.25rem);
}

/* === RIPPLE EFFECT === */
.bui-ripple {
    position: absolute;
    border-radius: 50%;
    transform: scale(0);
    pointer-events: none;
    background-color: var(--bui-ripple-color, rgba(255, 255, 255, 0.5));
    animation: bui-ripple-animation 600ms linear forwards;
}

[data-theme="light"] .bui-ripple {
    background-color: var(--bui-ripple-color, rgba(0, 0, 0, 0.2));
}

@keyframes bui-ripple-animation {
    to {
        transform: scale(4);
        opacity: 0;
    }
}

/* === UTILITY CLASSES === */
bui-component .hidden {
    position: absolute;
    opacity: 0;
    width: 0;
    height: 0;
    pointer-events: none;
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