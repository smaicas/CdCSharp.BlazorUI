using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Core.Assets.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class PickerFamilyCssGenerator : IAssetGenerator
{
    public string FileName => "_picker-family.css";
    public string Name => "Picker Family";

    private static string V(string variable, string fallback) => $"var({variable}, {fallback})";

    public async Task<string> GetContent()
    {
        string picker = FeatureDefinitions.DataAttributes.PickerBase;
        string sizeMult = FeatureDefinitions.ComponentVariables.Size.Multiplier;
        string gap = FeatureDefinitions.ComponentVariables.Density.Gap;

        string row = FeatureDefinitions.CssClasses.Picker.Row;
        string title = FeatureDefinitions.CssClasses.Picker.Title;
        string btn = FeatureDefinitions.CssClasses.Picker.Btn;
        string btnIcon = FeatureDefinitions.CssClasses.Picker.BtnIcon;
        string grid = FeatureDefinitions.CssClasses.Picker.Grid;
        string cell = FeatureDefinitions.CssClasses.Picker.Cell;
        string cellSelected = FeatureDefinitions.CssClasses.Picker.CellSelected;
        string cellMuted = FeatureDefinitions.CssClasses.Picker.CellMuted;
        string input = FeatureDefinitions.CssClasses.Picker.Input;
        string separator = FeatureDefinitions.CssClasses.Picker.Separator;
        string slider = FeatureDefinitions.CssClasses.Picker.Slider;
        string preview = FeatureDefinitions.CssClasses.Picker.Preview;

        return $$"""
/* ========================================
   Picker Family Styles
   Auto-generated - Do not edit manually
   ======================================== */

bui-component[{{picker}}] {
    display: flex;
    flex-direction: column;
    gap: {{V(gap, "0.5rem")}};
    padding: calc(0.75rem * {{V(sizeMult, "1")}});
    background: var(--palette-surface);
    border: 1px solid var(--palette-border);
    border-radius: 8px;
    user-select: none;
    --_cell: calc(36px * {{V(sizeMult, "1")}});
    --_btn: calc(32px * {{V(sizeMult, "1")}});
}

/* Row */
bui-component[{{picker}}] .{{row}} {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: {{V(gap, "0.5rem")}};
}

/* Title */
bui-component[{{picker}}] .{{title}} {
    flex: 1;
    font-weight: 600;
    text-align: center;
}

/* Button */
bui-component[{{picker}}] .{{btn}} {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.25rem;
    height: var(--_btn);
    padding-inline: 0.75rem;
    border: 1px solid var(--palette-border);
    border-radius: 6px;
    background: transparent;
    color: inherit;
    font: inherit;
    font-weight: 500;
    cursor: pointer;
    transition: background-color 150ms ease;
}

bui-component[{{picker}}] .{{btn}}:hover {
    background: color-mix(in srgb, var(--palette-surfacecontrast) 8%, transparent);
}

bui-component[{{picker}}] .{{btn}}:active {
    transform: scale(0.96);
}

bui-component[{{picker}}] .{{btnIcon}} {
    width: var(--_btn);
    padding: 0;
}

/* Grid */
bui-component[{{picker}}] .{{grid}} {
    display: grid;
    grid-template-columns: repeat(7, var(--_cell));
    gap: 2px;
}

/* Cell */
bui-component[{{picker}}] .{{cell}} {
    display: flex;
    align-items: center;
    justify-content: center;
    width: var(--_cell);
    height: var(--_cell);
    border: none;
    border-radius: 50%;
    background: transparent;
    color: inherit;
    font: inherit;
    cursor: pointer;
    transition: background-color 150ms ease;
}

bui-component[{{picker}}] .{{cell}}:hover:not(.{{cellSelected}}) {
    background: color-mix(in srgb, var(--palette-surfacecontrast) 8%, transparent);
}

bui-component[{{picker}}] .{{cellSelected}} {
    background: var(--palette-primary);
    color: var(--palette-primarycontrast);
}

bui-component[{{picker}}] .{{cellMuted}} {
    opacity: 0.3;
}

/* Input */
bui-component[{{picker}}] .{{input}} {
    width: calc(3rem * {{V(sizeMult, "1")}});
    height: var(--_btn);
    padding: 0.25rem;
    border: 1px solid var(--palette-border);
    border-radius: 6px;
    background: transparent;
    color: inherit;
    font: inherit;
    font-weight: 600;
    font-variant-numeric: tabular-nums;
    text-align: center;
}

bui-component[{{picker}}] .{{input}}:focus {
    outline: none;
    border-color: var(--palette-primary);
}

/* Separator */
bui-component[{{picker}}] .{{separator}} {
    font-size: 1.25em;
    font-weight: 600;
    opacity: 0.5;
}

/* Slider */
bui-component[{{picker}}] .{{slider}} {
    position: relative;
    height: calc(14px * {{V(sizeMult, "1")}});
    border-radius: 7px;
    overflow: hidden;
}

bui-component[{{picker}}] .{{slider}} input[type="range"] {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    margin: 0;
    opacity: 0;
    cursor: pointer;
}

bui-component[{{picker}}] .{{slider}}::after {
    content: '';
    position: absolute;
    top: 50%;
    left: var(--value, 0%);
    width: 6px;
    height: calc(18px * {{V(sizeMult, "1")}});
    background: var(--palette-white);
    border: 1px solid var(--palette-border);
    border-radius: 3px;
    transform: translate(-50%, -50%);
    pointer-events: none;
    box-shadow: 0 1px 3px rgba(0,0,0,0.2);
}

/* Preview */
bui-component[{{picker}}] .{{preview}} {
    height: calc(28px * {{V(sizeMult, "1")}});
    border: 1px solid var(--palette-border);
    border-radius: 6px;
    background-image: 
        linear-gradient(45deg, var(--palette-border) 25%, transparent 25%),
        linear-gradient(-45deg, var(--palette-border) 25%, transparent 25%),
        linear-gradient(45deg, transparent 75%, var(--palette-border) 75%),
        linear-gradient(-45deg, transparent 75%, var(--palette-border) 75%);
    background-size: 8px 8px;
    background-position: 0 0, 0 4px, 4px -4px, -4px 0;
    background-color: var(--palette-background);
    overflow: hidden;
}

bui-component[{{picker}}] .{{preview}} > div {
    width: 100%;
    height: 100%;
}

/* ========================================
   KEYBOARD FOCUS INDICATORS
   ======================================== */

bui-component[data-bui-picker-base] .bui-picker__btn:focus-visible {
    outline: 2px solid var(--palette-highlight);
    outline-offset: -2px;
}

bui-component[data-bui-picker-base] .bui-picker__cell:focus-visible {
    outline: 2px solid var(--palette-highlight);
    outline-offset: -2px;
}

bui-component[data-bui-picker-base] .bui-picker__input:focus-visible {
    outline: 2px solid var(--palette-highlight);
    outline-offset: 2px;
}

/* Slider focus - box-shadow instead of outline due to overflow:hidden */
bui-component[{{picker}}] .{{slider}}:focus-within {
    box-shadow: 0 0 0 2px var(--palette-surface), 0 0 0 4px var(--palette-highlight);
}
""";
    }
}