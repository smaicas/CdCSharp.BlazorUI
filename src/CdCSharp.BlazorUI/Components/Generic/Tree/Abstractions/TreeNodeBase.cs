using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public abstract class TreeNodeBase<TItem, TSelf> : ITreeNode<TItem>
    where TSelf : TreeNodeBase<TItem, TSelf>
{
    public required string Key { get; init; }
    public TItem? Item { get; init; }
    public string? Text { get; init; }
    public string? Icon { get; init; }
    public bool IsDisabled { get; init; }
    public int Depth { get; init; }
    public TSelf? ParentNode { get; init; }
    public RenderFragment? CustomContent { get; init; }
    public object? AdditionalData { get; init; }

    internal List<TSelf> ChildrenInternal { get; } = [];
    internal bool HasChildrenFlag { get; set; }

    public bool HasChildren => HasChildrenFlag || ChildrenInternal.Count > 0;

    ITreeNode? ITreeNode.Parent => ParentNode;
    IReadOnlyList<ITreeNode> ITreeNode.Children => ChildrenInternal;
    ITreeNode<TItem>? ITreeNode<TItem>.Parent => ParentNode;
    IReadOnlyList<ITreeNode<TItem>> ITreeNode<TItem>.Children => ChildrenInternal;
}