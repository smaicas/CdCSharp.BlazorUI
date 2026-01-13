using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public sealed class DataGridColumnRegistration<TItem>
{
    public TableColumnAlign Align { get; init; } = TableColumnAlign.Left;
    public string? CellClass { get; init; }
    public Func<TItem, TItem, int>? CustomComparer { get; init; }
    public Func<TItem, string, bool>? CustomFilter { get; init; }
    public bool Filterable { get; init; }
    public string? Format { get; init; }
    public string? Header { get; init; }
    public string? HeaderClass { get; init; }
    public bool Sortable { get; init; }
    public RenderFragment<TItem>? Template { get; init; }
    public Func<TItem, object?>? ValueSelector { get; init; }
    public bool Visible { get; init; } = true;
    public string? Width { get; init; }
}