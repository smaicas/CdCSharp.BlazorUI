using CdCSharp.BlazorUI.Core.Components.Selection;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public sealed class TreeSelectionNode<TItem> : TreeNodeBase<TItem, TreeSelectionNode<TItem>>, IHierarchicalSelectionOption
{
    object? ISelectionOption.Value => Item;

    string ISelectionOption.DisplayText => Text ?? Item?.ToString() ?? Key;

    bool ISelectionOption.IsDisabled => IsDisabled;

    RenderFragment? ISelectionOption.Content => CustomContent;

    string IHierarchicalSelectionOption.Key => Key;

    int IHierarchicalSelectionOption.Depth => Depth;

    IHierarchicalSelectionOption? IHierarchicalSelectionOption.Parent => ParentNode;

    IReadOnlyList<IHierarchicalSelectionOption> IHierarchicalSelectionOption.Children =>
        ChildrenInternal.Cast<IHierarchicalSelectionOption>().ToList();
}