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
   Shared by: BUIInputText, BUIInputTextArea, 
              BUIInputNumber, BUIInputSelect, etc.
   Applied via: data-bui-input-base attribute
   ============================================ */

/* === BASE VARIABLES === */
[data-bui-input-base] {
    display: inline-flex;
    flex-direction: column;
    gap: 0.25rem;
    
    --bui-input-padding-y: 1rem;
    --bui-input-padding-x: 0.875rem;
    --bui-input-min-height: 3.5rem;
    --bui-input-border-radius: 4px;
    --bui-input-border-color: rgba(0, 0, 0, 0.23);
    --bui-input-hover-border-color: var(--palette-backgroundcontrast);
    --bui-input-focus-border-color: var(--palette-primary);
    --bui-input-background: transparent;
    --bui-input-transition: all 200ms cubic-bezier(0.4, 0, 0.2, 1);
    
    --bui-label-color: rgba(0, 0, 0, 0.6);
    --bui-label-focus-color: var(--palette-primary);
    --bui-label-error-color: var(--palette-error);
    --bui-label-size: 1rem;
    --bui-label-floated-size: 0.75rem;
    --bui-label-floated-top: 0;
}

[data-theme="dark"] [data-bui-input-base] {
    --bui-input-border-color: rgba(255, 255, 255, 0.23);
    --bui-label-color: rgba(255, 255, 255, 0.7);
}

/* === SIZE VARIANTS === */
[data-bui-input-base][data-bui-size="small"] {
    --bui-input-padding-y: 0.5rem;
    --bui-input-padding-x: 0.75rem;
    --bui-input-min-height: 2.5rem;
    --bui-label-size: 0.875rem;
    --bui-label-floated-size: 0.625rem;
}

[data-bui-input-base][data-bui-size="large"] {
    --bui-input-padding-y: 1.25rem;
    --bui-input-padding-x: 1rem;
    --bui-input-min-height: 4rem;
    --bui-label-size: 1.125rem;
    --bui-label-floated-size: 0.875rem;
}

/* === WRAPPER BASE === */
[data-bui-input-base] .bui-input__wrapper {
    position: relative;
    display: inline-flex;
    align-items: center;
    min-height: calc(var(--bui-input-min-height) * var(--bui-density-spacing-multiplier, 1));
    font-family: inherit;
    font-size: inherit;
    line-height: 1.5;
    color: inherit;
    background-color: var(--bui-input-background);
    transition: var(--bui-input-transition);
}

/* === INPUT FIELD === */
[data-bui-input-base] .bui-input__field {
    flex: 1;
    width: 100%;
    padding: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1)) 
             calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1));
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

/* === FLOATING LABEL BASE === */
[data-bui-input-base] .bui-input__label {
    position: absolute;
    left: calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1));
    top: 50%;
    transform: translateY(-50%);
    font-size: var(--bui-label-size);
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
    margin-left: 0.125rem;
}

/* ============================================
   VARIANT: OUTLINED
   ============================================ */

/* El wrapper NO tiene borde - el fieldset es el que lo tiene */
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper {
    border: none;
    border-radius: var(--bui-input-border-radius);
}

/* Fieldset crea el borde con el hueco para el label */
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__fieldset {
    position: absolute;
    inset: -5px 0 0 0;
    margin: 0;
    padding: 0 8px;
    border: 1px solid var(--bui-input-border-color);
    border-radius: var(--bui-input-border-radius);
    overflow: hidden;
    pointer-events: none;
    text-align: left;
}

/* Legend crea el hueco en el borde */
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__legend {
    float: unset;
    width: auto;
    height: 11px;
    padding: 0;
    font-size: var(--bui-label-floated-size);
    line-height: 11px;
    visibility: hidden;
    max-width: 0.01px;
    transition: max-width 50ms cubic-bezier(0.4, 0, 0.2, 1);
    white-space: nowrap;
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__legend span {
    padding-left: 5px;
    padding-right: 5px;
    display: inline-block;
}

/* Label posición inicial (dentro del input) */
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__label {
    left: calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1));
    background: transparent;
}

