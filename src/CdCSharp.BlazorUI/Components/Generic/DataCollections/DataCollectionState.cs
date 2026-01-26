namespace CdCSharp.BlazorUI.Components;

public sealed class DataCollectionState<TItem>
{
    private readonly HashSet<TItem> _selectedItems = [];

    public int CurrentPage { get; set; } = 1;
    public string FilterText { get; set; } = string.Empty;
    public int PageSize { get; set; } = 20;
    public IReadOnlySet<TItem> SelectedItems => _selectedItems;
    public string? SortColumn { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.None;

    public void ClearSelection() => _selectedItems.Clear();

    public bool IsSelected(TItem item) => _selectedItems.Contains(item);

    public void ResetPagination() => CurrentPage = 1;

    public void SelectAll(IEnumerable<TItem> items)
    {
        foreach (TItem item in items)
        {
            _selectedItems.Add(item);
        }
    }

    public void SelectItem(TItem item, SelectionMode mode)
    {
        if (mode == SelectionMode.Single)
        {
            _selectedItems.Clear();
        }

        if (_selectedItems.Contains(item))
        {
            _selectedItems.Remove(item);
        }
        else
        {
            _selectedItems.Add(item);
        }
    }

    public void ToggleSort(string columnName)
    {
        if (SortColumn == columnName)
        {
            SortDirection = SortDirection switch
            {
                SortDirection.None => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.Ascending
            };

            if (SortDirection == SortDirection.None)
            {
                SortColumn = null;
            }
        }
        else
        {
            SortColumn = columnName;
            SortDirection = SortDirection.Ascending;
        }
    }
}