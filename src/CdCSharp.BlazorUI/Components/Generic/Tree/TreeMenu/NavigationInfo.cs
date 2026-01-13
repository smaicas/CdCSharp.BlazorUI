using Microsoft.AspNetCore.Components.Routing;

namespace CdCSharp.BlazorUI.Components;

public sealed class NavigationInfo
{
    public bool HasNavigation => !string.IsNullOrEmpty(Href);
    public string? Href { get; init; }
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;
    public string? Target { get; init; }
}