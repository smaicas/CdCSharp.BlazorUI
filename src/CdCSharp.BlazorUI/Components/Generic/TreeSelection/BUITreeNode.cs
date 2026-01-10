using CdCSharp.BlazorUI.Core.Components.Tree;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public class BUITreeNode<TItem> : TreeNodeComponentBase
{
    [Parameter] public TItem? Data { get; set; }

    protected override object? GetAdditionalData() => Data;
}

public class BUITreeNode : BUITreeNode<object>
{
}
