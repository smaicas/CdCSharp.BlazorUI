using CdCSharp.BlazorUI.Core.Abstractions.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CdCSharp.BlazorUI.Components;

public abstract class BUIDataCollectionBase<TItem, TComponent, TVariant>
    : BUIVariantComponentBase<TComponent, TVariant>,
      IHasDensity,
      IHasShadow,
      IHasBorder,
      IHasBackgroundColor,
      IDataCollectionFamilyComponent
    where TComponent : BUIDataCollectionBase<TItem, TComponent, TVariant>
    where TVariant : Variant
{
    protected readonly DataColumnRegistry<TItem> ColumnRegistry = new();
    protected readonly DataCollectionState<TItem> State = new();
    protected List<DataColumnRegistration<TItem>> RegisteredColumns = [];
    protected List<DataColumnRegistration<TItem>> VisibleColumns = [];
    protected List<TItem> FilteredItems = [];
    protected List<TItem> ProcessedItems = [];
    protected bool ColumnsBuilt;
    protected bool PreventRowKeyDown;
    protected int TotalPages;
    protected int PaginationStart;
    protected int PaginationEnd;

    [Parameter] public RenderFragment? Columns { get; set; }
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public DensityEnum Density { get; set; } = DensityEnum.Standard;
    [Parameter] public bool Hoverable { get; set; } = true;
    [Parameter] public ShadowStyle? Shadow { get; set; }

    [Parameter] public SelectionMode SelectionMode { get; set; } = SelectionMode.None;
    [Parameter] public HashSet<TItem>? SelectedItems { get; set; }
    [Parameter] public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    [Parameter] public bool Sortable { get; set; }
    [Parameter] public string? DefaultSortColumn { get; set; }
    [Parameter] public SortDirection? DefaultSortDirection { get; set; }

    [Parameter] public bool Filterable { get; set; }
    [Parameter] public string FilterPlaceholder { get; set; } = "Search...";
    [Parameter] public Func<TItem, string, bool>? CustomFilter { get; set; }

    [Parameter] public int? PageSize { get; set; }
    [Parameter] public int[] PageSizeOptions { get; set; } = [10, 20, 50, 100];
    [Parameter] public bool ShowPageSizeSelector { get; set; }

    [Parameter] public bool EnableVirtualization { get; set; }
    [Parameter] public string? Height { get; set; }

    [Parameter] public RenderFragment? EmptyContent { get; set; }
    [Parameter] public RenderFragment? LoadingContent { get; set; }
    [Parameter] public bool Loading { get; set; }

    [Parameter] public BorderStyle? Border { get; set; }
    [Parameter] public string? BackgroundColor { get; set; }

    [Parameter] public RowStylePattern? ItemPattern { get; set; }

    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }
    [Parameter] public EventCallback<DataCollectionSortEventArgs> OnSort { get; set; }
    [Parameter] public EventCallback<DataCollectionFilterEventArgs> OnFilter { get; set; }
    [Parameter] public EventCallback<DataCollectionPageChangeEventArgs> OnPageChange { get; set; }

    protected bool IsInteractiveRow => OnRowClick.HasDelegate || SelectionMode != SelectionMode.None;

    protected bool UsePerItemPatternStyles =>
        ItemPattern != null &&
        (!ItemPattern.IsCssExpressible || (EnableVirtualization && !string.IsNullOrEmpty(Height)));

    public override void BuildComponentDataAttributes(Dictionary<string, object> dataAttributes)
    {
        base.BuildComponentDataAttributes(dataAttributes);
        dataAttributes["data-bui-data-collection"] = "true";

        if (Hoverable)
        {
            dataAttributes["data-bui-hoverable"] = "true";
        }

        if (ItemPattern != null && !UsePerItemPatternStyles)
        {
            string? patternAttr = ItemPattern.GetPatternDataAttribute();
            if (patternAttr != null)
            {
                dataAttributes["data-bui-row-pattern"] = patternAttr;
            }
        }
    }

    public override void BuildComponentCssVariables(Dictionary<string, string> cssVariables)
    {
        base.BuildComponentCssVariables(cssVariables);

        if (ItemPattern != null && !UsePerItemPatternStyles)
        {
            foreach (KeyValuePair<string, string> kv in ItemPattern.GetContainerCssVariables())
                cssVariables[kv.Key] = kv.Value;
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (PageSize.HasValue)
        {
            State.PageSize = PageSize.Value;
        }

        if (!string.IsNullOrEmpty(DefaultSortColumn))
        {
            State.SortColumn = DefaultSortColumn;
            State.SortDirection = DefaultSortDirection ?? SortDirection.Ascending;
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (SelectedItems != null)
        {
            State.ClearSelection();
            foreach (TItem item in SelectedItems)
            {
                State.SelectItem(item, SelectionMode.Multiple);
            }
        }

        ProcessData();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender && Columns != null && !ColumnsBuilt)
        {
            RegisteredColumns = ColumnRegistry.Columns.ToList();
            ColumnRegistry.Clear();
            ColumnsBuilt = true;
            ProcessData();
            StateHasChanged();
        }
    }

    protected void ProcessData()
    {
        if (!ColumnsBuilt || Items == null)
        {
            FilteredItems = [];
            ProcessedItems = [];
            VisibleColumns = [];
            return;
        }

        VisibleColumns = RegisteredColumns.Where(c => c.Visible).ToList();
        FilteredItems = ApplyFilter(Items).ToList();
        IEnumerable<TItem> sorted = ApplySort(FilteredItems);
        CalculatePaginationInfo();
        ProcessedItems = ApplyPagination(sorted).ToList();
    }

    protected IEnumerable<TItem> ApplyFilter(IEnumerable<TItem> items)
    {
        if (string.IsNullOrWhiteSpace(State.FilterText))
            return items;

        if (CustomFilter != null)
            return items.Where(item => CustomFilter(item, State.FilterText));

        string searchText = State.FilterText.ToLowerInvariant();
        List<DataColumnRegistration<TItem>> filterableColumns = RegisteredColumns
            .Where(c => c.Filterable && c.ValueSelector != null)
            .ToList();

        if (!filterableColumns.Any())
            filterableColumns = RegisteredColumns.Where(c => c.ValueSelector != null).ToList();

        return items.Where(item =>
            filterableColumns.Any(col =>
            {
                object? value = col.ValueSelector?.Invoke(item);
                return value?.ToString()?.ToLowerInvariant().Contains(searchText) ?? false;
            }));
    }

    protected IEnumerable<TItem> ApplySort(IEnumerable<TItem> items)
    {
        if (string.IsNullOrEmpty(State.SortColumn))
            return items;

        DataColumnRegistration<TItem>? column = RegisteredColumns.FirstOrDefault(c => c.Header == State.SortColumn);
        if (column?.ValueSelector == null)
            return items;

        if (column.CustomComparer != null)
        {
            List<TItem> list = items.ToList();
            Comparison<TItem> comparison = State.SortDirection == SortDirection.Ascending
                ? new Comparison<TItem>(column.CustomComparer)
                : (x, y) => column.CustomComparer(y, x);
            list.Sort(comparison);
            return list;
        }

        return State.SortDirection == SortDirection.Ascending
            ? items.OrderBy(item => column.ValueSelector(item))
            : items.OrderByDescending(item => column.ValueSelector(item));
    }

    protected IEnumerable<TItem> ApplyPagination(IEnumerable<TItem> items)
    {
        if (!PageSize.HasValue)
            return items;

        int skip = (State.CurrentPage - 1) * State.PageSize;
        return items.Skip(skip).Take(State.PageSize);
    }

    protected void CalculatePaginationInfo()
    {
        if (!PageSize.HasValue || !FilteredItems.Any())
        {
            TotalPages = 0;
            PaginationStart = 0;
            PaginationEnd = 0;
            return;
        }

        TotalPages = (int)Math.Ceiling(FilteredItems.Count / (double)State.PageSize);

        if (State.CurrentPage > TotalPages)
            State.CurrentPage = TotalPages > 0 ? TotalPages : 1;

        PaginationStart = (State.CurrentPage - 1) * State.PageSize + 1;
        PaginationEnd = Math.Min(State.CurrentPage * State.PageSize, FilteredItems.Count);
    }

    protected async Task HandleSort(DataColumnRegistration<TItem> column)
    {
        if (!column.Sortable || string.IsNullOrEmpty(column.Header))
            return;

        State.ToggleSort(column.Header);
        State.ResetPagination();
        ProcessData();

        if (OnSort.HasDelegate)
        {
            await OnSort.InvokeAsync(new DataCollectionSortEventArgs
            {
                ColumnName = column.Header,
                Direction = State.SortDirection
            });
        }
    }

    protected async Task HandleSortSelectChange(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            State.SortColumn = null;
            State.SortDirection = SortDirection.None;
        }
        else
        {
            State.SortColumn = value;
            if (State.SortDirection == SortDirection.None)
                State.SortDirection = SortDirection.Ascending;
        }

        State.ResetPagination();
        ProcessData();

        if (OnSort.HasDelegate && !string.IsNullOrEmpty(State.SortColumn))
        {
            await OnSort.InvokeAsync(new DataCollectionSortEventArgs
            {
                ColumnName = State.SortColumn,
                Direction = State.SortDirection
            });
        }
    }

    protected async Task ToggleSortDirectionClicked(MouseEventArgs e)
    {
        State.SortDirection = State.SortDirection == SortDirection.Ascending
            ? SortDirection.Descending
            : SortDirection.Ascending;
        ProcessData();

        if (OnSort.HasDelegate && !string.IsNullOrEmpty(State.SortColumn))
        {
            await OnSort.InvokeAsync(new DataCollectionSortEventArgs
            {
                ColumnName = State.SortColumn,
                Direction = State.SortDirection
            });
        }
    }

    protected async Task HandleFilterChange(ChangeEventArgs e)
    {
        State.FilterText = e.Value?.ToString() ?? string.Empty;
        State.ResetPagination();
        ProcessData();

        if (OnFilter.HasDelegate)
        {
            await OnFilter.InvokeAsync(new DataCollectionFilterEventArgs
            {
                FilterText = State.FilterText
            });
        }
    }

    protected async Task HandleFilterInputChange(string? value)
    {
        await HandleFilterChange(new ChangeEventArgs { Value = value });
    }

    protected async Task ClearFilterClicked(MouseEventArgs e) => ClearFilter();

    protected void ClearFilter()
    {
        State.FilterText = string.Empty;
        State.ResetPagination();
        ProcessData();
    }

    protected async Task HandlePageSizeSelectChange(string? value)
    {
        if (int.TryParse(value, out int size))
        {
            await HandlePageSizeChange(new ChangeEventArgs { Value = size });
        }
    }

    protected async Task HandleSelectRow(TItem item)
    {
        State.SelectItem(item, SelectionMode);
        await NotifySelectionChanged();
    }

    protected async Task HandleSelectAll(ChangeEventArgs e)
    {
        if (e.Value is bool isChecked && isChecked)
            State.SelectAll(ProcessedItems);
        else
            State.ClearSelection();

        await NotifySelectionChanged();
    }

    protected async Task ClearSelectionClicked(MouseEventArgs e) => await ClearSelection();

    protected async Task ClearSelection()
    {
        State.ClearSelection();
        await NotifySelectionChanged();
    }

    protected bool IsAllSelected()
    {
        return ProcessedItems.Any() && ProcessedItems.All(State.IsSelected);
    }

    protected async Task NotifySelectionChanged()
    {
        if (SelectedItemsChanged.HasDelegate)
        {
            await SelectedItemsChanged.InvokeAsync([.. State.SelectedItems]);
        }
    }

    protected async Task HandleRowClick(TItem item)
    {
        if (SelectionMode != SelectionMode.None)
        {
            await HandleSelectRow(item);
        }

        if (OnRowClick.HasDelegate)
        {
            await OnRowClick.InvokeAsync(item);
        }
    }

    protected async Task HandleRowKeyDown(KeyboardEventArgs e, TItem item)
    {
        PreventRowKeyDown = false;

        if (e.Key is "Enter" or " ")
        {
            PreventRowKeyDown = true;
            await HandleRowClick(item);
        }
    }

    protected async Task ChangePage(int page)
    {
        if (page < 1 || page > TotalPages)
            return;

        State.CurrentPage = page;
        ProcessData();

        if (OnPageChange.HasDelegate)
        {
            await OnPageChange.InvokeAsync(new DataCollectionPageChangeEventArgs
            {
                Page = page,
                PageSize = State.PageSize
            });
        }
    }

    protected async Task HandlePageSizeChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int newSize))
        {
            State.PageSize = newSize;
            State.ResetPagination();
            ProcessData();

            if (OnPageChange.HasDelegate)
            {
                await OnPageChange.InvokeAsync(new DataCollectionPageChangeEventArgs
                {
                    Page = State.CurrentPage,
                    PageSize = newSize
                });
            }
        }
    }

    protected string? GetItemPatternStyle(int index)
    {
        if (!UsePerItemPatternStyles) return null;
        return ItemPattern?.GetItemInlineStyle(index);
    }

    protected IEnumerable<int> GetVisiblePages()
    {
        const int maxVisible = 5;
        int start = Math.Max(1, State.CurrentPage - maxVisible / 2);
        int end = Math.Min(TotalPages, start + maxVisible - 1);

        if (end - start + 1 < maxVisible)
            start = Math.Max(1, end - maxVisible + 1);

        return Enumerable.Range(start, end - start + 1);
    }

    protected static string GetAlignClass(ColumnAlign align, string prefix) => align switch
    {
        ColumnAlign.Center => $"{prefix}--center",
        ColumnAlign.Right => $"{prefix}--right",
        _ => string.Empty
    };

    protected static string FormatValue(object? value, string? format)
    {
        if (value == null) return string.Empty;

        if (!string.IsNullOrEmpty(format) && value is IFormattable formattable)
            return formattable.ToString(format, null);

        return value.ToString() ?? string.Empty;
    }

    protected string? GetAriaSort(DataColumnRegistration<TItem> col)
    {
        if (!col.Sortable || State.SortColumn != col.Header)
            return null;

        return State.SortDirection == SortDirection.Ascending ? "ascending" : "descending";
    }

    public void SortBy(string columnName, SortDirection direction)
    {
        State.SortColumn = columnName;
        State.SortDirection = direction;
        ProcessData();
        StateHasChanged();
    }

    public void Filter(string filterText)
    {
        State.FilterText = filterText;
        State.ResetPagination();
        ProcessData();
        StateHasChanged();
    }

    public void GoToPage(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            State.CurrentPage = page;
            ProcessData();
            StateHasChanged();
        }
    }

    public IReadOnlySet<TItem> GetSelectedItems() => State.SelectedItems;
}