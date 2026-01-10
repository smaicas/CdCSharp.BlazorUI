using CdCSharp.BlazorUI.Core.Components.Tree;

namespace CdCSharp.BlazorUI.Components;

public enum TreeSelectionMode
{
    Single,
    Multiple
}

public sealed class TreeSelectionNodeContext<TItem>
{
    public required TreeNodeState<TItem> Node { get; init; }
    public bool IsSelected { get; init; }
    public bool IsExpanded { get; init; }
    public bool IsLoading { get; init; }
}

public sealed class TreeSelectionChangedEventArgs<TItem>
{
    public required HashSet<string> SelectedKeys { get; init; }
    public required IReadOnlyList<TreeNodeState<TItem>> SelectedNodes { get; init; }
    public required HashSet<string> PreviousKeys { get; init; }
    public required TreeNodeState<TItem> ChangedNode { get; init; }
}