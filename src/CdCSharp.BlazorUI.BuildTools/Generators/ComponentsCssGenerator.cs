using CdCSharp.BlazorUI.Core.Css;
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

        sb.AppendLine(@$"
/* ========================================
   Common Component Classes
   Auto-generated - Do not edit manually
   ======================================== */

{FeatureDefinitions.Tags.Component} {{
    display: inline-flex;
    align-items: center;
    gap: calc(0.75rem * var(--bui-density-spacing-multiplier, 1));
    vertical-align: middle;
    --bui-padding-base: 0.5rem; 
    
    /* Calculated padding to use by specific selectors */
    --bui-calculated-padding: calc(var(--bui-padding-base) * var(--bui-density-spacing-multiplier, 1));

    /* Color variables with fallback to palette */");

        sb.AppendLine($"    background-color: var({FeatureDefinitions.CssVariables.BackgroundColor}, inherit);");
        sb.AppendLine($"    color: var({FeatureDefinitions.CssVariables.Color}, inherit);");
        sb.AppendLine(@"}");
        sb.AppendLine();

        sb.AppendLine(@$"
/* Component-specific BEM classes */
.{FeatureDefinitions.CssClasses.InputLabel} {{
    display: block;
    margin-bottom: 0.25rem;
    font-weight: 500;
    color: inherit;
}}

.{FeatureDefinitions.CssClasses.InputRequired} {{
    color: var(--palette-error);
    margin-left: 0.125rem;
}}

.{FeatureDefinitions.CssClasses.InputContainer} {{
    position: relative;
    display: flex;
    align-items: center;
}}

.{FeatureDefinitions.CssClasses.InputLoading} {{
    position: absolute;
    right: 8px;
    top: 50%;
    transform: translateY(-50%);
}}

.{FeatureDefinitions.CssClasses.InputValidation} {{
    color: var(--palette-error);
    font-size: 0.75rem;
    margin-top: 0.25rem;
}}

.{FeatureDefinitions.CssClasses.InputHelperText} {{
    color: inherit;
    opacity: 0.7;
    font-size: 0.75rem;
    margin-top: 0.25rem;
}}
");

        sb.AppendLine();
        sb.AppendLine("/* Size Classes */");
        sb.AppendLine($"{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Size}=\"{FeatureDefinitions.SizeValues.Small}\"] {{");
        sb.AppendLine("    font-size: 0.875rem;");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine($"{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Size}=\"{FeatureDefinitions.SizeValues.Medium}\"] {{");
        sb.AppendLine("    font-size: 1rem;");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine($"{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Size}=\"{FeatureDefinitions.SizeValues.Large}\"] {{");
        sb.AppendLine("    font-size: 1.125rem;");
        sb.AppendLine("}");

        sb.AppendLine(@"
/* Density Classes */");

        sb.AppendLine($"{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Density}=\"{FeatureDefinitions.DensityValues.Comfortable}\"] {{");
        sb.AppendLine($"    {FeatureDefinitions.CssVariables.DensitySpacingMultiplier}: 1.5;");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine($"{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Density}=\"{FeatureDefinitions.DensityValues.Standard}\"] {{");
        sb.AppendLine($"    {FeatureDefinitions.CssVariables.DensitySpacingMultiplier}: 1;");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine($"{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Density}=\"{FeatureDefinitions.DensityValues.Compact}\"] {{");
        sb.AppendLine($"    {FeatureDefinitions.CssVariables.DensitySpacingMultiplier}: 0.5;");
        sb.AppendLine("}");

        sb.AppendLine($@"
/* Full Width */
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.FullWidth}=""true""] {{
    width: 100%;
}}

/* Loading State */
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Loading}=""true""] {{
    pointer-events: none;
    position: relative;
}}

{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Loading}=""true""]::after {{
    content: '';
    position: absolute;
    inset: 0;
    background-color: rgba(255, 255, 255, 0.6);
    pointer-events: none;
}}

@media (prefers-color-scheme: dark) {{
    {FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Loading}=""true""]::after {{
        background-color: rgba(0, 0, 0, 0.6);
    }}
}}

/* Elevation */");

        sb.Append(GenerateElevationClasses());

        sb.AppendLine($@"
/* Ripple Effect */
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Ripple}=""true""] {{
    position: relative;
    overflow: hidden;
}}

{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Ripple}=""true""] .{FeatureDefinitions.CssClasses.Ripple} {{
    position: absolute;
    border-radius: 50%;
    transform: scale(0);
    animation: bui-ripple-animation var({FeatureDefinitions.CssVariables.RippleDuration}, 600ms) ease-out;
    background-color: var({FeatureDefinitions.CssVariables.RippleColor}, rgba(255, 255, 255, 0.5));
    pointer-events: none;
}}

[data-theme=""dark""] {FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Ripple}=""true""] .{FeatureDefinitions.CssClasses.Ripple} {{
    background-color: var({FeatureDefinitions.CssVariables.RippleColor}, rgba(255, 255, 255, 0.5));
}}

