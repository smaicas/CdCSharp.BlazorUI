using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class DropdownBaseGenerator : IAssetGenerator
{
    public string Name => "Dropdown Shared CSS";

    public string FileName => "_dropdown-base.css";

    public async Task<string> GetContent() => """
/* ============================================
   Dropdown Base Styles
   Shared by: BUIInputDropdown, BUIInputTreeDropdown
   ============================================ */

/* Base component setup */
[data-bui-dropdown-base] {
    display: flex;
    flex-direction: column;
    position: relative;
    --bui-dropdown-padding-y: 0.5rem;
    --bui-dropdown-padding-x: 0.75rem;
    --bui-dropdown-min-height: 2.5rem;
    --bui-dropdown-border-radius: 4px;
    --bui-dropdown-border-color: rgba(0, 0, 0, 0.23);
    --bui-dropdown-hover-border-color: var(--palette-backgroundcontrast);
    --bui-dropdown-focus-border-color: var(--palette-primary);
    --bui-dropdown-transition: all 200ms ease-in-out;
}

/* Size variants */
[data-bui-dropdown-base][data-bui-size="small"] {
    --bui-dropdown-padding-y: 0.375rem;
    --bui-dropdown-padding-x: 0.5rem;
    --bui-dropdown-min-height: 2rem;
}

[data-bui-dropdown-base][data-bui-size="large"] {
    --bui-dropdown-padding-y: 0.625rem;
    --bui-dropdown-padding-x: 1rem;
    --bui-dropdown-min-height: 3rem;
}

/* Container */
[data-bui-dropdown-base] .bui-dropdown__container {
    position: relative;
}

/* Trigger button */
[data-bui-dropdown-base] .bui-dropdown__trigger {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    cursor: pointer;
    min-height: calc(var(--bui-dropdown-min-height) * var(--bui-density-spacing-multiplier, 1));
    background-color: transparent;
    transition: var(--bui-dropdown-transition);
    text-align: left;
    border: none;
    font: inherit;
    color: inherit;
}

/* Value display */
[data-bui-dropdown-base] .bui-dropdown__value {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

[data-bui-dropdown-base] .bui-dropdown__placeholder {
    opacity: 0.6;
}

/* Arrow icon */
[data-bui-dropdown-base] .bui-dropdown__arrow {
    display: flex;
    align-items: center;
    transition: transform 200ms ease;
}

/* Overlay */
[data-bui-dropdown-base] .bui-dropdown__overlay {
    position: fixed;
    inset: 0;
    z-index: 999;
}

/* Menu */
[data-bui-dropdown-base] .bui-dropdown__menu {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    margin-top: 4px;
    background-color: var(--palette-surface);
    border: 1px solid var(--bui-dropdown-border-color);
    border-radius: var(--bui-dropdown-border-radius);
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    overflow: hidden;
    z-index: 1000;
    min-width: 100%;
}

/* Search */
[data-bui-dropdown-base] .bui-dropdown__search {
    padding: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}

[data-bui-dropdown-base] .bui-dropdown__search-input {
    width: 100%;
    padding: calc(0.375rem * var(--bui-density-spacing-multiplier, 1)) calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    border: 1px solid var(--bui-dropdown-border-color);
    border-radius: var(--bui-dropdown-border-radius);
    background: transparent;
    font: inherit;
    color: inherit;
}

[data-bui-dropdown-base] .bui-dropdown__search-input:focus {
    outline: none;
    border-color: var(--bui-dropdown-focus-border-color);
}

/* Select All */
[data-bui-dropdown-base] .bui-dropdown__select-all {
    display: flex;
    gap: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    padding: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}

[data-bui-dropdown-base] .bui-dropdown__select-all-button {
    flex: 1;
    padding: calc(0.375rem * var(--bui-density-spacing-multiplier, 1)) calc(0.75rem * var(--bui-density-spacing-multiplier, 1));
    background: none;
    border: 1px solid var(--bui-dropdown-border-color);
    border-radius: var(--bui-dropdown-border-radius);
    color: inherit;
    cursor: pointer;
    transition: background-color 150ms ease;
}

[data-bui-dropdown-base] .bui-dropdown__select-all-button:hover {
    background-color: rgba(0, 0, 0, 0.04);
}

[data-theme="dark"] [data-bui-dropdown-base] .bui-dropdown__select-all-button:hover {
    background-color: rgba(255, 255, 255, 0.08);
}

/* Option items */
[data-bui-dropdown-base] .bui-dropdown__option {
    display: flex;
    align-items: center;
    gap: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    padding: calc(var(--bui-dropdown-padding-y) * var(--bui-density-spacing-multiplier, 1)) calc(var(--bui-dropdown-padding-x) * var(--bui-density-spacing-multiplier, 1));
    cursor: pointer;
    transition: background-color 150ms ease;
}

[data-bui-dropdown-base] .bui-dropdown__option:hover:not(.bui-dropdown__option--disabled) {
    background-color: rgba(0, 0, 0, 0.04);
}

[data-theme="dark"] [data-bui-dropdown-base] .bui-dropdown__option:hover:not(.bui-dropdown__option--disabled) {
    background-color: rgba(255, 255, 255, 0.08);
}

[data-bui-dropdown-base] .bui-dropdown__option--selected {
    background-color: var(--palette-primary);
    color: var(--palette-primarycontrast);
}

[data-bui-dropdown-base] .bui-dropdown__option--selected:hover {
    filter: brightness(1.1);
}

[data-bui-dropdown-base] .bui-dropdown__option--focused {
    background-color: rgba(0, 0, 0, 0.08);
}

[data-bui-dropdown-base] .bui-dropdown__option--disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

[data-bui-dropdown-base] .bui-dropdown__no-options {
    padding: calc(1rem * var(--bui-density-spacing-multiplier, 1));
    text-align: center;
    opacity: 0.6;
}

/* ===== VARIANT: Outlined ===== */
[data-bui-dropdown-base][data-bui-variant="outlined"] .bui-dropdown__trigger {
    border: 1px solid var(--bui-dropdown-border-color);
    border-radius: var(--bui-dropdown-border-radius);
    padding: calc(var(--bui-dropdown-padding-y) * var(--bui-density-spacing-multiplier, 1)) calc(var(--bui-dropdown-padding-x) * var(--bui-density-spacing-multiplier, 1));
}

[data-bui-dropdown-base][data-bui-variant="outlined"] .bui-dropdown__trigger:hover:not(:disabled) {
    border-color: var(--bui-dropdown-hover-border-color);
}

[data-bui-dropdown-base][data-bui-variant="outlined"] .bui-dropdown__container--open .bui-dropdown__trigger {
    border-color: var(--bui-dropdown-focus-border-color);
    box-shadow: 0 0 0 1px var(--bui-dropdown-focus-border-color);
}

/* ===== VARIANT: Filled ===== */
[data-bui-dropdown-base][data-bui-variant="filled"] .bui-dropdown__trigger {
    background-color: rgba(0, 0, 0, 0.06);
    border-top-left-radius: var(--bui-dropdown-border-radius);
    border-top-right-radius: var(--bui-dropdown-border-radius);
    border-bottom: 2px solid transparent;
    padding: calc(var(--bui-dropdown-padding-y) * var(--bui-density-spacing-multiplier, 1)) calc(var(--bui-dropdown-padding-x) * var(--bui-density-spacing-multiplier, 1));
}

[data-bui-dropdown-base][data-bui-variant="filled"] .bui-dropdown__trigger:hover:not(:disabled) {
    background-color: rgba(0, 0, 0, 0.09);
}

[data-bui-dropdown-base][data-bui-variant="filled"] .bui-dropdown__container--open .bui-dropdown__trigger {
    border-bottom-color: var(--bui-dropdown-focus-border-color);
}

/* ===== VARIANT: Standard ===== */
[data-bui-dropdown-base][data-bui-variant="standard"] .bui-dropdown__trigger {
    border-bottom: 1px solid rgba(0, 0, 0, 0.42);
    padding: calc(var(--bui-dropdown-padding-y) * var(--bui-density-spacing-multiplier, 1)) 0;
}

[data-bui-dropdown-base][data-bui-variant="standard"] .bui-dropdown__trigger:hover:not(:disabled) {
    border-bottom-color: var(--bui-dropdown-hover-border-color);
}

[data-bui-dropdown-base][data-bui-variant="standard"] .bui-dropdown__container--open .bui-dropdown__trigger {
    border-bottom-color: var(--bui-dropdown-focus-border-color);
    border-bottom-width: 2px;
}

/* ===== STATES ===== */

/* Disabled */
[data-bui-dropdown-base][data-bui-disabled="true"] .bui-dropdown__trigger {
    cursor: not-allowed;
    opacity: 0.6;
}

/* Error */
[data-bui-dropdown-base][data-bui-error="true"] .bui-dropdown__trigger {
    border-color: var(--palette-error);
}

/* Full width */
[data-bui-dropdown-base][data-bui-fullwidth="true"] {
    width: 100%;
}

/* ===== PLACEMENT ===== */

/* Bottom */
[data-bui-dropdown-base][data-bui-placement="bottom"] .bui-dropdown__menu,
[data-bui-dropdown-base][data-bui-placement="bottomstart"] .bui-dropdown__menu,
[data-bui-dropdown-base][data-bui-placement="bottomend"] .bui-dropdown__menu {
    top: 100%;
    bottom: auto;
    margin-top: 4px;
    margin-bottom: 0;
}

/* Top */
[data-bui-dropdown-base][data-bui-placement="top"] .bui-dropdown__menu,
[data-bui-dropdown-base][data-bui-placement="topstart"] .bui-dropdown__menu,
[data-bui-dropdown-base][data-bui-placement="topend"] .bui-dropdown__menu {
    top: auto;
    bottom: 100%;
    margin-top: 0;
    margin-bottom: 4px;
}/* ============================================
   Dropdown Base Styles
   Shared by: BUIInputDropdown, BUIInputTreeDropdown
   ============================================ */

/* Base component setup */
[data-bui-dropdown-base] {
    display: flex;
    flex-direction: column;
    position: relative;
    --bui-dropdown-padding-y: 0.5rem;
    --bui-dropdown-padding-x: 0.75rem;
    --bui-dropdown-min-height: 2.5rem;
    --bui-dropdown-border-radius: 4px;
    --bui-dropdown-border-color: rgba(0, 0, 0, 0.23);
    --bui-dropdown-hover-border-color: var(--palette-backgroundcontrast);
    --bui-dropdown-focus-border-color: var(--palette-primary);
    --bui-dropdown-transition: all 200ms ease-in-out;
}

/* Size variants */
[data-bui-dropdown-base][data-bui-size="small"] {
    --bui-dropdown-padding-y: 0.375rem;
    --bui-dropdown-padding-x: 0.5rem;
    --bui-dropdown-min-height: 2rem;
}

[data-bui-dropdown-base][data-bui-size="large"] {
    --bui-dropdown-padding-y: 0.625rem;
    --bui-dropdown-padding-x: 1rem;
    --bui-dropdown-min-height: 3rem;
}

/* Container */
[data-bui-dropdown-base] .bui-dropdown__container {
    position: relative;
}

/* Trigger button */
[data-bui-dropdown-base] .bui-dropdown__trigger {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    cursor: pointer;
    min-height: calc(var(--bui-dropdown-min-height) * var(--bui-density-spacing-multiplier, 1));
    background-color: transparent;
    transition: var(--bui-dropdown-transition);
    text-align: left;
    border: none;
    font: inherit;
    color: inherit;
}

/* Value display */
[data-bui-dropdown-base] .bui-dropdown__value {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

[data-bui-dropdown-base] .bui-dropdown__placeholder {
    opacity: 0.6;
}

/* Arrow icon */
[data-bui-dropdown-base] .bui-dropdown__arrow {
    display: flex;
    align-items: center;
    transition: transform 200ms ease;
}

/* Overlay */
[data-bui-dropdown-base] .bui-dropdown__overlay {
    position: fixed;
    inset: 0;
    z-index: 999;
}

/* Menu */
[data-bui-dropdown-base] .bui-dropdown__menu {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    margin-top: 4px;
    background-color: var(--palette-surface);
    border: 1px solid var(--bui-dropdown-border-color);
    border-radius: var(--bui-dropdown-border-radius);
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    overflow: hidden;
    z-index: 1000;
    min-width: 100%;
}

/* Search */
[data-bui-dropdown-base] .bui-dropdown__search {
    padding: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}

[data-bui-dropdown-base] .bui-dropdown__search-input {
    width: 100%;
    padding: calc(0.375rem * var(--bui-density-spacing-multiplier, 1)) calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    border: 1px solid var(--bui-dropdown-border-color);
    border-radius: var(--bui-dropdown-border-radius);
    background: transparent;
    font: inherit;
    color: inherit;
}

[data-bui-dropdown-base] .bui-dropdown__search-input:focus {
    outline: none;
    border-color: var(--bui-dropdown-focus-border-color);
}

/* Select All */
[data-bui-dropdown-base] .bui-dropdown__select-all {
    display: flex;
    gap: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    padding: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}

[data-bui-dropdown-base] .bui-dropdown__select-all-button {
    flex: 1;
    padding: calc(0.375rem * var(--bui-density-spacing-multiplier, 1)) calc(0.75rem * var(--bui-density-spacing-multiplier, 1));
    background: none;
    border: 1px solid var(--bui-dropdown-border-color);
    border-radius: var(--bui-dropdown-border-radius);
    color: inherit;
    cursor: pointer;
    transition: background-color 150ms ease;
}

[data-bui-dropdown-base] .bui-dropdown__select-all-button:hover {
    background-color: rgba(0, 0, 0, 0.04);
}

[data-theme="dark"] [data-bui-dropdown-base] .bui-dropdown__select-all-button:hover {
    background-color: rgba(255, 255, 255, 0.08);
}

/* Option items */
[data-bui-dropdown-base] .bui-dropdown__option {
    display: flex;
    align-items: center;
    gap: calc(0.5rem * var(--bui-density-spacing-multiplier, 1));
    padding: calc(var(--bui-dropdown-padding-y) * var(--bui-density-spacing-multiplier, 1)) calc(var(--bui-dropdown-padding-x) * var(--bui-density-spacing-multiplier, 1));
    cursor: pointer;
    transition: background-color 150ms ease;
}

[data-bui-dropdown-base] .bui-dropdown__option:hover:not(.bui-dropdown__option--disabled) {
    background-color: rgba(0, 0, 0, 0.04);
}

[data-theme="dark"] [data-bui-dropdown-base] .bui-dropdown__option:hover:not(.bui-dropdown__option--disabled) {
    background-color: rgba(255, 255, 255, 0.08);
}

[data-bui-dropdown-base] .bui-dropdown__option--selected {
    background-color: var(--palette-primary);
    color: var(--palette-primarycontrast);
}

[data-bui-dropdown-base] .bui-dropdown__option--selected:hover {
    filter: brightness(1.1);
}

[data-bui-dropdown-base] .bui-dropdown__option--focused {
    background-color: rgba(0, 0, 0, 0.08);
}

[data-bui-dropdown-base] .bui-dropdown__option--disabled {
    opacity: 0.5;
    cursor: not-allowed;
}

[data-bui-dropdown-base] .bui-dropdown__no-options {
    padding: calc(1rem * var(--bui-density-spacing-multiplier, 1));
    text-align: center;
    opacity: 0.6;
}

/* ===== VARIANT: Outlined ===== */
[data-bui-dropdown-base][data-bui-variant="outlined"] .bui-dropdown__trigger {
    border: 1px solid var(--bui-dropdown-border-color);
    border-radius: var(--bui-dropdown-border-radius);
    padding: calc(var(--bui-dropdown-padding-y) * var(--bui-density-spacing-multiplier, 1)) calc(var(--bui-dropdown-padding-x) * var(--bui-density-spacing-multiplier, 1));
}

[data-bui-dropdown-base][data-bui-variant="outlined"] .bui-dropdown__trigger:hover:not(:disabled) {
    border-color: var(--bui-dropdown-hover-border-color);
}

[data-bui-dropdown-base][data-bui-variant="outlined"] .bui-dropdown__container--open .bui-dropdown__trigger {
    border-color: var(--bui-dropdown-focus-border-color);
    box-shadow: 0 0 0 1px var(--bui-dropdown-focus-border-color);
}

/* ===== VARIANT: Filled ===== */
[data-bui-dropdown-base][data-bui-variant="filled"] .bui-dropdown__trigger {
    background-color: rgba(0, 0, 0, 0.06);
    border-top-left-radius: var(--bui-dropdown-border-radius);
    border-top-right-radius: var(--bui-dropdown-border-radius);
    border-bottom: 2px solid transparent;
    padding: calc(var(--bui-dropdown-padding-y) * var(--bui-density-spacing-multiplier, 1)) calc(var(--bui-dropdown-padding-x) * var(--bui-density-spacing-multiplier, 1));
}

[data-bui-dropdown-base][data-bui-variant="filled"] .bui-dropdown__trigger:hover:not(:disabled) {
    background-color: rgba(0, 0, 0, 0.09);
}

[data-bui-dropdown-base][data-bui-variant="filled"] .bui-dropdown__container--open .bui-dropdown__trigger {
    border-bottom-color: var(--bui-dropdown-focus-border-color);
}

/* ===== VARIANT: Standard ===== */
[data-bui-dropdown-base][data-bui-variant="standard"] .bui-dropdown__trigger {
    border-bottom: 1px solid rgba(0, 0, 0, 0.42);
    padding: calc(var(--bui-dropdown-padding-y) * var(--bui-density-spacing-multiplier, 1)) 0;
}

[data-bui-dropdown-base][data-bui-variant="standard"] .bui-dropdown__trigger:hover:not(:disabled) {
    border-bottom-color: var(--bui-dropdown-hover-border-color);
}

[data-bui-dropdown-base][data-bui-variant="standard"] .bui-dropdown__container--open .bui-dropdown__trigger {
    border-bottom-color: var(--bui-dropdown-focus-border-color);
    border-bottom-width: 2px;
}

/* ===== STATES ===== */

/* Disabled */
[data-bui-dropdown-base][data-bui-disabled="true"] .bui-dropdown__trigger {
    cursor: not-allowed;
    opacity: 0.6;
}

/* Error */
[data-bui-dropdown-base][data-bui-error="true"] .bui-dropdown__trigger {
    border-color: var(--palette-error);
}

/* Full width */
[data-bui-dropdown-base][data-bui-fullwidth="true"] {
    width: 100%;
}

/* ===== PLACEMENT ===== */

/* Bottom */
[data-bui-dropdown-base][data-bui-placement="bottom"] .bui-dropdown__menu,
[data-bui-dropdown-base][data-bui-placement="bottomstart"] .bui-dropdown__menu,
[data-bui-dropdown-base][data-bui-placement="bottomend"] .bui-dropdown__menu {
    top: 100%;
    bottom: auto;
    margin-top: 4px;
    margin-bottom: 0;
}

/* Top */
[data-bui-dropdown-base][data-bui-placement="top"] .bui-dropdown__menu,
[data-bui-dropdown-base][data-bui-placement="topstart"] .bui-dropdown__menu,
[data-bui-dropdown-base][data-bui-placement="topend"] .bui-dropdown__menu {
    top: auto;
    bottom: 100%;
    margin-top: 0;
    margin-bottom: 4px;
}
""";
}