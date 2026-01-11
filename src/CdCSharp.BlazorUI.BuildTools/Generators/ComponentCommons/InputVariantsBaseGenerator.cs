using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class InputVariantsBaseGenerator : IAssetGenerator
{
    public string Name => "Dropdown Shared CSS";

    public string FileName => "_dropdown-base.css";

    public async Task<string> GetContent() => """
/* ============================================
   Input Variants Base Styles
   Shared by: Text, Number, TextArea, Select, Dropdown
   ============================================ */

/* Base input variables */
[data-bui-input-base] {
    --bui-input-padding-y: 0.5rem;
    --bui-input-padding-x: 0.75rem;
    --bui-input-min-height: 2.5rem;
    --bui-input-border-radius: 4px;
    --bui-input-border-color: rgba(0, 0, 0, 0.23);
    --bui-input-hover-border-color: var(--palette-backgroundcontrast);
    --bui-input-focus-border-color: var(--palette-primary);
    --bui-input-transition: all 200ms ease-in-out;
}

/* Size: Small */
[data-bui-input-base][data-bui-size="small"] {
    --bui-input-padding-y: 0.375rem;
    --bui-input-padding-x: 0.5rem;
    --bui-input-min-height: 2rem;
}

/* Size: Large */
[data-bui-input-base][data-bui-size="large"] {
    --bui-input-padding-y: 0.625rem;
    --bui-input-padding-x: 1rem;
    --bui-input-min-height: 3rem;
}

/* ===== VARIANT: Outlined ===== */
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__field,
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper {
    border: 1px solid var(--bui-input-border-color);
    border-radius: var(--bui-input-border-radius);
    padding: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1)) calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1));
    background-color: transparent;
    transition: var(--bui-input-transition);
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__field:hover:not(:disabled):not(:read-only),
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    border-color: var(--bui-input-hover-border-color);
}

[data-bui-input-base][data-bui-variant="outlined"] .bui-input__field:focus,
[data-bui-input-base][data-bui-variant="outlined"] .bui-input__wrapper:focus-within {
    border-color: var(--bui-input-focus-border-color);
    box-shadow: 0 0 0 1px var(--bui-input-focus-border-color);
    outline: none;
}

/* ===== VARIANT: Filled ===== */
[data-bui-input-base][data-bui-variant="filled"] .bui-input__field,
[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper {
    background-color: rgba(0, 0, 0, 0.06);
    border: none;
    border-bottom: 2px solid transparent;
    border-top-left-radius: var(--bui-input-border-radius);
    border-top-right-radius: var(--bui-input-border-radius);
    padding: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1)) calc(var(--bui-input-padding-x) * var(--bui-density-spacing-multiplier, 1));
    transition: var(--bui-input-transition);
}

[data-bui-input-base][data-bui-variant="filled"] .bui-input__field:hover:not(:disabled):not(:read-only),
[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    background-color: rgba(0, 0, 0, 0.09);
}

[data-bui-input-base][data-bui-variant="filled"] .bui-input__field:focus,
[data-bui-input-base][data-bui-variant="filled"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--bui-input-focus-border-color);
    outline: none;
}

/* ===== VARIANT: Standard ===== */
[data-bui-input-base][data-bui-variant="standard"] .bui-input__field,
[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper {
    border: none;
    border-bottom: 1px solid rgba(0, 0, 0, 0.42);
    border-radius: 0;
    padding: calc(var(--bui-input-padding-y) * var(--bui-density-spacing-multiplier, 1)) 0;
    background-color: transparent;
    transition: var(--bui-input-transition);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__field:hover:not(:disabled):not(:read-only),
[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:hover:not(:has(:disabled)) {
    border-bottom-color: var(--bui-input-hover-border-color);
}

[data-bui-input-base][data-bui-variant="standard"] .bui-input__field:focus,
[data-bui-input-base][data-bui-variant="standard"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--bui-input-focus-border-color);
    border-bottom-width: 2px;
    outline: none;
}

/* ===== STATES ===== */

/* Disabled */
[data-bui-input-base][data-bui-disabled="true"] .bui-input__field,
[data-bui-input-base][data-bui-disabled="true"] .bui-input__wrapper {
    cursor: not-allowed;
    opacity: 0.6;
}

/* ReadOnly */
[data-bui-input-base][data-bui-readonly="true"] .bui-input__field {
    cursor: default;
}

/* Error */
[data-bui-input-base][data-bui-error="true"] .bui-input__field,
[data-bui-input-base][data-bui-error="true"] .bui-input__wrapper {
    border-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="outlined"] .bui-input__field:focus,
[data-bui-input-base][data-bui-error="true"][data-bui-variant="outlined"] .bui-input__wrapper:focus-within {
    border-color: var(--palette-error);
    box-shadow: 0 0 0 1px var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="filled"] .bui-input__field:focus,
[data-bui-input-base][data-bui-error="true"][data-bui-variant="filled"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--palette-error);
}

[data-bui-input-base][data-bui-error="true"][data-bui-variant="standard"] .bui-input__field:focus,
[data-bui-input-base][data-bui-error="true"][data-bui-variant="standard"] .bui-input__wrapper:focus-within {
    border-bottom-color: var(--palette-error);
}

/* Full width */
[data-bui-input-base][data-bui-fullwidth="true"] {
    width: 100%;
}
""";
}