[data-theme=""light""] {FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Ripple}=""true""] .{FeatureDefinitions.CssClasses.Ripple} {{
    background-color: var({FeatureDefinitions.CssVariables.RippleColor}, rgba(0, 0, 0, 0.5));
}}

/* Border styles using CSS variables */
{FeatureDefinitions.Tags.Component} {{
    border-width: var({FeatureDefinitions.CssVariables.BorderWidth}, 0);
    border-style: var({FeatureDefinitions.CssVariables.BorderStyle}, solid);
    border-color: var({FeatureDefinitions.CssVariables.BorderColor}, transparent);
    border-radius: var({FeatureDefinitions.CssVariables.BorderRadius}, 0);
    
    /* Individual borders override */
    border-top-width: var({FeatureDefinitions.CssVariables.BorderTopWidth}, var({FeatureDefinitions.CssVariables.BorderWidth}, 0));
    border-top-style: var({FeatureDefinitions.CssVariables.BorderTopStyle}, var({FeatureDefinitions.CssVariables.BorderStyle}, solid));
    border-top-color: var({FeatureDefinitions.CssVariables.BorderTopColor}, var({FeatureDefinitions.CssVariables.BorderColor}, transparent));
    
    border-right-width: var({FeatureDefinitions.CssVariables.BorderRightWidth}, var({FeatureDefinitions.CssVariables.BorderWidth}, 0));
    border-right-style: var({FeatureDefinitions.CssVariables.BorderRightStyle}, var({FeatureDefinitions.CssVariables.BorderStyle}, solid));
    border-right-color: var({FeatureDefinitions.CssVariables.BorderRightColor}, var({FeatureDefinitions.CssVariables.BorderColor}, transparent));
    
    border-bottom-width: var({FeatureDefinitions.CssVariables.BorderBottomWidth}, var({FeatureDefinitions.CssVariables.BorderWidth}, 0));
    border-bottom-style: var({FeatureDefinitions.CssVariables.BorderBottomStyle}, var({FeatureDefinitions.CssVariables.BorderStyle}, solid));
    border-bottom-color: var({FeatureDefinitions.CssVariables.BorderBottomColor}, var({FeatureDefinitions.CssVariables.BorderColor}, transparent));
    
    border-left-width: var({FeatureDefinitions.CssVariables.BorderLeftWidth}, var({FeatureDefinitions.CssVariables.BorderWidth}, 0));
    border-left-style: var({FeatureDefinitions.CssVariables.BorderLeftStyle}, var({FeatureDefinitions.CssVariables.BorderStyle}, solid));
    border-left-color: var({FeatureDefinitions.CssVariables.BorderLeftColor}, var({FeatureDefinitions.CssVariables.BorderColor}, transparent));
}}

/* Generic SVG styles */
{FeatureDefinitions.Tags.Component} svg {{
    fill: currentColor;
}}

/* SVG sizing based on component size */
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Size}=""{FeatureDefinitions.SizeValues.Small}""] svg:not([{FeatureDefinitions.DataAttributes.Component}=""svg-icon""] svg) {{
    width: 1rem;
    height: 1rem;
}}

{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Size}=""{FeatureDefinitions.SizeValues.Medium}""] svg:not([{FeatureDefinitions.DataAttributes.Component}=""svg-icon""] svg) {{
    width: 1.25rem;
    height: 1.25rem;
}}

{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Size}=""{FeatureDefinitions.SizeValues.Large}""] svg:not([{FeatureDefinitions.DataAttributes.Component}=""svg-icon""] svg) {{
    width: 1.5rem;
    height: 1.5rem;
}}

/* Generic disabled states for form controls */
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Disabled}=""true""] input,
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Disabled}=""true""] textarea,
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Disabled}=""true""] select {{
    cursor: not-allowed;
    opacity: 0.6;
}}

{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.ReadOnly}=""true""] input,
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.ReadOnly}=""true""] textarea,
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.ReadOnly}=""true""] select {{
    cursor: default;
}}

{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Error}=""true""] input,
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Error}=""true""] textarea,
{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Error}=""true""] select {{
    border-color: var(--palette-error);
}}

@keyframes bui-ripple-animation {{
    to {{
        transform: scale(4);
        opacity: 0;
    }}
}}
");

        return sb.ToString();
    }

    private static string GenerateElevationClasses()
    {
        StringBuilder sb = new();

        for (int i = 0; i <= 24; i++)
        {
            if (i == 0)
            {
                sb.AppendLine($"{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Elevation}=\"{i}\"] {{");
                sb.AppendLine("    box-shadow: none;");
                sb.AppendLine("}");
            }
            else
            {
                (string umbra, string penumbra, string ambient) = GetElevationValues(i);
                sb.AppendLine($"{FeatureDefinitions.Tags.Component}[{FeatureDefinitions.DataAttributes.Elevation}=\"{i}\"] {{");
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