/* Estado flotado - label sube */
[data-bui-input-base][data-bui-variant="outlined"][data-bui-floated="true"] .bui-input__label {
    top: 0;
    transform: translateY(-50%) scale(0.75);
    padding: 0 5px;
    left: calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1) - 5px);
    max-width: calc(133% - 2 * var(--bui-input-padding-x));
}

/* Estado flotado - legend se expande para crear el hueco */
[data-bui-input-base][data-bui-variant="outlined"][data-bui-floated="true"] .bui-input__legend {
    max-width: 100%;
    transition: max-width 100ms cubic-bezier(0.4, 0, 0.2, 1) 50ms;
}

/* Hover */
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper:hover:not(:has(:disabled)) .bui-input__fieldset {
    border-color: var(--bui-input-hover-border-color);
}

/* Focus */
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

/* Campo con padding extra arriba para el label */
[data-bui-input-base][data-bui-variant="filled"] .bui-input__field {
    padding-top: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1) + 0.75rem);
    padding-bottom: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1) - 0.25rem);
}

/* Label posición inicial */
[data-bui-input-base][data-bui-variant="filled"] .bui-input__label {
    top: 50%;
    transform: translateY(-50%);
}

/* Estado flotado */
[data-bui-input-base][data-bui-variant="filled"][data-bui-floated="true"] .bui-input__label {
    top: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1) * 0.5 + 0.25rem);
    transform: translateY(0) scale(0.75);
}

/* Hover */
[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    --bui-input-background: rgba(0, 0, 0, 0.09);
}

[data-theme="dark"] [data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    --bui-input-background: rgba(255, 255, 255, 0.13);
}

/* Focus */
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
    min-height: calc(var(--bui-input-min-height) * var(--bui-density-spacing-multiplier, 1) - 0.5rem);
}

/* Campo con padding mínimo horizontal */
[data-bui-input-base][data-bui-variant="standard"] .bui-input__field {
    padding-left: 0;
    padding-right: 0;
    padding-top: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1) + 0.5rem);
    padding-bottom: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1) * 0.5);
}

/* Label posición inicial */
[data-bui-input-base][data-bui-variant="standard"] .bui-input__label {
    left: 0;
    top: 50%;
    transform: translateY(-50%);
}

/* Estado flotado */
[data-bui-input-base][data-bui-variant="standard"][data-bui-floated="true"] .bui-input__label {
    top: 0;
    transform: translateY(0) scale(0.75);
}

/* Hover */
[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    border-bottom-color: var(--bui-input-hover-border-color);
}

/* Focus */
[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--bui-input-focus-border-color);
    border-bottom-width: 2px;
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--bui-label-focus-color);
}

/* ============================================
   ESTADOS GLOBALES
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

[data-bui-input-base][data-bui-error="true"][data-bui-variant="outlined"] .bui-input__wrapper:focus-within .bui-input__fieldset {
    border-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="filled"] .bui-input__wrapper:focus-within,
[data-bui-input-base][data-bui-error="true"][data-bui-variant="standard"] .bui-input__wrapper:focus-within {
    border-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"] .bui-input__wrapper:focus-within .bui-input__label {
    color: var(--bui-label-error-color);
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
    right: calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1));
    top: 50%;
    transform: translateY(-50%);
}

/* === HELPER TEXT === */
[data-bui-input-base] .bui-input__helper-text {
    font-size: 0.75rem;
    color: var(--bui-label-color);
    padding-left: calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1));
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__helper-text {
    padding-left: 0;
}

/* === VALIDATION === */
[data-bui-input-base] .bui-input__validation {
    font-size: 0.75rem;
    color: var(--palette-error);
    padding-left: calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1));
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__validation {
    padding-left: 0;
}
""";
}