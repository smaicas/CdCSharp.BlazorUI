using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Components.Tree;

/// <summary>
/// Represents the internal state of a tree node.
/// Implements ITreeNode interfaces for public API exposure.
/// </summary>
public sealed class TreeNodeState<TItem> : ITreeNode<TItem>
{
    public required string Key { get; init; }

    // Data-bound mode
    public TItem? Item { get; init; }

    // Declarative mode
    public string? Text { get; init; }
    public string? Icon { get; init; }
    public RenderFragment? CustomContent { get; init; }
    public object? AdditionalData { get; init; }

    // Common
    public bool Disabled { get; init; }
    public bool HasChildren { get; set; }
    public int Depth { get; init; }

    public TreeNodeState<TItem>? Parent { get; init; }
    public List<TreeNodeState<TItem>> ChildrenInternal { get; set; } = [];

    // ===== ITreeNode Implementation =====

    ITreeNode? ITreeNode.Parent => Parent;
    IReadOnlyList<ITreeNode> ITreeNode.Children => ChildrenInternal;

    // ===== ITreeNode<TItem> Implementation =====

    ITreeNode<TItem>? ITreeNode<TItem>.Parent => Parent;
    IReadOnlyList<ITreeNode<TItem>> ITreeNode<TItem>.Children => ChildrenInternal;
}

/// <summary>
/// Public interface for accessing tree node information.
/// Non-generic version for use when type is not needed.
/// </summary>
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

/// <summary>
/// Generic interface for accessing tree node information with typed item data.
/// </summary>
public interface ITreeNode<TItem> : ITreeNode
{
    TItem? Item { get; }
    new ITreeNode<TItem>? Parent { get; }
    new IReadOnlyList<ITreeNode<TItem>> Children { get; }
}