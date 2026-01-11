namespace CdCSharp.BlazorUI.Components;

public sealed class DataGridSortEventArgs : EventArgs
{
    public required string ColumnName { get; init; }
    public required SortDirection Direction { get; init; }
}

public sealed class DataGridFilterEventArgs : EventArgs
{
    public required string FilterText { get; init; }
}

public sealed class DataGridPageChangeEventArgs : EventArgs
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}

public sealed class DataGridSelectionEventArgs<TItem> : EventArgs
{
    public required IReadOnlySet<TItem> SelectedItems { get; init; }
}