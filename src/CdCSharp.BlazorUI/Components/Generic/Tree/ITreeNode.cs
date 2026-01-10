namespace CdCSharp.BlazorUI.Components;

public interface ITreeNode
{
    string Key { get; }
    string? Text { get; }
    string? Icon { get; }
    bool Disabled { get; }
    bool HasChildren { get; }
    int Depth { get; }
    ITreeNode? Parent { get; }
    IReadOnlyList<ITreeNode> Children { get; }
}

public interface ITreeNode<TItem> : ITreeNode
{
    TItem? Item { get; }
    new ITreeNode<TItem>? Parent { get; }
    new IReadOnlyList<ITreeNode<TItem>> Children { get; }
}

public interface ITreeContext<TItem>
{
    bool IsExpanded(string key);
    bool IsLoading(string key);
    Task ToggleExpandAsync(ITreeNode<TItem> node);
    Task ExpandAsync(string key);
    Task CollapseAsync(string key);
}

internal interface ITreeNodeRegistry
{
    void RegisterNode(TreeNodeRegistration registration);
}