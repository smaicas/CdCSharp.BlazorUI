using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace CdCSharp.BlazorUI.Components.Layout;

internal sealed class TreeMenuItemState
{
    public required string Key { get; init; }
    public string? Text { get; init; }
    public string? Icon { get; init; }
    public string? Href { get; init; }
    public string? Target { get; init; }
    public NavLinkMatch Match { get; init; }
    public bool Disabled { get; init; }
    public bool HasChildren => ChildrenInternal.Count > 0;
    public int Depth { get; init; }
    public TreeMenuItemState? Parent { get; init; }
    public List<TreeMenuItemState> ChildrenInternal { get; } = [];
    public RenderFragment? CustomContent { get; init; }
    public EventCallback OnClick { get; init; }
}