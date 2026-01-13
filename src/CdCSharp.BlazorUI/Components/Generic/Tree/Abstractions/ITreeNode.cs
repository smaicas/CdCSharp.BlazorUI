namespace CdCSharp.BlazorUI.Components;

public interface ITreeNode
{
    IReadOnlyList<ITreeNode> Children { get; }
    int Depth { get; }
    bool HasChildren { get; }
    string? Icon { get; }
    bool IsDisabled { get; }
    string Key { get; }
    ITreeNode? Parent { get; }
    string? Text { get; }
}

public interface ITreeNode<TItem> : ITreeNode
{
    new IReadOnlyList<ITreeNode<TItem>> Children { get; }
    TItem? Item { get; }
    new ITreeNode<TItem>? Parent { get; }
}