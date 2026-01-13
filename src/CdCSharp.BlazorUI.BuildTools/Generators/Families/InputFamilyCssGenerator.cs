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
        string inlineBorderBottom = FeatureDefinitions.InlineVariables.BorderBottom;
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

    --_input-h: calc(2.5rem * {{V(sizeMult, "1")}});
    --_input-px: calc(0.75rem * {{V(sizeMult, "1")}});
    --_input-py: calc(0.5rem * {{V(sizeMult, "1")}});
    --_input-radius: 4px;
    --_input-transition: 200ms ease;
    --_input-scale: 0.85;

    --_input-label-color: var(--palette-surfacecontrast);
    --_input-focus-color: var(--palette-primary);
    --_input-error-color: var(--palette-error);

    --_wrapper-bg: transparent;
    --_wrapper-border-block-end: none;
    --_wrapper-radius: 0;
    --_wrapper-min-h: var(--_input-h);

    --_field-pt: var(--_input-py);
    --_field-px: var(--_input-px);

    --_fieldset-display: none;
    --_fieldset-bg: transparent;

    --_label-inset-inline-start: var(--_input-px);
    --_label-floated-inset: var(--_input-px);
    --_label-outline-offset: 0em;
}

/* === WRAPPER === */

bui-component[{{inputBase}}] .{{wrapper}} {
    position: relative;
    display: flex;
    align-items: center;
    min-height: var(--_wrapper-min-h);
    background: var(--_wrapper-bg);
    border-block-end: {{V(inlineBorderBottom, "var(--_wrapper-border-block-end)")}};
    border-radius: {{V(inlineRadius, "var(--_wrapper-radius)")}};
}

/* === FIELD === */

bui-component[{{inputBase}}] .{{field}} {
    flex: 1;
    width: 100%;
    padding-block-start: var(--_field-pt);
    padding-block-end: var(--_input-py);
    padding-inline: var(--_field-px);
    border: none;
    background: transparent;
    font: inherit;
    color: {{V(inlineColor, "inherit")}};
    outline: none;
}

bui-component[{{inputBase}}] .{{field}}::placeholder {
    color: var(--_input-label-color);
    opacity: 0;
    transition: opacity 200ms;
}

bui-component[{{inputBase}}][{{floated}}="true"] .{{field}}::placeholder {
    opacity: {{V(opacityPlaceholder)}};
}

/* === LABEL === */

bui-component[{{inputBase}}] .{{label}} {
    position: absolute;
    inset-inline-start: var(--_label-inset-inline-start);
    top: 50%;
    transform: translateY(-50%);
    color: var(--_input-label-color);
    pointer-events: none;
    transition: var(--_input-transition);
    transform-origin: top left;
    max-width: calc(100% - 2 * var(--_input-px));
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

bui-component[{{inputBase}}][{{floated}}="true"] .{{label}} {
    top: 0;
    transform: translateY(0) scale(var(--_input-scale));
}

bui-component[{{inputBase}}]:focus-within .{{label}} {
    color: var(--_input-focus-color);
}

bui-component[{{inputBase}}][{{error}}="true"] .{{label}} {
    color: var(--_input-error-color);
}

bui-component[{{inputBase}}] .{{required}} {
    color: var(--_input-error-color);
    margin-inline-start: 0.125em;
}

/* === HELPER & VALIDATION === */

bui-component[{{inputBase}}] .{{helper}} {
    font-size: 0.875em;
    color: var(--_input-label-color);
    opacity: 0.7;
    padding-inline-start: var(--_input-px);
}

bui-component[{{inputBase}}] .{{validation}} {
    font-size: 0.875em;
    color: var(--_input-error-color);
    padding-inline-start: var(--_input-px);
}

/* === LOADING === */

bui-component[{{inputBase}}] .{{loading}} {
    position: absolute;
    inset-inline-end: var(--_input-px);
    top: 50%;
    transform: translateY(-50%);
}

/* === FIELDSET BASE === */

bui-component[{{inputBase}}] .{{fieldset}} {
    display: var(--_fieldset-display);
}

/* ========================================
   VARIANT: OUTLINED
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="outlined"] {
    --_fieldset-display: block;
    --_fieldset-bg: {{V(inlineBg)}};
    --_label-floated-inset: calc(0.45em / {{V(sizeMult, "1")}});
    --_label-outline-offset: calc(0.35em * {{V(sizeMult, "1")}});
}

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{fieldset}} {
    position: absolute;
    inset: -0.35em 0 0 0;
    margin: 0;
    padding-inline: 0.5em;
    background: var(--_fieldset-bg);
    border: {{V(inlineBorder, "1px solid var(--palette-border)")}};
    border-radius: {{V(inlineRadius, "var(--_input-radius)")}};
    pointer-events: none;
    transition: var(--_input-transition);
}

/* Legend mechanics (CRITICAL) */

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{legend}} {
    padding: 0;
    font-size: 1em;
    line-height: 1;
    visibility: hidden;
    max-width: 0.01px;
    white-space: nowrap;
    transition: max-width 100ms cubic-bezier(0.4, 0, 0.2, 1);
}

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{legend}} span {
    display: inline-block;
    padding-inline: calc(0.35em * var(--_input-scale));
    font-size: calc(1em * var(--_input-scale));
}

