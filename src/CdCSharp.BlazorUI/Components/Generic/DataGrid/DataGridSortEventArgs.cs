namespace CdCSharp.BlazorUI.Components;

public sealed class DataGridFilterEventArgs : EventArgs
{
    public string FilterText { get; init; }
}

public sealed class DataGridPageChangeEventArgs : EventArgs
{
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public sealed class DataGridSortEventArgs : EventArgs
{
    public string ColumnName { get; init; }
    public SortDirection Direction { get; init; }
}

public sealed class DataGridSelectionEventArgs<TItem> : EventArgs
{
    public IReadOnlySet<TItem> SelectedItems { get; init; }
}