using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Core.Assets.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class InputFamilyGenerator : IAssetGenerator
{
    public string FileName => "_input-family.css";
    public string Name => "Input Family";

    private static string V(string variable) => $"var({variable})";
    private static string V(string variable, string fallback) => $"var({variable}, {fallback})";
    private static string Inline(string side, string fallback) =>
        V(side, V(FeatureDefinitions.InlineVariables.Border, fallback));

    public async Task<string> GetContent()
    {
        string inputBase = FeatureDefinitions.DataAttributes.InputBase;
        string variant = FeatureDefinitions.DataAttributes.Variant;
        string floated = FeatureDefinitions.DataAttributes.Floated;
        string error = FeatureDefinitions.DataAttributes.Error;
        string sizeMult = FeatureDefinitions.ComponentVariables.Size.Multiplier;

        string inlineBg = FeatureDefinitions.InlineVariables.BackgroundColor;
        string inlineColor = FeatureDefinitions.InlineVariables.Color;
        string inlineBorder = FeatureDefinitions.InlineVariables.Border;
        string inlineBorderTop = FeatureDefinitions.InlineVariables.BorderTop;
        string inlineBorderRight = FeatureDefinitions.InlineVariables.BorderRight;
        string inlineBorderBottom = FeatureDefinitions.InlineVariables.BorderBottom;
        string inlineBorderLeft = FeatureDefinitions.InlineVariables.BorderLeft;
        string inlineRadius = FeatureDefinitions.InlineVariables.BorderRadius;

        string wrapper = FeatureDefinitions.CssClasses.Input.Wrapper;
        string field = FeatureDefinitions.CssClasses.Input.Field;
        string fieldset = FeatureDefinitions.CssClasses.Input.Fieldset;
        string legend = FeatureDefinitions.CssClasses.Input.Legend;
        string label = FeatureDefinitions.CssClasses.Input.Label;
        string required = FeatureDefinitions.CssClasses.Input.Required;
        string helper = FeatureDefinitions.CssClasses.Input.HelperText;
        string validation = FeatureDefinitions.CssClasses.Input.Validation;
        string loading = FeatureDefinitions.CssClasses.Input.Loading;
        string opacityPlaceholder = FeatureDefinitions.Tokens.Opacity.Placeholder;

        return $$"""
/* ========================================
   Input Family Styles
   Auto-generated - Do not edit manually
   ======================================== */

/* === BASE === */

bui-component[{{inputBase}}] {
    flex-direction: column;
    gap: 0.25rem;
    --input-h: calc(2.5rem * {{V(sizeMult, "1")}});
    --input-px: calc(0.75rem * {{V(sizeMult, "1")}});
    --input-py: calc(0.5rem * {{V(sizeMult, "1")}});
    --input-border: var(--palette-border, currentColor);
    --input-radius: 4px;
    --input-transition: 200ms ease;
    --input-label: var(--palette-surfacecontrast);
    --input-focus: var(--palette-primary);
    --input-error: var(--palette-error);
    --input-scale: 0.85;
}

/* === WRAPPER === */

bui-component[{{inputBase}}] .{{wrapper}} {
    position: relative;
    display: flex;
    align-items: center;
    min-height: var(--input-h);
    background: {{V(inlineBg, "transparent")}};
    transition: var(--input-transition);
}

/* === FIELD === */

bui-component[{{inputBase}}] .{{field}} {
    flex: 1;
    width: 100%;
    padding: var(--input-py) var(--input-px);
    border: none;
    background: transparent;
    font: inherit;
    color: {{V(inlineColor, "inherit")}};
    outline: none;
}

bui-component[{{inputBase}}] .{{field}}::placeholder {
    color: var(--input-label);
    opacity: 0;
    transition: opacity 200ms;
}

bui-component[{{inputBase}}][{{floated}}="true"] .{{field}}::placeholder {
    opacity: {{V(opacityPlaceholder)}};
}

/* === LABEL === */

bui-component[{{inputBase}}] .{{label}} {
    position: absolute;
    left: var(--input-px);
    top: 50%;
    transform: translateY(-50%);
    color: var(--input-label);
    pointer-events: none;
    transition: var(--input-transition);
    transform-origin: top left;
    max-width: calc(100% - 2 * var(--input-px));
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

bui-component[{{inputBase}}] .{{required}} {
    color: var(--input-error);
    margin-left: 0.125em;
}

/* === HELPER & VALIDATION === */

bui-component[{{inputBase}}] .{{helper}} {
    font-size: 0.875em;
    color: var(--input-label);
    opacity: 0.7;
    padding-left: var(--input-px);
}

bui-component[{{inputBase}}] .{{validation}} {
    font-size: 0.875em;
    color: var(--input-error);
    padding-left: var(--input-px);
}

/* === LOADING === */

bui-component[{{inputBase}}] .{{loading}} {
    position: absolute;
    right: var(--input-px);
    top: 50%;
    transform: translateY(-50%);
}

/* ========================================
   VARIANT: OUTLINED
   Border → fieldset | Background → wrapper
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{wrapper}} {
    background: transparent;
}

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{fieldset}} {
    position: absolute;
    inset: -0.35em 0 0 0;
    margin: 0;
    padding: 0 0.5em;
    background: {{V(inlineBg, "var(--palette-background)")}};
    border-top: {{Inline(inlineBorderTop, "1px solid var(--input-border)")}};
    border-right: {{Inline(inlineBorderRight, "1px solid var(--input-border)")}};
    border-bottom: {{Inline(inlineBorderBottom, "1px solid var(--input-border)")}};
    border-left: {{Inline(inlineBorderLeft, "1px solid var(--input-border)")}};
    border-radius: {{V(inlineRadius, "var(--input-radius)")}};
    overflow: hidden;
    pointer-events: none;
    transition: var(--input-transition);
}

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{legend}} {
    padding: 0;
    font-size: 1em;
    line-height: 1;
    visibility: hidden;
    max-width: 0.01px;
    transition: max-width 50ms cubic-bezier(0.4, 0, 0.2, 1);
    white-space: nowrap;
}

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{legend}} span {
    padding: 0 calc(0.35em * var(--input-scale));
    display: inline-block;
    font-size: calc(1em * var(--input-scale));
}

/* Outlined: floated */
bui-component[{{inputBase}}][{{variant}}="outlined"][{{floated}}="true"] .{{label}} {
    top: 0;
    left: calc(0.45em / {{V(sizeMult, "1")}});
    padding: 0 calc(0.35em * {{V(sizeMult, "1")}});
    transform: translateY(-50%) scale(var(--input-scale));
}

bui-component[{{inputBase}}][{{variant}}="outlined"][{{floated}}="true"] .{{legend}} {
    max-width: 100%;
    transition: max-width 100ms cubic-bezier(0.4, 0, 0.2, 1) 50ms;
}

/* Outlined: hover */
bui-component[{{inputBase}}][{{variant}}="outlined"] .{{wrapper}}:hover:not(:has(:disabled)) .{{fieldset}} {
    border-color: {{V(inlineBorder, "var(--palette-surfacecontrast)")}};
}

/* Outlined: focus */
bui-component[{{inputBase}}][{{variant}}="outlined"] .{{wrapper}}:focus-within .{{fieldset}} {
    border: {{V(inlineBorder, "2px solid var(--input-focus)")}};
}

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{wrapper}}:focus-within .{{label}} {
    color: var(--input-focus);
}

/* Outlined: error */
bui-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{fieldset}},
bui-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{wrapper}}:focus-within .{{fieldset}} {
    border: {{V(inlineBorder, "2px solid var(--input-error)")}};
}

bui-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{label}} {
    color: var(--input-error);
}

/* ========================================
   VARIANT: FILLED
   Border & Background → wrapper
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="filled"] .{{wrapper}} {
    background: {{V(inlineBg, "var(--palette-surface)")}};
    border-top: {{Inline(inlineBorderTop, "none")}};
    border-right: {{Inline(inlineBorderRight, "none")}};
    border-bottom: {{Inline(inlineBorderBottom, "2px solid var(--input-border)")}};
    border-left: {{Inline(inlineBorderLeft, "none")}};
    border-radius: {{V(inlineRadius, "4px 4px 0 0")}};
}

bui-component[{{inputBase}}][{{variant}}="filled"] .{{field}} {
    padding-top: calc(var(--input-py) + 0.75em * {{V(sizeMult, "1")}});
}

bui-component[{{inputBase}}][{{variant}}="filled"] .{{fieldset}} {
    display: none;
}

/* Filled: floated */
bui-component[{{inputBase}}][{{variant}}="filled"][{{floated}}="true"] .{{label}} {
    top: 0;
    transform: translateY(0) scale(var(--input-scale));
}

/* Filled: hover */
bui-component[{{inputBase}}][{{variant}}="filled"] .{{wrapper}}:hover:not(:has(:disabled)) {
    filter: brightness(1.1);
}

/* Filled: focus */
bui-component[{{inputBase}}][{{variant}}="filled"] .{{wrapper}}:focus-within {
    border-bottom-color: {{V(inlineBorder, "var(--input-focus)")}};
}

bui-component[{{inputBase}}][{{variant}}="filled"] .{{wrapper}}:focus-within .{{label}} {
    color: var(--input-focus);
}

/* Filled: error */
bui-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{wrapper}},
bui-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{wrapper}}:focus-within {
    border-bottom-color: {{V(inlineBorder, "var(--input-error)")}};
}

bui-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{label}} {
    color: var(--input-error);
}

/* ========================================
   VARIANT: STANDARD
   Border → wrapper (bottom only)
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="standard"] .{{wrapper}} {
    border-top: {{Inline(inlineBorderTop, "none")}};
    border-right: {{Inline(inlineBorderRight, "none")}};
    border-bottom: {{Inline(inlineBorderBottom, "1px solid var(--input-border)")}};
    border-left: {{Inline(inlineBorderLeft, "none")}};
    border-radius: {{V(inlineRadius, "0")}};
    min-height: calc(var(--input-h) - 0.5em);
}

bui-component[{{inputBase}}][{{variant}}="standard"] .{{field}} {
    padding-left: 0;
    padding-right: 0;
    padding-top: calc(var(--input-py) + 0.75em * {{V(sizeMult, "1")}});
}

bui-component[{{inputBase}}][{{variant}}="standard"] .{{label}} {
    left: 0;
}

bui-component[{{inputBase}}][{{variant}}="standard"] .{{fieldset}} {
    display: none;
}

/* Standard: floated */
bui-component[{{inputBase}}][{{variant}}="standard"][{{floated}}="true"] .{{label}} {
    top: 0;
    transform: translateY(0) scale(var(--input-scale));
}

/* Standard: hover */
bui-component[{{inputBase}}][{{variant}}="standard"] .{{wrapper}}:hover:not(:has(:disabled)) {
    border-bottom-color: {{V(inlineBorder, "var(--palette-surfacecontrast)")}};
}

/* Standard: focus */
bui-component[{{inputBase}}][{{variant}}="standard"] .{{wrapper}}:focus-within {
    border-bottom: {{Inline(inlineBorderBottom, "2px solid var(--input-focus)")}};
}

bui-component[{{inputBase}}][{{variant}}="standard"] .{{wrapper}}:focus-within .{{label}} {
    color: var(--input-focus);
}

/* Standard: error */
bui-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{wrapper}},
bui-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{wrapper}}:focus-within {
    border-bottom-color: {{V(inlineBorder, "var(--input-error)")}};
}

bui-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{label}} {
    color: var(--input-error);
}

/* Standard: helper/validation sin padding */
bui-component[{{inputBase}}][{{variant}}="standard"] .{{helper}},
bui-component[{{inputBase}}][{{variant}}="standard"] .{{validation}} {
    padding-left: 0;
}
""";
    }
}