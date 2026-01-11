namespace CdCSharp.BlazorUI.Components;

internal sealed class DataGridState<TItem>
{
    private readonly HashSet<TItem> _selectedItems = [];

    public string? SortColumn { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.None;
    public string FilterText { get; set; } = string.Empty;
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public IReadOnlySet<TItem> SelectedItems => _selectedItems;

    public void SelectItem(TItem item, SelectionMode mode)
    {
        if (mode == SelectionMode.Single)
        {
            _selectedItems.Clear();
            _selectedItems.Add(item);
        }
        else if (mode == SelectionMode.Multiple)
        {
            if (_selectedItems.Contains(item))
                _selectedItems.Remove(item);
            else
                _selectedItems.Add(item);
        }
    }

    public void SelectAll(IEnumerable<TItem> items)
    {
        _selectedItems.Clear();
        foreach (TItem? item in items)
        {
            _selectedItems.Add(item);
        }
    }

    public void ClearSelection()
    {
        _selectedItems.Clear();
    }

    public bool IsSelected(TItem item) => _selectedItems.Contains(item);

    public void ToggleSort(string columnName)
    {
        if (SortColumn == columnName)
        {
            SortDirection = SortDirection switch
            {
                SortDirection.None => SortDirection.Ascending,
                SortDirection.Ascending => SortDirection.Descending,
                SortDirection.Descending => SortDirection.None,
                _ => SortDirection.None
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

    public void ResetPagination()
    {
        CurrentPage = 1;
    }
}