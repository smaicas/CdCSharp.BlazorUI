using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class InputFamilyCssGenerator : IAssetGenerator
{
    public string Name => "Input Family CSS";
    public string FileName => "_input-family.css";

    public async Task<string> GetContent() => """
/* ============================================
   Input Family Base Styles
   ============================================ */

/* === BASE VARIABLES === */
[data-bui-input-base] {
    flex-direction: column;
    
    /* Input dimensions - escalados por Size */
    --bui-input-height: 3.5em;
    --bui-input-padding-x: 0.875em;
    --bui-input-padding-y: 1em;
    --bui-input-border-radius: 4px;
    
    /* Colores */
    --bui-input-border-color: rgba(0, 0, 0, 0.23);
    --bui-input-hover-border-color: var(--palette-backgroundcontrast);
    --bui-input-focus-border-color: var(--palette-primary);
    --bui-input-background: transparent;
    
    /* Label colors */
    --bui-label-color: rgba(0, 0, 0, 0.6);
    --bui-label-focus-color: var(--palette-primary);
    --bui-label-error-color: var(--palette-error);
    
    /* Floating label - posiciones parametrizadas */
    --bui-label-floated-scale: 0.85;
    --bui-label-floated-top: 0;
    --bui-label-translate-y: -50%;
    
    /* Transition */
    --bui-input-transition: all 200ms cubic-bezier(0.4, 0, 0.2, 1);
}

[data-theme="dark"] [data-bui-input-base] {
    --bui-input-border-color: rgba(255, 255, 255, 0.23);
    --bui-label-color: rgba(255, 255, 255, 0.7);
}

/* === SIZE ADJUSTMENTS === */
[data-bui-input-base][data-bui-size="small"] {
    --bui-input-height: 2.5em;
    --bui-input-padding-x: 0.75em;
    --bui-input-padding-y: 0.5em;
}

[data-bui-input-base][data-bui-size="large"] {
    --bui-input-height: 4em;
    --bui-input-padding-x: 1em;
    --bui-input-padding-y: 1.25em;
}

/* === WRAPPER === */
[data-bui-input-base] .bui-input__wrapper {
    position: relative;
    display: inline-flex;
    align-items: center;
    min-height: var(--bui-input-height);
    font: inherit;
    color: inherit;
    background-color: var(--bui-input-background);
    transition: var(--bui-input-transition);
}

/* === INPUT FIELD === */
[data-bui-input-base] .bui-input__field {
    flex: 1;
    width: 100%;
    padding: var(--bui-input-padding-y) var(--bui-input-padding-x);
    border: none;
    background: transparent;
    font: inherit;
    color: inherit;
    outline: none;
}

[data-bui-input-base] .bui-input__field::placeholder {
    color: var(--bui-label-color);
    opacity: 0;
    transition: opacity 200ms;
}

[data-bui-input-base][data-bui-floated="true"] .bui-input__field::placeholder {
    opacity: 1;
}

/* === FLOATING LABEL === */
[data-bui-input-base] .bui-input__label {
    position: absolute;
    left: var(--bui-input-padding-x);
    top: 50%;
    transform: translateY(var(--bui-label-translate-y));
    font-size: var(--bui-font-size);
    color: var(--bui-label-color);
    pointer-events: none;
    transition: var(--bui-input-transition);
    transform-origin: left top;
    max-width: calc(100% - 2 * var(--bui-input-padding-x));
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

[data-bui-input-base] .bui-input__required {
    color: var(--palette-error);
    margin-left: 0.125em;
}

/* === FIELDSET/LEGEND (for Outlined) === */
[data-bui-input-base] .bui-input__fieldset {
    display: none;
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__fieldset {
    display: block;
    position: absolute;
    inset: -0.35em 0 0 0;
    margin: 0;
    padding: 0 0.5em;
    border: 1px solid var(--bui-input-border-color);
    border-radius: var(--bui-input-border-radius);
    overflow: hidden;
    pointer-events: none;
    text-align: left;
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__legend {
    float: unset;
    width: auto;
    height: 0.7em;
    padding: 0;
    font-size: calc(var(--bui-font-size) * var(--bui-label-floated-scale));
    line-height: 0.7em;
    visibility: hidden;
    max-width: 0.01px;
    transition: max-width 50ms cubic-bezier(0.4, 0, 0.2, 1);
    white-space: nowrap;
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__legend span {
    padding: 0 0.35em;
    display: inline-block;
}

/* ============================================
   VARIANT: OUTLINED
   ============================================ */

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper {
    border: none;
    border-radius: var(--bui-input-border-radius);
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__label {
    background: transparent;
}

[data-bui-input-base][data-bui-variant="outlined"][data-bui-floated="true"] .bui-input__label {
    top: var(--bui-label-floated-top);
    transform: translateY(-50%) scale(var(--bui-label-floated-scale));
    padding: 0 0.35em;
    left: calc(var(--bui-input-padding-x) - 0.35em);
}

[data-bui-input-base][data-bui-variant="outlined"][data-bui-floated="true"] .bui-input__legend {
    max-width: 100%;
    transition: max-width 100ms cubic-bezier(0.4, 0, 0.2, 1) 50ms;
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper:hover:not(:has(:disabled)) .bui-input__fieldset {
    border-color: var(--bui-input-hover-border-color);
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper:focus-within .bui-input__fieldset {
    border-color: var(--bui-input-focus-border-color);
    border-width: 2px;
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--bui-label-focus-color);
}

/* ============================================
   VARIANT: FILLED
   ============================================ */

[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper {
    --bui-input-background: rgba(0, 0, 0, 0.06);
    border: none;
    border-bottom: 1px solid var(--bui-input-border-color);
    border-top-left-radius: var(--bui-input-border-radius);
    border-top-right-radius: var(--bui-input-border-radius);
}

[data-theme="dark"] [data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper {
    --bui-input-background: rgba(255, 255, 255, 0.09);
}

[data-bui-input-base][data-bui-variant="filled"] .bui-input__field {
    padding-top: calc(var(--bui-input-padding-y) + 0.5em);
    padding-bottom: calc(var(--bui-input-padding-y) - 0.5em);
}

[data-bui-input-base][data-bui-variant="filled"][data-bui-floated="true"] .bui-input__label {
    top: 0.5em;
    transform: translateY(0) scale(var(--bui-label-floated-scale));
}

[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    --bui-input-background: rgba(0, 0, 0, 0.09);
}

[data-theme="dark"] [data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    --bui-input-background: rgba(255, 255, 255, 0.13);
}

[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--bui-input-focus-border-color);
    border-bottom-width: 2px;
}

[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--bui-label-focus-color);
}

/* ============================================
   VARIANT: STANDARD
   ============================================ */

[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper {
    border: none;
    border-bottom: 1px solid var(--bui-input-border-color);
    border-radius: 0;
    min-height: calc(var(--bui-input-height) - 0.5em);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__field {
    padding-left: 0;
    padding-right: 0;
    padding-top: calc(var(--bui-input-padding-y) + 0.5em);
    padding-bottom: calc(var(--bui-input-padding-y) - 0.5em);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__label {
    left: 0;
}

[data-bui-input-base][data-bui-variant="standard"][data-bui-floated="true"] .bui-input__label {
    top: 0;
    transform: translateY(0) scale(var(--bui-label-floated-scale));
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    border-bottom-color: var(--bui-input-hover-border-color);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--bui-input-focus-border-color);
    border-bottom-width: 2px;
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--bui-label-focus-color);
}

/* ============================================
   GLOBAL STATES
   ============================================ */

/* === ERROR === */
[data-bui-input-base][data-bui-error="true"][data-bui-variant="outlined"] .bui-input__fieldset {
    border-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="filled"] .bui-input__wrapper,
[data-bui-input-base][data-bui-error="true"][data-bui-variant="standard"] .bui-input__wrapper {
    border-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"] .bui-input__label {
    color: var(--bui-label-error-color);
}

[data-bui-input-base][data-bui-error="true"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--bui-label-error-color);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="outlined"] .bui-input__wrapper:focus-within .bui-input__fieldset {
    border-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="filled"] .bui-input__wrapper:focus-within,
[data-bui-input-base][data-bui-error="true"][data-bui-variant="standard"] .bui-input__wrapper:focus-within {
    border-color: var(--palette-error);
}

/* === DISABLED === */
[data-bui-input-base][data-bui-disabled="true"] .bui-input__wrapper {
    opacity: 0.6;
    cursor: not-allowed;
}

[data-bui-input-base][data-bui-disabled="true"] .bui-input__field {
    cursor: not-allowed;
}

/* === FULL WIDTH === */
[data-bui-input-base][data-bui-fullwidth="true"] {
    width: 100%;
}

[data-bui-input-base][data-bui-fullwidth="true"] .bui-input__wrapper {
    width: 100%;
}

/* === LOADING === */
[data-bui-input-base] .bui-input__loading {
    position: absolute;
    right: var(--bui-input-padding-x);
    top: 50%;
    transform: translateY(-50%);
}

/* === HELPER TEXT === */
[data-bui-input-base] .bui-input__helper-text {
    font-size: var(--bui-font-size-small, 0.875em);
    color: var(--bui-label-color);
    padding-left: var(--bui-input-padding-x);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__helper-text {
    padding-left: 0;
}

/* === VALIDATION === */
[data-bui-input-base] .bui-input__validation {
    font-size: var(--bui-font-size-small, 0.875em);
    color: var(--palette-error);
    padding-left: var(--bui-input-padding-x);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__validation {
    padding-left: 0;
}
""";
}