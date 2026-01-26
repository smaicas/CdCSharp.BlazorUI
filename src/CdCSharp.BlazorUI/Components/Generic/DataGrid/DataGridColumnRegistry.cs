namespace CdCSharp.BlazorUI.Components;

internal interface IDataGridColumnRegistry<TItem>
{
    IReadOnlyList<DataGridColumnRegistration<TItem>> Columns { get; }
    void RegisterColumn(DataGridColumnRegistration<TItem> column);
}

internal sealed class DataGridColumnRegistry<TItem> : IDataGridColumnRegistry<TItem>
{
    private readonly List<DataGridColumnRegistration<TItem>> _columns = [];

    public void Clear() => _columns.Clear();

    public IReadOnlyList<DataGridColumnRegistration<TItem>> Columns => _columns;

    public void RegisterColumn(DataGridColumnRegistration<TItem> column)
                => _columns.Add(column);
}