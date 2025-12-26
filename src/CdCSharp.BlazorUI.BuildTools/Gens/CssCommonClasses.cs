using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Gens;

[ExcludeFromCodeCoverage]
public static class CssCommonClasses
{
    public static string GetCss()
    {
        StringBuilder sb = new();

        sb.AppendLine("/* ========================================");
        sb.AppendLine("   Common Component Classes");
        sb.AppendLine("   Auto-generated - Do not edit manually");
        sb.AppendLine("   ======================================== */");
        sb.AppendLine();

        // Size Classes
        sb.AppendLine("/* Size Classes */");
        sb.AppendLine(".ui-size-small {");
        sb.AppendLine("    font-size: 0.875rem;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".ui-size-medium {");
        sb.AppendLine("    font-size: 1rem;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".ui-size-large {");
        sb.AppendLine("    font-size: 1.125rem;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Density Classes
        sb.AppendLine("/* Density Classes */");
        sb.AppendLine(".ui-density-comfortable {");
        sb.AppendLine("    --ui-density-spacing-multiplier: 1.5;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".ui-density-standard {");
        sb.AppendLine("    --ui-density-spacing-multiplier: 1;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".ui-density-compact {");
        sb.AppendLine("    --ui-density-spacing-multiplier: 0.75;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Full Width
        sb.AppendLine("/* Full Width */");
        sb.AppendLine(".ui-full-width {");
        sb.AppendLine("    width: 100%;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Loading State
        sb.AppendLine("/* Loading State */");
        sb.AppendLine(".ui-loading {");
        sb.AppendLine("    pointer-events: none;");
        sb.AppendLine("    position: relative;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".ui-loading::after {");
        sb.AppendLine("    content: '';");
        sb.AppendLine("    position: absolute;");
        sb.AppendLine("    inset: 0;");
        sb.AppendLine("    background-color: rgba(255, 255, 255, 0.6);");
        sb.AppendLine("    pointer-events: none;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("@media (prefers-color-scheme: dark) {");
        sb.AppendLine("    .ui-loading::after {");
        sb.AppendLine("        background-color: rgba(0, 0, 0, 0.6);");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();

        // Elevation
        sb.AppendLine("/* Elevation */");
        sb.Append(GenerateElevationClasses());
        sb.AppendLine();

        // Ripple Effect
        sb.AppendLine("/* Ripple Effect */");
        sb.AppendLine(".ui-has-ripple {");
        sb.AppendLine("    position: relative;");
        sb.AppendLine("    overflow: hidden;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".ui-ripple {");
        sb.AppendLine("    position: absolute;");
        sb.AppendLine("    border-radius: 50%;");
        sb.AppendLine("    transform: scale(0);");
        sb.AppendLine("    animation: ui-ripple-animation var(--ui-ripple-duration, 600ms) ease-out;");
        sb.AppendLine("    background-color: var(--ui-ripple-color, rgba(0, 0, 0, 0.1));");
        sb.AppendLine("    pointer-events: none;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("@media (prefers-color-scheme: dark) {");
        sb.AppendLine("    .ui-ripple {");
        sb.AppendLine("        background-color: var(--ui-ripple-color, rgba(255, 255, 255, 0.1));");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("@keyframes ui-ripple-animation {");
        sb.AppendLine("    to {");
        sb.AppendLine("        transform: scale(4);");
        sb.AppendLine("        opacity: 0;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateElevationClasses()
    {
        StringBuilder sb = new();

        for (int i = 0; i <= 24; i++)
        {
            (string? umbra, string? penumbra, string? ambient) = GetElevationValues(i);

            if (i == 0)
            {
                sb.AppendLine(".ui-elevation-0 {");
                sb.AppendLine("    box-shadow: none;");
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine($".ui-elevation-{i} {{");
                sb.AppendLine($"    box-shadow: {umbra}, {penumbra}, {ambient};");
                sb.AppendLine("}");
            }

            if (i < 24)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static (string umbra, string penumbra, string ambient) GetElevationValues(int elevation)
    {
        if (elevation == 0) return ("none", "", "");

        const double umbraOpacity = 0.2;
        const double penumbraOpacity = 0.14;
        const double ambientOpacity = 0.12;

        double umbraOffset = Math.Round(elevation * 0.5, 1);
        double umbraBlur = elevation;
        double penumbraOffset = elevation;
        double penumbraBlur = elevation * 2;

        string umbra = $"0px {umbraOffset}px {umbraBlur}px rgba(0,0,0,{umbraOpacity})";
        string penumbra = $"0px {penumbraOffset}px {penumbraBlur}px rgba(0,0,0,{penumbraOpacity})";
        string ambient = $"0px 1px 3px rgba(0,0,0,{ambientOpacity})";

        return (umbra, penumbra, ambient);
    }
}