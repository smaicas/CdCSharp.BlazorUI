namespace CdCSharp.BlazorUI.Components;

public sealed class DataGridColumnRegistration<TItem> : TableColumnRegistration<TItem>
{
    public bool Sortable { get; init; }
    public bool Filterable { get; init; }
    public Func<TItem, TItem, int>? CustomComparer { get; init; }
    public Func<TItem, string, bool>? CustomFilter { get; init; }
}