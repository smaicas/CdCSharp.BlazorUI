namespace CdCSharp.BlazorUI.Components;

public sealed class TreeNodeEventArgs<TNode> : EventArgs
    where TNode : ITreeNode
{
    public required TNode Node { get; init; }
    public required string Key { get; init; }
    public int Depth { get; init; }
    public bool IsExpanded { get; init; }
}