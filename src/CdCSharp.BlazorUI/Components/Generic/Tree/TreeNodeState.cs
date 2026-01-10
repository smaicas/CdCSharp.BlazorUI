using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

internal sealed class TreeNodeState<TItem> : ITreeNode<TItem>
{
    public required string Key { get; init; }
    public TItem? Item { get; init; }
    public string? Text { get; init; }
    public string? Icon { get; init; }
    public bool Disabled { get; init; }
    public bool HasChildren { get; set; }
    public int Depth { get; init; }
    public TreeNodeState<TItem>? Parent { get; init; }
    public List<TreeNodeState<TItem>> ChildrenInternal { get; set; } = [];
    public RenderFragment? CustomContent { get; init; }
    public object? AdditionalData { get; init; }

    // ITreeNode implementation
    ITreeNode? ITreeNode.Parent => Parent;
    IReadOnlyList<ITreeNode> ITreeNode.Children => ChildrenInternal;

    // ITreeNode<TItem> implementation
    ITreeNode<TItem>? ITreeNode<TItem>.Parent => Parent;
    IReadOnlyList<ITreeNode<TItem>> ITreeNode<TItem>.Children => ChildrenInternal;
}
