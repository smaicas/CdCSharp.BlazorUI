using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;
using static CdCSharp.BlazorUI.Core.Css.FeatureDefinitions;

namespace CdCSharp.BlazorUI.Core.Assets.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class InputFamilyGenerator : IAssetGenerator
{
    public string Name => "Input Family";
    public string FileName => "_input-family.css";

    public async Task<string> GetContent()
    {
        return $$"""
/* ========================================
   Input Family Styles (Production Ready)
   Auto-generated - Do not edit manually
   ======================================== */

/* === BASE STRUCTURE === */
bui-component[{{DataAttributes.InputBase}}] {
    flex-direction: column;
    gap: var({{Tokens.Spacing.Space1}});

    /* Input-specific variables */
    --input-height: var({{SizeVariables.Height}}, 2.5rem);
    --input-padding-x: var({{SizeVariables.PaddingX}}, var({{Tokens.Spacing.Space3}}));
    --input-padding-y: var({{SizeVariables.PaddingY}}, var({{Tokens.Spacing.Space2}}));
    --input-radius: var({{Tokens.Radius.Sm}});
    --input-border-color: var(--palette-border);
    --input-bg: transparent;
    --input-transition: var({{Tokens.Transition.Normal}});
    
    /* Label colors */
    --input-label-color: var(--palette-surfacecontrast);
    --input-label-focus-color: var(--palette-primary);
    --input-label-error-color: var(--palette-error);
    --input-label-scale: 0.85;
}

/* === WRAPPER === */
bui-component[{{DataAttributes.InputBase}}] .bui-input__wrapper {
    position: relative;
    display: flex;
    align-items: center;
    min-height: var(--input-height);
    background: var(--input-bg);
    transition: var(--input-transition);
}

/* === FIELD === */
bui-component[{{DataAttributes.InputBase}}] .bui-input__field {
    flex: 1;
    width: 100%;
    padding: var(--input-padding-y) var(--input-padding-x);
    border: none;
    background: transparent;
    font: inherit;
    color: inherit;
    outline: none;
}

bui-component[{{DataAttributes.InputBase}}] .bui-input__field::placeholder {
    color: var(--input-label-color);
    opacity: 0;
    transition: opacity 200ms;
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Floated}}="true"] .bui-input__field::placeholder {
    opacity: var({{Tokens.Opacity.Placeholder}});
}

/* === FLOATING LABEL === */
bui-component[{{DataAttributes.InputBase}}] .bui-input__label {
    position: absolute;
    left: var(--input-padding-x);
    top: 50%;
    transform: translateY(-50%);
    font-size: inherit;
    color: var(--input-label-color);
    pointer-events: none;
    transition: var(--input-transition);
    transform-origin: top left;
    max-width: calc(100% - 2 * var(--input-padding-x));
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

bui-component[{{DataAttributes.InputBase}}] .bui-input__required {
    color: var(--input-label-error-color);
    margin-left: 0.125em;
}

/* === FIELDSET/LEGEND (outlined only) === */
bui-component[{{DataAttributes.InputBase}}] .bui-input__fieldset {
    display: none;
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"] .bui-input__fieldset {
    display: block;
    position: absolute;
    inset: -0.35em 0 0 0;
    margin: 0;
    padding: 0 0.5em;
    border: 1px solid var(--input-border-color);
    border-radius: var(--input-radius);
    overflow: hidden;
    pointer-events: none;
    transition: var(--input-transition);
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"] .bui-input__legend {
    float: unset;
    width: auto;
    padding: 0;
    font-size: 1em;
    line-height: 1;
    visibility: hidden;
    max-width: 0.01px;
    transition: max-width 50ms cubic-bezier(0.4, 0, 0.2, 1);
    white-space: nowrap;
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"] .bui-input__legend span {
    padding: 0 calc(0.35em * var(--input-label-scale));
    display: inline-block;
    font-size: calc(1em * var(--input-label-scale));
}

/* === HELPER & VALIDATION === */
bui-component[{{DataAttributes.InputBase}}] .bui-input__helper-text {
    font-size: var({{Tokens.Typography.FontSizeSm}});
    color: var(--input-label-color);
    opacity: 0.7;
    padding-left: var(--input-padding-x);
}

bui-component[{{DataAttributes.InputBase}}] .bui-input__validation {
    font-size: var({{Tokens.Typography.FontSizeSm}});
    color: var(--input-label-error-color);
    padding-left: var(--input-padding-x);
}

/* === LOADING INDICATOR === */
bui-component[{{DataAttributes.InputBase}}] .bui-input__loading {
    position: absolute;
    right: var(--input-padding-x);
    top: 50%;
    transform: translateY(-50%);
}

/* ========================================
   VARIANT: OUTLINED
   ======================================== */

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"] .bui-input__wrapper {
    border: none;
}

/* Floated state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"][{{DataAttributes.Floated}}="true"] .bui-input__label {
    top: 0;
    transform: translateY(-50%) scale(var(--input-label-scale));
    transform-origin: top left;
    left: calc(.45em / var(--bui-size-multiplier));
    padding: 0 calc(.35em * var(--bui-size-multiplier));
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"][{{DataAttributes.Floated}}="true"] .bui-input__legend {
    max-width: 100%;
    transition: max-width 100ms cubic-bezier(0.4, 0, 0.2, 1) 50ms;
}

/* Hover state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"] .bui-input__wrapper:hover:not(:has(:disabled)) .bui-input__fieldset {
    border-color: var(--palette-surfacecontrast);
}

/* Focus state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"] .bui-input__wrapper:focus-within .bui-input__fieldset {
    border-color: var(--input-label-focus-color);
    border-width: 2px;
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--input-label-focus-color);
}

/* Error state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"][{{DataAttributes.Error}}="true"] .bui-input__fieldset {
    border-color: var(--input-label-error-color);
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"][{{DataAttributes.Error}}="true"] .bui-input__label {
    color: var(--input-label-error-color);
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="outlined"][{{DataAttributes.Error}}="true"] .bui-input__wrapper:focus-within .bui-input__fieldset {
    border-color: var(--input-label-error-color);
}

/* ========================================
   VARIANT: FILLED
   ======================================== */

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="filled"] .bui-input__wrapper {
    --input-bg: var(--palette-surface);
    border-bottom: 2px solid var(--input-border-color);
    border-radius: var(--input-radius) var(--input-radius) 0 0;
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="filled"] .bui-input__field {
    padding-top: calc(var(--input-padding-y) + (.75em * var(--bui-size-multiplier)));
}

/* Floated state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="filled"][{{DataAttributes.Floated}}="true"] .bui-input__label {
    top: 0px;
    transform: translateY(0) scale(var(--input-label-scale));
}

/* Hover state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    filter: brightness(1.1);
}

/* Focus state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="filled"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--input-label-focus-color);
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="filled"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--input-label-focus-color);
}

/* Error state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="filled"][{{DataAttributes.Error}}="true"] .bui-input__wrapper {
    border-bottom-color: var(--input-label-error-color);
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="filled"][{{DataAttributes.Error}}="true"] .bui-input__label {
    color: var(--input-label-error-color);
}

/* ========================================
   VARIANT: STANDARD
   ======================================== */

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"] .bui-input__wrapper {
    border-bottom: 1px solid var(--input-border-color);
    border-radius: 0;
    min-height: calc(var(--input-height) - 0.5em);
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"] .bui-input__field {
    padding-left: 0;
    padding-right: 0;
    padding-top: calc(var(--input-padding-y) + (.75em * var(--bui-size-multiplier)));
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"] .bui-input__label {
    left: 0;
}

/* Floated state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"][{{DataAttributes.Floated}}="true"] .bui-input__label {
    top: 0;
    transform: translateY(0) scale(var(--input-label-scale));
}

/* Hover state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    border-bottom-color: var(--palette-surfacecontrast);
}

/* Focus state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"] .bui-input__wrapper:focus-within {
    border-bottom-width: 2px;
    border-bottom-color: var(--input-label-focus-color);
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--input-label-focus-color);
}

/* Error state */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"][{{DataAttributes.Error}}="true"] .bui-input__wrapper {
    border-bottom-color: var(--input-label-error-color);
}

bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"][{{DataAttributes.Error}}="true"] .bui-input__label {
    color: var(--input-label-error-color);
}

/* No left padding for helper/validation in standard variant */
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"] .bui-input__helper-text,
bui-component[{{DataAttributes.InputBase}}][{{DataAttributes.Variant}}="standard"] .bui-input__validation {
    padding-left: 0;
}
""";
    }
}