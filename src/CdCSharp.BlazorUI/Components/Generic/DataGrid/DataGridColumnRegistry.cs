namespace CdCSharp.BlazorUI.Components;

internal interface IDataGridColumnRegistry<TItem>
{
    void RegisterColumn(DataGridColumnRegistration<TItem> column);
}

internal sealed class DataGridColumnRegistry<TItem> : IDataGridColumnRegistry<TItem>
{
    private readonly List<DataGridColumnRegistration<TItem>> _columns = [];

    public void Clear() => _columns.Clear();

    public IReadOnlyList<DataGridColumnRegistration<TItem>> GetColumns()
        => _columns.ToList();

    public void RegisterColumn(DataGridColumnRegistration<TItem> column)
                => _columns.Add(column);
}