/* Outlined: floated */

bui-component[{{inputBase}}][{{variant}}="outlined"][{{floated}}="true"] .{{label}} {
    inset-inline-start: calc(
    var(--_label-floated-inset) + var(--_label-outline-offset));
    transform: translateY(-50%) scale(var(--_input-scale));
}

bui-component[{{inputBase}}][{{variant}}="outlined"][{{floated}}="true"] .{{legend}} {
    max-width: 100%;
}

/* Outlined: focus & error */

bui-component[{{inputBase}}][{{variant}}="outlined"]:focus-within .{{fieldset}} {
    border: 2px solid var(--_input-focus-color);
}

bui-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{fieldset}} {
    border: {{V(inlineBorder, "2px solid var(--_input-error-color)")}};
}

/* ========================================
   VARIANT: FILLED
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="filled"] {
    --_wrapper-bg: {{V(inlineBg, "var(--palette-surface)")}};
    --_wrapper-border-block-end: 2px solid var(--palette-border);
    --_wrapper-radius: 4px 4px 0 0;
    --_field-pt: calc(var(--_input-py) + 0.75em * {{V(sizeMult, "1")}});
}

/* Filled: focus & error */

bui-component[{{inputBase}}][{{variant}}="filled"]:focus-within .{{wrapper}} {
    border-block-end: 2px solid var(--_input-focus-color);
}

bui-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{wrapper}} {
    border-block-end-color: var(--_input-error-color);
}

/* ========================================
   VARIANT: STANDARD
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="standard"] {
    --_wrapper-border-block-end: 1px solid var(--palette-border);
    --_wrapper-min-h: calc(var(--_input-h) - 0.5em);
    --_wrapper-bg: {{V(inlineBg)}};
    --_field-pt: calc(var(--_input-py) + 0.75em * {{V(sizeMult, "1")}});
    --_field-px: 0;
    --_label-inset-inline-start: 0;
}

/* Standard: focus & error */

bui-component[{{inputBase}}][{{variant}}="standard"]:focus-within .{{wrapper}} {
    border-block-end: 2px solid var(--_input-focus-color);
}

bui-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{wrapper}} {
    border-block-end-color: var(--_input-error-color);
}

/* Standard: helper/validation sin padding */

bui-component[{{inputBase}}][{{variant}}="standard"] .{{helper}},
bui-component[{{inputBase}}][{{variant}}="standard"] .{{validation}} {
    padding-inline-start: 0;
}

/* Outline para Outlined variant - solo con teclado */
bui-component[data-bui-input-base][data-bui-variant="outlined"][data-keyboard-focus="true"] .bui-input__fieldset {
    outline: 2px solid var(--palette-highlight);
    outline-offset: 2px;
}

/* Outline para Filled y Standard variants - solo con teclado */
bui-component[data-bui-input-base]:not([data-bui-variant="outlined"])[data-keyboard-focus="true"] .bui-input__wrapper {
    outline: 2px solid var(--palette-highlight);
    outline-offset: 2px;
}
""";
    }
}
