using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public sealed class TreeNodeBuildContext<TItem>
{
    public object? AdditionalData { get; init; }
    public RenderFragment? CustomContent { get; init; }
    public int Depth { get; init; }
    public bool HasChildren { get; init; }
    public string? Icon { get; init; }
    public bool IsDisabled { get; init; }
    public TItem? Item { get; init; }
    public required string Key { get; init; }
    public object? Parent { get; init; }
    public string? Text { get; init; }
}