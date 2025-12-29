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

ui-component {{
    display: inline-flex;
    /* Color variables with fallback to palette */
    background-color: var(--ui-bg-color, inherit);
    color: var(--ui-color, inherit);
}}

/* Size Classes */
ui-component[data-ui-size=""small""] {{
    font-size: 0.875rem;
}}

ui-component[data-ui-size=""medium""] {{
    font-size: 1rem;
}}

ui-component[data-ui-size=""large""] {{
    font-size: 1.125rem;
}}

/* Density Classes */
ui-component[data-ui-density=""comfortable""] {{
    --ui-density-spacing-multiplier: 1.5;
}}

ui-component[data-ui-density=""standard""] {{
    --ui-density-spacing-multiplier: 1;
}}

ui-component[data-ui-density=""compact""] {{
    --ui-density-spacing-multiplier: 0.75;
}}

/* Full Width */
ui-component[data-ui-fullwidth=""true""] {{
    width: 100%;
}}

/* Loading State */
ui-component[data-ui-loading=""true""] {{
    pointer-events: none;
    position: relative;
}}

ui-component[data-ui-loading=""true""]::after {{
    content: '';
    position: absolute;
    inset: 0;
    background-color: rgba(255, 255, 255, 0.6);
    pointer-events: none;
}}

@media (prefers-color-scheme: dark) {{
    ui-component[data-ui-loading=""true""]::after {{
        background-color: rgba(0, 0, 0, 0.6);
    }}
}}

/* Elevation */
{GenerateElevationClasses()}

/* Ripple Effect */
ui-component[data-ui-ripple=""true""] {{
    position: relative;
    overflow: hidden;
}}

ui-component[data-ui-ripple=""true""] .ui-ripple {{
    position: absolute;
    border-radius: 50%;
    transform: scale(0);
    animation: ui-ripple-animation var(--ui-ripple-duration, 600ms) ease-out;
    background-color: var(--ui-ripple-color, rgba(255, 255, 255, 0.5));
    pointer-events: none;
}}

[data-theme=""dark""] ui-component[data-ui-ripple=""true""] .ui-ripple {{
    background-color: var(--ui-ripple-color, rgba(255, 255, 255, 0.5));
}}

[data-theme=""light""] ui-component[data-ui-ripple=""true""] .ui-ripple {{
    background-color: var(--ui-ripple-color, rgba(0, 0, 0, 0.5));
}}

/* Border styles using CSS variables */
ui-component {{
    border-width: var(--ui-border-width, 0);
    border-style: var(--ui-border-style, solid);
    border-color: var(--ui-border-color, transparent);
    border-radius: var(--ui-border-radius, 0);
    
    /* Individual borders override */
    border-top-width: var(--ui-border-top-width, var(--ui-border-width, 0));
    border-top-style: var(--ui-border-top-style, var(--ui-border-style, solid));
    border-top-color: var(--ui-border-top-color, var(--ui-border-color, transparent));
    
    border-right-width: var(--ui-border-right-width, var(--ui-border-width, 0));
    border-right-style: var(--ui-border-right-style, var(--ui-border-style, solid));
    border-right-color: var(--ui-border-right-color, var(--ui-border-color, transparent));
    
    border-bottom-width: var(--ui-border-bottom-width, var(--ui-border-width, 0));
    border-bottom-style: var(--ui-border-bottom-style, var(--ui-border-style, solid));
    border-bottom-color: var(--ui-border-bottom-color, var(--ui-border-color, transparent));
    
    border-left-width: var(--ui-border-left-width, var(--ui-border-width, 0));
    border-left-style: var(--ui-border-left-style, var(--ui-border-style, solid));
    border-left-color: var(--ui-border-left-color, var(--ui-border-color, transparent));
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
            if (i == 0)
            {
                sb.AppendLine($"ui-component[data-ui-elevation=\"{i}\"] {{");
                sb.AppendLine("    box-shadow: none;");
                sb.AppendLine("}");
            }
            else
            {
                (string umbra, string penumbra, string ambient) = GetElevationValues(i);
                sb.AppendLine($"ui-component[data-ui-elevation=\"{i}\"] {{");
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