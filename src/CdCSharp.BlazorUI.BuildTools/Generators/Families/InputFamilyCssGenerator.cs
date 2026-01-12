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
    display: flex;
    flex-direction: column;
    
    --bui-padding-y: 0.5rem;
    --bui-padding-x: 0.75rem;
    --bui-input-inner-padding: 0.250rem 0.375rem;
    --bui-input-min-height: 2.5rem;
    --bui-input-border-radius: 4px;
    --bui-input-border-color: rgba(0, 0, 0, 0.23);
    --bui-input-hover-border-color: var(--palette-backgroundcontrast);
    --bui-input-focus-border-color: var(--palette-primary);
    --bui-input-transition: all 200ms ease-in-out;
}

[data-theme="dark"] [data-bui-input-base] {
    --bui-input-border-color: rgba(255, 255, 255, 0.23);
}

/* === SIZE VARIANTS === */
[data-bui-input-base][data-bui-size="small"] {
    --bui-padding-y: 0.375rem;
    --bui-padding-x: 0.5rem;
    --bui-input-min-height: 2rem;
}

[data-bui-input-base][data-bui-size="large"] {
    --bui-padding-y: 0.625rem;
    --bui-padding-x: 1rem;
    --bui-input-min-height: 3rem;
}

/* === COMMON FIELD STYLES === */
[data-bui-input-base] .bui-input__wrapper {
    display: inline-flex;
    align-items: center;
    gap: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
}

[data-bui-input-base] .bui-input__wrapper {
    position: relative;
    font-family: inherit;
    font-size: inherit;
    line-height: 1.5;
    color: inherit;
    background-color: transparent;
    transition: var(--bui-input-transition);
    min-height: calc(var(--bui-input-min-height) * var(--bui-density-spacing-multiplier, 1));
}

[data-bui-input-base] .bui-input__wrapper .bui-input__field {
    padding: var(--bui-input-inner-padding);
}

/* === VARIANT: OUTLINED === */
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper {
    border: 1px solid var(--bui-input-border-color);
    border-radius: var(--bui-input-border-radius);
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    border-color: var(--bui-input-hover-border-color);
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper:focus-within {
    border-color: var(--bui-input-focus-border-color);
    box-shadow: 0 0 0 1px var(--bui-input-focus-border-color);
    outline: none;
}

/* === VARIANT: FILLED === */
[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper {
    background-color: rgba(0, 0, 0, 0.06);
    border: none;
    border-bottom: 2px solid transparent;
    border-top-left-radius: var(--bui-input-border-radius);
    border-top-right-radius: var(--bui-input-border-radius);
}

[data-theme="dark"] [data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper {
    background-color: rgba(255, 255, 255, 0.06);
}

[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    background-color: rgba(0, 0, 0, 0.09);
}

[data-theme="dark"] [data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    background-color: rgba(255, 255, 255, 0.09);
}

[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--bui-input-focus-border-color);
    outline: none;
}

/* === VARIANT: STANDARD === */
[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper {
    border: none;
    border-bottom: 1px solid rgba(0, 0, 0, 0.42);
    border-radius: 0;
}

[data-theme="dark"] [data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper {
    border-bottom-color: rgba(255, 255, 255, 0.42);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    border-bottom-color: var(--bui-input-hover-border-color);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--bui-input-focus-border-color);
    border-bottom-width: 2px;
    outline: none;
}

/* === STATE: ERROR === */
[data-bui-input-base][data-bui-error="true"] .bui-input__wrapper {
    border-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="outlined"] .bui-input__wrapper:focus-within {
    border-color: var(--palette-error);
    box-shadow: 0 0 0 1px var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="filled"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="standard"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--palette-error);
}
""";
}