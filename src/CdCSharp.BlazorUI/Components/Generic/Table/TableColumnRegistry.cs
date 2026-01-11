namespace CdCSharp.BlazorUI.Components;

internal interface ITableColumnRegistry<TItem>
{
    void RegisterColumn(TableColumnRegistration<TItem> column);
}

internal sealed class TableColumnRegistry<TItem> : ITableColumnRegistry<TItem>
{
    private readonly List<TableColumnRegistration<TItem>> _columns = [];

    public void RegisterColumn(TableColumnRegistration<TItem> column)
        => _columns.Add(column);

    public IReadOnlyList<TableColumnRegistration<TItem>> GetColumns()
        => _columns.ToList(); // Return copy to prevent external modification

    public void Clear() => _columns.Clear();
}