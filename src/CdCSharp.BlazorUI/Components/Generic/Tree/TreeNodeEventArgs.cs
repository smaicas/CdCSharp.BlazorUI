namespace CdCSharp.BlazorUI.Components;

public sealed class TreeNodeEventArgs<TItem> : EventArgs
{
    public required ITreeNode<TItem> Node { get; init; }
    public required string Key { get; init; }
    public TItem? Item { get; init; }
    public int Depth { get; init; }
    public bool IsExpanded { get; init; }
}
