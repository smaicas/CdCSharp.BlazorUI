using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public class TableColumnRegistration<TItem>
{
    public string? Header { get; init; }
    public Func<TItem, object?>? ValueSelector { get; init; }
    public RenderFragment<TItem>? Template { get; init; }
    public string? Format { get; init; }
    public TableColumnAlign Align { get; init; } = TableColumnAlign.Left;
    public string? Width { get; init; }
    public string? HeaderClass { get; init; }
    public string? CellClass { get; init; }
    public bool Visible { get; init; } = true;
}