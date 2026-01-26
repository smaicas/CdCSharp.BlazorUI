namespace CdCSharp.BlazorUI.Components;

public sealed class DataCollectionFilterEventArgs : EventArgs
{
    public required string FilterText { get; init; }
}

public sealed class DataCollectionPageChangeEventArgs : EventArgs
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}

public sealed class DataCollectionSortEventArgs : EventArgs
{
    public required string ColumnName { get; init; }
    public required SortDirection Direction { get; init; }
}

public sealed class DataCollectionSelectionEventArgs<TItem> : EventArgs
{
    public required IReadOnlySet<TItem> SelectedItems { get; init; }
}