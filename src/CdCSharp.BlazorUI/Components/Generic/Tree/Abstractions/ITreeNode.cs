namespace CdCSharp.BlazorUI.Components;

public interface ITreeNode
{
    string Key { get; }
    string? Text { get; }
    string? Icon { get; }
    int Depth { get; }
    bool HasChildren { get; }
    bool IsDisabled { get; }
    ITreeNode? Parent { get; }
    IReadOnlyList<ITreeNode> Children { get; }
}

public interface ITreeNode<TItem> : ITreeNode
{
    TItem? Item { get; }
    new ITreeNode<TItem>? Parent { get; }
    new IReadOnlyList<ITreeNode<TItem>> Children { get; }
}