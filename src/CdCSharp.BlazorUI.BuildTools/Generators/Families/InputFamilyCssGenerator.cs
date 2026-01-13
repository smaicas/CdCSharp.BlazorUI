using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Core.Assets.Generators;

/// <summary>
/// Generates shared styles for input family components. Border and border-radius have fallback
/// values that can be overridden via IHasBorder. Transitions use fixed values as fallback for
/// future IHasTransitions implementation.
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class InputFamilyGenerator : IAssetGenerator
{
    public string FileName => "_input-family.css";
    public string Name => "Input Family";

    public async Task<string> GetContent()
    {
        return $$"""
/* ========================================
   Input Family Styles
   Auto-generated - Do not edit manually

   Shared styles for all input components.
   Border/radius can be overridden via IHasBorder.
   ======================================== */

/* ========================================
   BASE STRUCTURE
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] {
    flex-direction: column;
    gap: 0.25rem;

    /* Size-dependent variables using multiplier */
    --input-height: calc(2.5rem * var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1));
    --input-padding-x: calc(0.75rem * var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1));
    --input-padding-y: calc(0.5rem * var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1));

    /* Border fallbacks - overridable via IHasBorder inline variables */
    {{FeatureDefinitions.ComponentVariables.Input.BorderColor}}: var({{FeatureDefinitions.InlineVariables.Border}}, var(--palette-border, currentColor));
    {{FeatureDefinitions.ComponentVariables.Input.BorderWidth}}: 1px;
    {{FeatureDefinitions.ComponentVariables.Input.BorderRadius}}: var({{FeatureDefinitions.InlineVariables.BorderRadius}}, 4px);

    {{FeatureDefinitions.ComponentVariables.Input.Background}}: transparent;

    /* Transition fallback - fixed value for now, future IHasTransitions support */
    {{FeatureDefinitions.ComponentVariables.Input.Transition}}: 200ms ease;

    /* Label colors */
    {{FeatureDefinitions.ComponentVariables.Input.LabelColor}}: var(--palette-surfacecontrast);
    {{FeatureDefinitions.ComponentVariables.Input.LabelFocusColor}}: var(--palette-primary);
    {{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}}: var(--palette-error);
    {{FeatureDefinitions.ComponentVariables.Input.LabelScale}}: 0.85;
}

/* ========================================
   WRAPPER
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__wrapper {
    position: relative;
    display: flex;
    align-items: center;
    min-height: var(--input-height);
    background: var({{FeatureDefinitions.ComponentVariables.Input.Background}});
    transition: var({{FeatureDefinitions.ComponentVariables.Input.Transition}});
}

/* ========================================
   FIELD
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__field {
    flex: 1;
    width: 100%;
    padding: var(--input-padding-y) var(--input-padding-x);
    border: none;
    background: transparent;
    font: inherit;
    color: inherit;
    outline: none;
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__field::placeholder {
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelColor}});
    opacity: 0;
    transition: opacity 200ms;
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Floated}}="true"] .bui-input__field::placeholder {
    opacity: var({{FeatureDefinitions.Tokens.Opacity.Placeholder}});
}

/* ========================================
   FLOATING LABEL
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__label {
    position: absolute;
    left: var(--input-padding-x);
    top: 50%;
    transform: translateY(-50%);
    font-size: inherit;
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelColor}});
    pointer-events: none;
    transition: var({{FeatureDefinitions.ComponentVariables.Input.Transition}});
    transform-origin: top left;
    max-width: calc(100% - 2 * var(--input-padding-x));
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__required {
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
    margin-left: 0.125em;
}

/* ========================================
   FIELDSET/LEGEND (outlined only)
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__fieldset {
    display: none;
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"] .bui-input__fieldset {
    display: block;
    position: absolute;
    inset: -0.35em 0 0 0;
    margin: 0;
    padding: 0 0.5em;
    border: var({{FeatureDefinitions.ComponentVariables.Input.BorderWidth}}) solid var({{FeatureDefinitions.ComponentVariables.Input.BorderColor}});
    border-radius: var({{FeatureDefinitions.ComponentVariables.Input.BorderRadius}});
    overflow: hidden;
    pointer-events: none;
    transition: var({{FeatureDefinitions.ComponentVariables.Input.Transition}});
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"] .bui-input__legend {
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

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"] .bui-input__legend span {
    padding: 0 calc(0.35em * var({{FeatureDefinitions.ComponentVariables.Input.LabelScale}}));
    display: inline-block;
    font-size: calc(1em * var({{FeatureDefinitions.ComponentVariables.Input.LabelScale}}));
}

/* ========================================
   HELPER & VALIDATION
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__helper-text {
    font-size: 0.875em;
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelColor}});
    opacity: 0.7;
    padding-left: var(--input-padding-x);
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__validation {
    font-size: 0.875em;
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
    padding-left: var(--input-padding-x);
}

/* ========================================
   LOADING INDICATOR
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}] .bui-input__loading {
    position: absolute;
    right: var(--input-padding-x);
    top: 50%;
    transform: translateY(-50%);
}

/* ========================================
   VARIANT: OUTLINED
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"] .bui-input__wrapper {
    border: none;
}

/* Floated state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"][{{FeatureDefinitions.DataAttributes.Floated}}="true"] .bui-input__label {
    top: 0;
    transform: translateY(-50%) scale(var({{FeatureDefinitions.ComponentVariables.Input.LabelScale}}));
    transform-origin: top left;
    left: calc(0.45em / var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1));
    padding: 0 calc(0.35em * var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1));
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"][{{FeatureDefinitions.DataAttributes.Floated}}="true"] .bui-input__legend {
    max-width: 100%;
    transition: max-width 100ms cubic-bezier(0.4, 0, 0.2, 1) 50ms;
}

/* Hover state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"] .bui-input__wrapper:hover:not(:has(:disabled)) .bui-input__fieldset {
    border-color: var(--palette-surfacecontrast);
}

/* Focus state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"] .bui-input__wrapper:focus-within .bui-input__fieldset {
    border-color: var({{FeatureDefinitions.ComponentVariables.Input.LabelFocusColor}});
    border-width: 2px;
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelFocusColor}});
}

/* Error state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"][{{FeatureDefinitions.DataAttributes.Error}}="true"] .bui-input__fieldset {
    border-color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"][{{FeatureDefinitions.DataAttributes.Error}}="true"] .bui-input__label {
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="outlined"][{{FeatureDefinitions.DataAttributes.Error}}="true"] .bui-input__wrapper:focus-within .bui-input__fieldset {
    border-color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
}

/* ========================================
   VARIANT: FILLED
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"] {
    {{FeatureDefinitions.ComponentVariables.Input.BorderRadius}}: var({{FeatureDefinitions.InlineVariables.BorderRadius}}, 4px 4px 0 0);
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"] .bui-input__wrapper {
    {{FeatureDefinitions.ComponentVariables.Input.Background}}: var(--palette-surface);
    border-bottom: 2px solid var({{FeatureDefinitions.ComponentVariables.Input.BorderColor}});
    border-radius: var({{FeatureDefinitions.ComponentVariables.Input.BorderRadius}});
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"] .bui-input__field {
    padding-top: calc(var(--input-padding-y) + (0.75em * var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1)));
}

/* Floated state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"][{{FeatureDefinitions.DataAttributes.Floated}}="true"] .bui-input__label {
    top: 0px;
    transform: translateY(0) scale(var({{FeatureDefinitions.ComponentVariables.Input.LabelScale}}));
}

/* Hover state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    filter: brightness(1.1);
}

/* Focus state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"] .bui-input__wrapper:focus-within {
    border-bottom-color: var({{FeatureDefinitions.ComponentVariables.Input.LabelFocusColor}});
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelFocusColor}});
}

/* Error state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"][{{FeatureDefinitions.DataAttributes.Error}}="true"] .bui-input__wrapper {
    border-bottom-color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="filled"][{{FeatureDefinitions.DataAttributes.Error}}="true"] .bui-input__label {
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
}

/* ========================================
   VARIANT: STANDARD
   ======================================== */

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] {
    {{FeatureDefinitions.ComponentVariables.Input.BorderRadius}}: var({{FeatureDefinitions.InlineVariables.BorderRadius}}, 0);
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] .bui-input__wrapper {
    border-bottom: 1px solid var({{FeatureDefinitions.ComponentVariables.Input.BorderColor}});
    border-radius: 0;
    min-height: calc(var(--input-height) - 0.5em);
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] .bui-input__field {
    padding-left: 0;
    padding-right: 0;
    padding-top: calc(var(--input-padding-y) + (0.75em * var({{FeatureDefinitions.ComponentVariables.Size.Multiplier}}, 1)));
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] .bui-input__label {
    left: 0;
}

/* Floated state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"][{{FeatureDefinitions.DataAttributes.Floated}}="true"] .bui-input__label {
    top: 0;
    transform: translateY(0) scale(var({{FeatureDefinitions.ComponentVariables.Input.LabelScale}}));
}

/* Hover state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    border-bottom-color: var(--palette-surfacecontrast);
}

/* Focus state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] .bui-input__wrapper:focus-within {
    border-bottom-width: 2px;
    border-bottom-color: var({{FeatureDefinitions.ComponentVariables.Input.LabelFocusColor}});
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelFocusColor}});
}

/* Error state */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"][{{FeatureDefinitions.DataAttributes.Error}}="true"] .bui-input__wrapper {
    border-bottom-color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
}

bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"][{{FeatureDefinitions.DataAttributes.Error}}="true"] .bui-input__label {
    color: var({{FeatureDefinitions.ComponentVariables.Input.LabelErrorColor}});
}

/* No left padding for helper/validation in standard variant */
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] .bui-input__helper-text,
bui-component[{{FeatureDefinitions.DataAttributes.InputBase}}][{{FeatureDefinitions.DataAttributes.Variant}}="standard"] .bui-input__validation {
    padding-left: 0;
}
""";
    }
}