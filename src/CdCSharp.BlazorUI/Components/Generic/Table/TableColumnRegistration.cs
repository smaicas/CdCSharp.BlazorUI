using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

/// <summary>
/// Represents the registration data for a table column, including display, behavior, and interaction settings.
/// </summary>
public sealed class TableColumnRegistration<TItem>
{
    // === BASIC PROPERTIES ===

    /// <summary>
    /// Gets the header text displayed in the column header.
    /// </summary>
    public string? Header { get; init; }

    /// <summary>
    /// Gets the function that extracts the value from an item for this column.
    /// </summary>
    public Func<TItem, object?>? ValueSelector { get; init; }

    /// <summary>
    /// Gets the custom template for rendering cell content.
    /// When provided, this takes precedence over ValueSelector.
    /// </summary>
    public RenderFragment<TItem>? Template { get; init; }

    /// <summary>
    /// Gets the format string for displaying values (e.g., "C2" for currency, "d" for date).
    /// Only applies when the value implements IFormattable.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the text alignment for this column.
    /// </summary>
    public TableColumnAlign Align { get; init; } = TableColumnAlign.Left;

    /// <summary>
    /// Gets the width specification for this column (e.g., "100px", "20%", "auto").
    /// </summary>
    public string? Width { get; init; }

    // === STYLING ===

    /// <summary>
    /// Gets the CSS class applied to the header cell.
    /// </summary>
    public string? HeaderClass { get; init; }

    /// <summary>
    /// Gets the CSS class applied to body cells in this column.
    /// </summary>
    public string? CellClass { get; init; }

    /// <summary>
    /// Gets whether this column should be displayed.
    /// </summary>
    public bool Visible { get; init; } = true;

    // === INTERACTIVE FEATURES ===

    /// <summary>
    /// Gets whether this column can be sorted.
    /// Only effective when the parent table has Sortable=true.
    /// </summary>
    public bool Sortable { get; init; }

    /// <summary>
    /// Gets whether this column's values should be included in filtering.
    /// Only effective when the parent table has Filterable=true.
    /// </summary>
    public bool Filterable { get; init; } = true;

    /// <summary>
    /// Gets a custom comparer for sorting this column.
    /// If null, default comparison using ValueSelector is used.
    /// </summary>
    public Comparison<TItem>? CustomComparer { get; init; }

    /// <summary>
    /// Gets a custom filter function for this column.
    /// Parameters: (item, filterText) => bool
    /// If null, default string Contains comparison is used.
    /// </summary>
    public Func<TItem, string, bool>? CustomFilter { get; init; }
}