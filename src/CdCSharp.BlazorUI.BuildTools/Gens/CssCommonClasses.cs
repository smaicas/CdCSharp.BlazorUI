using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CdCSharp.BlazorUI.BuildTools.Gens;

[ExcludeFromCodeCoverage]
public static class CssCommonClasses
{
    public static string GetCss()
    {
        string css = $@"
/* ========================================
   Common Component Classes
   Auto-generated - Do not edit manually
   ======================================== */

/* Size Classes */
.ui-size-small {{
    font-size: 0.875rem;
}}

.ui-size-medium {{
    font-size: 1rem;
}}

.ui-size-large {{
    font-size: 1.125rem;
}}

/* Density Classes */
.ui-density-comfortable {{
    --ui-density-spacing-multiplier: 1.5;
}}

.ui-density-standard {{
    --ui-density-spacing-multiplier: 1;
}}

.ui-density-compact {{
    --ui-density-spacing-multiplier: 0.75;
}}

/* Full Width */
.ui-full-width {{
    width: 100%;
}}

/* Loading State */
.ui-loading {{
    pointer-events: none;
    position: relative;
}}

.ui-loading::after {{
    content: '';
    position: absolute;
    inset: 0;
    background-color: rgba(255, 255, 255, 0.6);
    pointer-events: none;
}}

@media (prefers-color-scheme: dark) {{
    .ui-loading::after {{
        background-color: rgba(0, 0, 0, 0.6);
    }}
}}

/* Elevation */
{GenerateElevationClasses()}

/* Ripple Effect */
.ui-has-ripple {{
    position: relative;
    overflow: hidden;
}}

.ui-ripple {{
    position: absolute;
    border-radius: 50%;
    transform: scale(0);
    animation: ui-ripple-animation var(--ui-ripple-duration, 600ms) ease-out;
    background-color: var(--ui-ripple-color, rgba(255, 255, 255, 0.5));
    pointer-events: none;
}}

[data-theme=""dark""] .ui-ripple {{
    background-color: var(--ui-ripple-color, rgba(255, 255, 255, 0.5));
}}

[data-theme=""light""] .ui-ripple {{
    background-color: var(--ui-ripple-color, rgba(0, 0, 0, 0.5));
}}

@keyframes ui-ripple-animation {{
    to {{
        transform: scale(4);
        opacity: 0;
    }}
}}
";

        return css;
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

        string umbra = FormattableString.Invariant(
            $"0px {umbraOffset}px {umbraBlur}px rgba(0,0,0,{umbraOpacity})");

        string penumbra = FormattableString.Invariant(
            $"0px {penumbraOffset}px {penumbraBlur}px rgba(0,0,0,{penumbraOpacity})");

        string ambient = FormattableString.Invariant(
            $"0px 1px 3px rgba(0,0,0,{ambientOpacity})");

        return (umbra, penumbra, ambient);
    }
}