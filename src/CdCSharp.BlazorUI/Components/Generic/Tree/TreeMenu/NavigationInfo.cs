using Microsoft.AspNetCore.Components.Routing;

namespace CdCSharp.BlazorUI.Components;

public sealed class NavigationInfo
{
    public string? Href { get; init; }
    public string? Target { get; init; }
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;

    public bool HasNavigation => !string.IsNullOrEmpty(Href);
}