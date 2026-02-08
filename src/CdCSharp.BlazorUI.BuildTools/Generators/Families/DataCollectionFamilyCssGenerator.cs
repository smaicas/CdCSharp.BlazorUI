using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Core.Assets.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class DataCollectionFamilyCssGenerator : IAssetGenerator
{
    public string FileName => "_data-collection-family.css";
    public string Name => "Data Collection Family";

    public async Task<string> GetContent() => """
/* ========================================
   Data Collection Family Styles
   Auto-generated - Do not edit manually
   ======================================== */

[data-bui-data-collection] {
    --_dc-padding-x: 1rem;
    --_dc-padding-y: 0.75rem;
    --_dc-header-bg: color-mix(in oklab, var(--palette-surface) 95%, var(--palette-surfacecontrast));
    --_dc-hover-bg: color-mix(in oklab, var(--palette-surface) 92%, var(--palette-surfacecontrast));
    --_dc-selected-bg: color-mix(in oklab, var(--palette-surface) 90%, var(--palette-primary));
    --_dc-background: var(--bui-inline-background, var(--palette-surface));
    --_dc-border-radius: var(--bui-inline-border-radius, var(--bui-border-radius));
    --_dc-border: var(--bui-inline-border, var(--bui-border-width) var(--bui-border-style) var(--palette-border));
    --_dc-border-top: var(--bui-inline-border-top, var(--bui-inline-border, var(--bui-border-width) var(--bui-border-style) var(--palette-border)));
    --_dc-border-right: var(--bui-inline-border-right, var(--bui-inline-border, var(--bui-border-width) var(--bui-border-style) var(--palette-border)));
    --_dc-border-bottom: var(--bui-inline-border-bottom, var(--bui-inline-border, var(--bui-border-width) var(--bui-border-style) var(--palette-border)));
    --_dc-border-left: var(--bui-inline-border-left, var(--bui-inline-border, var(--bui-border-width) var(--bui-border-style) var(--palette-border)));
    
    display: block;
    width: 100%;
    background-color: var(--_dc-background);
    border: var(--_dc-border);
    border-top: var(--_dc-border-top);
    border-right: var(--_dc-border-right);
    border-bottom: var(--_dc-border-bottom);
    border-left: var(--_dc-border-left);
    border-radius: var(--_dc-border-radius);
}

/* === TOOLBAR === */
[data-bui-data-collection] .bui-dc__toolbar {
    display: flex;
    align-items: center;
    gap: calc(1rem * var(--bui-density-multiplier, 1));
    padding: var(--_dc-padding-y) var(--_dc-padding-x);
    background: var(--_dc-header-bg);
    border-bottom: 1px solid var(--palette-border);
}

[data-bui-data-collection] .bui-dc__toolbar-spacer {
    flex: 1;
}

/* === FILTER === */
[data-bui-data-collection] .bui-dc__filter {
    position: relative;
    display: flex;
    align-items: center;
    gap: calc(0.5rem * var(--bui-density-multiplier, 1));
    flex: 0 1 300px;
}

/* === SELECTION INFO === */
[data-bui-data-collection] .bui-dc__selection-info {
    display: flex;
    align-items: center;
    gap: calc(0.5rem * var(--bui-density-multiplier, 1));
    font-size: 0.875rem;
    color: var(--palette-primary);
}

/* === PAGE SIZE SELECTOR === */
[data-bui-data-collection] .bui-dc__page-size {
    display: flex;
    align-items: center;
    gap: calc(0.5rem * var(--bui-density-multiplier, 1));
    font-size: 0.875rem;
}

/* === PAGINATION === */
[data-bui-data-collection] .bui-dc__pagination {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: var(--_dc-padding-y) var(--_dc-padding-x);
    border-top: 1px solid var(--palette-border);
    background: var(--_dc-header-bg);
}

[data-bui-data-collection] .bui-dc__pagination-info {
    font-size: 0.875rem;
    color: var(--palette-surfacecontrast);
    opacity: 0.8;
}

[data-bui-data-collection] .bui-dc__pagination-controls {
    display: flex;
    align-items: center;
    gap: calc(0.25rem * var(--bui-density-multiplier, 1));
}

/* === CHECKBOX (shared) === */
[data-bui-data-collection] .bui-dc__checkbox {
    cursor: pointer;
    width: 1.125rem;
    height: 1.125rem;
    accent-color: var(--palette-primary);
}

/* === EMPTY STATE === */
[data-bui-data-collection] .bui-dc__empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: calc(0.5rem * var(--bui-density-multiplier, 1));
    padding: calc(var(--_dc-padding-y) * 4) var(--_dc-padding-x);
    color: var(--palette-surfacecontrast);
    opacity: 0.6;
    text-align: center;
}

[data-bui-data-collection] .bui-dc__empty-icon {
    font-size: 3rem;
    opacity: 0.5;
}

[data-bui-data-collection] .bui-dc__empty-text {
    margin: 0;
    font-size: 1rem;
}

/* === LOADING STATE === */
[data-bui-data-collection] .bui-dc__loading {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: calc(var(--_dc-padding-y) * 4) var(--_dc-padding-x);
}

/* === DENSITY: Padding overrides === */
[data-bui-data-collection][data-bui-density="compact"] {
    --_dc-padding-x: 0.5rem;
    --_dc-padding-y: 0.375rem;
}

[data-bui-data-collection][data-bui-density="comfortable"] {
    --_dc-padding-x: 1.5rem;
    --_dc-padding-y: 1rem;
}

/* === RESPONSIVE === */
@media (max-width: 768px) {
    [data-bui-data-collection] .bui-dc__toolbar {
        flex-wrap: wrap;
    }

    [data-bui-data-collection] .bui-dc__filter {
        flex: 1 1 100%;
        order: -1;
        margin-bottom: 0.5rem;
    }

    [data-bui-data-collection] .bui-dc__pagination {
        flex-direction: column;
        gap: 1rem;
        align-items: stretch;
    }

    [data-bui-data-collection] .bui-dc__pagination-controls {
        justify-content: center;
    }
}
""";
}