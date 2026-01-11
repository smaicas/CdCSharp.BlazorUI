using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Forms;

public sealed class TreeDropdownNode<TValue>
{
    public required string Key { get; init; }
    public TValue? Item { get; init; }
    public required string Text { get; init; }
    public string? Icon { get; init; }
    public bool IsDisabled { get; init; }
    public bool HasChildren { get; set; }
    public int Depth { get; init; }
    public TreeDropdownNode<TValue>? Parent { get; init; }
    public RenderFragment? CustomContent { get; init; }
    public List<TreeDropdownNode<TValue>> Children { get; } = [];
}
