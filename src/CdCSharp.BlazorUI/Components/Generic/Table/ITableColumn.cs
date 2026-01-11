namespace CdCSharp.BlazorUI.Components;

public interface ITableColumn<TItem>
{
    string? Header { get; }
    TableColumnAlign Align { get; }
    string? Width { get; }
    bool Visible { get; }
    object? GetValue(TItem item);
    string? FormatValue(object? value);
}
