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
        string label = FeatureDefinitions.CssClasses.Input.Label;
        string required = FeatureDefinitions.CssClasses.Input.Required;
        string helper = FeatureDefinitions.CssClasses.Input.HelperText;
        string validation = FeatureDefinitions.CssClasses.Input.Validation;
        string opacityPlaceholder = FeatureDefinitions.Tokens.Opacity.Placeholder;

        string addon = FeatureDefinitions.CssClasses.Input.Addon;
        string addonBtn = FeatureDefinitions.CssClasses.Input.AddonBtn;
        string addonPrefix = FeatureDefinitions.CssClasses.Input.AddonPrefix;
        string addonSuffix = FeatureDefinitions.CssClasses.Input.AddonSuffix;

        string outline = FeatureDefinitions.CssClasses.Input.Outline;
        string outlineLeading = FeatureDefinitions.CssClasses.Input.OutlineLeading;
        string outlineNotch = FeatureDefinitions.CssClasses.Input.OutlineNotch;
        string outlineTrailing = FeatureDefinitions.CssClasses.Input.OutlineTrailing;

        return $$"""
/* ========================================
   Input Family Styles
   Auto-generated - Do not edit manually
   ======================================== */

/* === BASE === */

bui-component[{{inputBase}}] {
    position: relative;
    display: flex;
    flex-direction: column;
    width: 100%;
    gap: 0.25rem;

    --_input-h: calc(3.5rem * {{V(sizeMult, "1")}});
    --_input-px: calc(1rem * {{V(sizeMult, "1")}});
    --_input-py: calc(1rem * {{V(sizeMult, "1")}});
    --_input-radius: {{V(inlineRadius, "4px")}};
    --_input-transition: 150ms cubic-bezier(0.4, 0, 0.2, 1);
    --_input-scale: 0.75;
    --_input-floated-size: calc(1rem * var(--_input-scale) * {{V(sizeMult, "1")}});

    --_input-label-color: var(--palette-surfacecontrast);
    --_input-focus-color: var(--palette-primary);
    --_input-error-color: var(--palette-error);

    --_input-border-color: {{V(inlineBorder, "var(--palette-border)")}};
    --_input-border-width: 1px;

    --_wrapper-bg: transparent;
    --_wrapper-radius: var(--_input-radius);
    --_wrapper-min-h: var(--_input-h);
    --_wrapper-pt: 0px;

    --_field-px: var(--_input-px);

    --_addon-offset: 0rem;
    --_outline-leading-width: calc(var(--_input-px) - 4px + var(--_addon-offset));
}

/* === WRAPPER === */

bui-component[{{inputBase}}] .{{wrapper}} {
    position: relative;
    display: flex;
    align-items: center;
    min-height: var(--_wrapper-min-h);
    padding-block-start: var(--_wrapper-pt);
    background: var(--_wrapper-bg);
    border-radius: var(--_wrapper-radius);
}

bui-component[{{inputBase}}] .{{wrapper}} > *:not(.{{outline}}) {
    z-index: 1;
}

/* === FIELD === */

bui-component[{{inputBase}}] .{{field}} {
    flex: 1;
    width: 100%;
    min-width: 0;
    height: 100%;
    padding-block: var(--_input-py);
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
    transition: opacity var(--_input-transition);
}

bui-component[{{inputBase}}][{{floated}}="true"] .{{field}}::placeholder {
    opacity: {{V(opacityPlaceholder)}};
}

/* === OUTLINE SYSTEM === */

bui-component[{{inputBase}}] .{{outline}} {
    position: absolute;
    inset: 0;
    display: flex;
    pointer-events: none;
}

bui-component[{{inputBase}}] .{{outlineLeading}} {
    width: var(--_outline-leading-width);
    border: var(--_input-border-width) solid var(--_input-border-color);
    border-inline-end: none;
    border-radius: var(--_input-radius) 0 0 var(--_input-radius);
    transition: border-color var(--_input-transition), border-width var(--_input-transition);
}

bui-component[{{inputBase}}] .{{outlineNotch}} {
    position: relative;
    display: flex;
    flex-direction: column;
    border-block-start: var(--_input-border-width) solid var(--_input-border-color);
    border-block-end: var(--_input-border-width) solid var(--_input-border-color);
    transition: border-color var(--_input-transition), border-width var(--_input-transition);
}

bui-component[{{inputBase}}] .{{outlineTrailing}} {
    flex: 1;
    border: var(--_input-border-width) solid var(--_input-border-color);
    border-inline-start: none;
    border-radius: 0 var(--_input-radius) var(--_input-radius) 0;
    transition: border-color var(--_input-transition), border-width var(--_input-transition);
}

/* === LABEL === */

bui-component[{{inputBase}}] .{{label}} {
    display: inline-flex;
    align-items: center;
    padding-inline: 4px;
    font-size: 1rem;
    line-height: 1;
    color: var(--_input-label-color);
    white-space: nowrap;
    pointer-events: none;
    transition: font-size var(--_input-transition), transform var(--_input-transition), color var(--_input-transition), padding var(--_input-transition);
}

bui-component[{{inputBase}}] .{{required}} {
    color: var(--_input-error-color);
    margin-inline-start: 0.25em;
}

bui-component[{{inputBase}}]:focus-within .{{label}} {
    color: var(--_input-focus-color);
}

bui-component[{{inputBase}}][{{error}}="true"] .{{label}} {
    color: var(--_input-error-color);
}

/* === HELPER & VALIDATION === */

bui-component[{{inputBase}}] .{{helper}} {
    font-size: 0.75rem;
    color: var(--_input-label-color);
    opacity: 0.7;
    padding-inline-start: var(--_input-px);
}

bui-component[{{inputBase}}] .{{validation}} {
    font-size: 0.75rem;
    color: var(--_input-error-color);
    padding-inline-start: var(--_input-px);
}

/* === ADDON BASE === */

bui-component[{{inputBase}}] .{{addon}} {
    display: flex;
    align-items: center;
    justify-content: center;
    align-self: stretch;
    flex-shrink: 0;
    gap: 0.25rem;
    width: calc(2.5rem * {{V(sizeMult, "1")}});
    margin: 0;
    padding: 0;
    border: none;
    border-inline-start: 1px solid var(--_input-border-color);
    border-radius: 0;
    background: transparent;
    color: var(--_input-label-color);
    overflow: hidden;
    transition: background-color 150ms ease, border-color 150ms ease;
}

bui-component[{{inputBase}}] .{{addonBtn}} {
    width: calc(3rem * {{V(sizeMult, "1")}});
    cursor: pointer;
}

bui-component[{{inputBase}}] .{{addonBtn}}:hover:not(:disabled) {
    background: color-mix(in srgb, var(--palette-surfacecontrast) 4%, transparent);
}

bui-component[{{inputBase}}] .{{addonBtn}}:disabled {
    cursor: not-allowed;
    opacity: 0.5;
}

bui-component[{{inputBase}}]:focus-within .{{addon}} {
    border-inline-start-color: var(--_input-focus-color);
}

bui-component[{{inputBase}}][{{error}}="true"] .{{addon}} {
    border-inline-start-color: var(--_input-error-color);
}

/* === ADDON PREFIX === */

bui-component[{{inputBase}}] .{{addonPrefix}} {
    order: -1;
    border-inline-start: none;
    border-inline-end: 1px solid var(--_input-border-color);
}

bui-component[{{inputBase}}]:has(.{{addonPrefix}}) {
    --_addon-offset: calc(2.5rem * {{V(sizeMult, "1")}});
}

/* ========================================
   VARIANT: OUTLINED
   ======================================== */

/* Label: resting state */
bui-component[{{inputBase}}][{{variant}}="outlined"] .{{label}} {
    transform: translateY(calc((var(--_input-h) / 2) - 0.5em));
}

/* Label: floated state */
bui-component[{{inputBase}}][{{variant}}="outlined"][{{floated}}="true"] .{{label}} {
    font-size: var(--_input-floated-size);
    transform: translateY(-50%);
    padding-inline: 4px;
}

/* Notch: open when floated */
bui-component[{{inputBase}}][{{variant}}="outlined"][{{floated}}="true"] .{{outlineNotch}} {
    border-block-start-color: transparent;
}

/* Focus state */
bui-component[{{inputBase}}][{{variant}}="outlined"]:focus-within .{{outlineLeading}},
bui-component[{{inputBase}}][{{variant}}="outlined"]:focus-within .{{outlineNotch}},
bui-component[{{inputBase}}][{{variant}}="outlined"]:focus-within .{{outlineTrailing}} {
    border-color: var(--_input-focus-color);
    border-width: 2px;
}

bui-component[{{inputBase}}][{{variant}}="outlined"]:focus-within[{{floated}}="true"] .{{outlineNotch}} {
    border-block-start-color: transparent;
}

/* Error state */
bui-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{outlineLeading}},
bui-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{outlineNotch}},
bui-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"] .{{outlineTrailing}} {
    border-color: var(--_input-error-color);
}

bui-component[{{inputBase}}][{{variant}}="outlined"][{{error}}="true"][{{floated}}="true"] .{{outlineNotch}} {
    border-block-start-color: transparent;
}

/* Addon adjustments */
bui-component[{{inputBase}}][{{variant}}="outlined"] .{{addon}} {
    border-start-end-radius: var(--_input-radius);
    border-end-end-radius: var(--_input-radius);
}

bui-component[{{inputBase}}][{{variant}}="outlined"] .{{addonPrefix}} {
    border-radius: 0;
    border-start-start-radius: var(--_input-radius);
    border-end-start-radius: var(--_input-radius);
}

/* ========================================
   VARIANT: FILLED
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="filled"] {
    --_wrapper-bg: {{V(inlineBg, "color-mix(in srgb, var(--palette-surfacecontrast) 6%, transparent)")}};
    --_wrapper-radius: 4px 4px 0 0;
    --_wrapper-pt: calc(0.75rem * {{V(sizeMult, "1")}});
    --_outline-leading-width: calc(var(--_input-px) + var(--_addon-offset));
}

/* Outline: only bottom border */
bui-component[{{inputBase}}][{{variant}}="filled"] .{{outlineLeading}},
bui-component[{{inputBase}}][{{variant}}="filled"] .{{outlineTrailing}} {
    border: none;
    border-block-end: var(--_input-border-width) solid var(--_input-border-color);
    border-radius: 0;
}

bui-component[{{inputBase}}][{{variant}}="filled"] .{{outlineNotch}} {
    border-block-start: none;
    align-items: flex-start;
}

/* Label: resting state */
bui-component[{{inputBase}}][{{variant}}="filled"] .{{label}} {
    transform: translateY(calc((var(--_input-h) + var(--_wrapper-pt)) / 2 - 0.5em));
}

/* Label: floated state */
bui-component[{{inputBase}}][{{variant}}="filled"][{{floated}}="true"] .{{label}} {
    font-size: var(--_input-floated-size);
    transform: translateY(calc(var(--_wrapper-pt) * 0.5));
}

/* Focus state */
bui-component[{{inputBase}}][{{variant}}="filled"]:focus-within .{{outlineLeading}},
bui-component[{{inputBase}}][{{variant}}="filled"]:focus-within .{{outlineNotch}},
bui-component[{{inputBase}}][{{variant}}="filled"]:focus-within .{{outlineTrailing}} {
    border-block-end-color: var(--_input-focus-color);
    border-block-end-width: 2px;
}

/* Error state */
bui-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{outlineLeading}},
bui-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{outlineNotch}},
bui-component[{{inputBase}}][{{variant}}="filled"][{{error}}="true"] .{{outlineTrailing}} {
    border-block-end-color: var(--_input-error-color);
}

/* Addon adjustments */
bui-component[{{inputBase}}][{{variant}}="filled"] .{{addon}} {
    border-start-end-radius: 4px;
    /* Adjust padding */
    margin-block-start: calc(-1 * var(--_wrapper-pt));
}

bui-component[{{inputBase}}][{{variant}}="filled"] .{{addonPrefix}} {
    border-radius: 0;
    border-start-start-radius: 4px;
}

/* ========================================
   VARIANT: STANDARD
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="standard"] {
    --_wrapper-pt: calc(1rem * {{V(sizeMult, "1")}});
    --_wrapper-radius: 0;
    --_field-px: 0;
    --_outline-leading-width: var(--_addon-offset);
}

/* Outline: only bottom border */
bui-component[{{inputBase}}][{{variant}}="standard"] .{{outlineLeading}} {
    border: none;
    border-block-end: var(--_input-border-width) solid var(--_input-border-color);
    border-radius: 0;
}

bui-component[{{inputBase}}][{{variant}}="standard"] .{{outlineNotch}} {
    border-block-start: none;
    align-items: flex-start;
}

bui-component[{{inputBase}}][{{variant}}="standard"] .{{outlineTrailing}} {
    border: none;
    border-block-end: var(--_input-border-width) solid var(--_input-border-color);
    border-radius: 0;
}

bui-component[{{inputBase}}][{{variant}}="standard"] .{{label}} {
    padding-inline-start: 0;
}

/* Label: resting state */
bui-component[{{inputBase}}][{{variant}}="standard"] .{{label}} {
    transform: translateY(calc((var(--_input-h) + var(--_wrapper-pt)) / 2 - 0.5em));
}

/* Label: floated state */
bui-component[{{inputBase}}][{{variant}}="standard"][{{floated}}="true"] .{{label}} {
    font-size: var(--_input-floated-size);
    transform: translateY(calc(var(--_wrapper-pt) * 0.25));
}

/* Addon adjustments */
bui-component[{{inputBase}}][{{variant}}="standard"] .{{addon}} {
    /* Adjust padding */
    margin-block-start: calc(-1 * var(--_wrapper-pt));
}

/* Focus state */
bui-component[{{inputBase}}][{{variant}}="standard"]:focus-within .{{outlineLeading}},
bui-component[{{inputBase}}][{{variant}}="standard"]:focus-within .{{outlineNotch}},
bui-component[{{inputBase}}][{{variant}}="standard"]:focus-within .{{outlineTrailing}} {
    border-block-end-color: var(--_input-focus-color);
    border-block-end-width: 2px;
}

/* Error state */
bui-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{outlineLeading}},
bui-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{outlineNotch}},
bui-component[{{inputBase}}][{{variant}}="standard"][{{error}}="true"] .{{outlineTrailing}} {
    border-block-end-color: var(--_input-error-color);
}

/* Helper/validation no padding */
bui-component[{{inputBase}}][{{variant}}="standard"] .{{helper}},
bui-component[{{inputBase}}][{{variant}}="standard"] .{{validation}} {
    padding-inline-start: 0;
}

/* ========================================
   KEYBOARD FOCUS INDICATORS
   ======================================== */

bui-component[{{inputBase}}][{{variant}}="outlined"][data-keyboard-focus="true"] .{{outline}} {
    outline: 2px solid var(--palette-highlight);
    outline-offset: 2px;
    border-radius: var(--_input-radius);
}

bui-component[{{inputBase}}]:not([{{variant}}="outlined"])[data-keyboard-focus="true"] .{{wrapper}} {
    outline: 2px solid var(--palette-highlight);
    outline-offset: 2px;
}
""";
    }
}