namespace CdCSharp.BlazorUI.Components;

/// <summary>
/// Manages the internal state of a BUITable, including sorting, filtering, pagination, and selection.
/// </summary>
internal sealed class TableState<TItem>
{
    private readonly HashSet<TItem> _selectedItems = [];

    // === SORTING ===
    public string? SortColumn { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.None;

    // === FILTERING ===
    public string FilterText { get; set; } = string.Empty;

    // === PAGINATION ===
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    // === SELECTION ===
    public IReadOnlySet<TItem> SelectedItems => _selectedItems;

    /// <summary>
    /// Selects or deselects an item based on the selection mode.
    /// </summary>
    public void SelectItem(TItem item, SelectionMode mode)
    {
        if (mode == SelectionMode.None)
            return;

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

    /// <summary>
    /// Selects all provided items.
    /// </summary>
    public void SelectAll(IEnumerable<TItem> items)
    {
        _selectedItems.Clear();
        foreach (TItem? item in items)
        {
            _selectedItems.Add(item);
        }
    }

    /// <summary>
    /// Clears all selected items.
    /// </summary>
    public void ClearSelection()
    {
        _selectedItems.Clear();
    }

    /// <summary>
    /// Checks if an item is currently selected.
    /// </summary>
    public bool IsSelected(TItem item) => _selectedItems.Contains(item);

    /// <summary>
    /// Toggles the sort direction for a column.
    /// Cycles through: None -> Ascending -> Descending -> None
    /// </summary>
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

    /// <summary>
    /// Resets pagination to the first page.
    /// Typically called after filtering or changing page size.
    /// </summary>
    public void ResetPagination()
    {
        CurrentPage = 1;
    }
}