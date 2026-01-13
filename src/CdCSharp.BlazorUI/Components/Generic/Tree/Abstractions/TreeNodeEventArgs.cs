namespace CdCSharp.BlazorUI.Components;

public sealed class TreeNodeEventArgs<TNode> : EventArgs
    where TNode : ITreeNode
{
    public int Depth { get; init; }
    public bool IsExpanded { get; init; }
    public string Key { get; init; }
    public TNode Node { get; init; }
}