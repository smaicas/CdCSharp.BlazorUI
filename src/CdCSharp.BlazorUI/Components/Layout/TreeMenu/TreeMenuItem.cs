using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace CdCSharp.BlazorUI.Components.Layout.TreeMenu;

public sealed class TreeMenuItem
{
    public string? Href { get; init; }
    public string? Target { get; init; }
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;
    public EventCallback OnClick { get; init; }
}
