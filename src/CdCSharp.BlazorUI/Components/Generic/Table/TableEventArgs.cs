namespace CdCSharp.BlazorUI.Components;

/// <summary>
/// Event arguments for table sort events.
/// </summary>
public sealed class TableSortEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the column that was sorted.
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// Gets the new sort direction.
    /// </summary>
    public required SortDirection Direction { get; init; }
}

/// <summary>
/// Event arguments for table filter events.
/// </summary>
public sealed class TableFilterEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current filter text.
    /// </summary>
    public required string FilterText { get; init; }
}

/// <summary>
/// Event arguments for table page change events.
/// </summary>
public sealed class TablePageChangeEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new page number (1-based).
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Gets the current page size.
    /// </summary>
    public required int PageSize { get; init; }
}

/// <summary>
/// Event arguments for table selection change events.
/// </summary>
public sealed class TableSelectionEventArgs<TItem> : EventArgs
{
    /// <summary>
    /// Gets the currently selected items.
    /// </summary>
    public required IReadOnlySet<TItem> SelectedItems { get; init; }
}