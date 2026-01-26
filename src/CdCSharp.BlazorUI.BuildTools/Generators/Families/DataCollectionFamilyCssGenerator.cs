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

/* === SHARED VARIABLES === */
[data-bui-data-collection] {
    --_dc-padding-x: 1rem;
    --_dc-padding-y: 0.75rem;
    --_dc-header-bg: rgba(0, 0, 0, 0.02);
    --_dc-border-color: rgba(0, 0, 0, 0.12);
    --_dc-hover-bg: rgba(0, 0, 0, 0.04);
    --_dc-selected-bg: rgba(25, 118, 210, 0.08);
    --_dc-toolbar-bg: rgba(0, 0, 0, 0.02);
    display: block;
    width: 100%;
}

[data-theme="dark"] [data-bui-data-collection] {
    --_dc-header-bg: rgba(255, 255, 255, 0.05);
    --_dc-border-color: rgba(255, 255, 255, 0.12);
    --_dc-hover-bg: rgba(255, 255, 255, 0.08);
    --_dc-selected-bg: rgba(25, 118, 210, 0.15);
    --_dc-toolbar-bg: rgba(255, 255, 255, 0.02);
}

/* === TOOLBAR === */
[data-bui-data-collection] .bui-dc__toolbar {
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: var(--_dc-padding-y) var(--_dc-padding-x);
    background: var(--_dc-toolbar-bg);
    border-bottom: 1px solid var(--_dc-border-color);
}

[data-bui-data-collection] .bui-dc__toolbar-spacer {
    flex: 1;
}

/* === FILTER === */
[data-bui-data-collection] .bui-dc__filter {
    position: relative;
    display: flex;
    align-items: center;
    flex: 0 1 300px;
}

/* === SELECTION INFO === */
[data-bui-data-collection] .bui-dc__selection-info {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.875rem;
    color: var(--palette-primary);
}

/* === PAGE SIZE SELECTOR === */
[data-bui-data-collection] .bui-dc__page-size {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.875rem;
}

/* === PAGINATION === */
[data-bui-data-collection] .bui-dc__pagination {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: var(--_dc-padding-y) var(--_dc-padding-x);
    border-top: 1px solid var(--_dc-border-color);
    background: var(--_dc-toolbar-bg);
}

[data-bui-data-collection] .bui-dc__pagination-info {
    font-size: 0.875rem;
    color: var(--palette-surfacecontrast);
    opacity: 0.8;
}

[data-bui-data-collection] .bui-dc__pagination-controls {
    display: flex;
    align-items: center;
    gap: 0.25rem;
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
    gap: 0.5rem;
    padding: 3rem 1rem;
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
    padding: 3rem 1rem;
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