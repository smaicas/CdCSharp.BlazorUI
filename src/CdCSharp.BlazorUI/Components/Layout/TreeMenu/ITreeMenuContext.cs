using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace CdCSharp.BlazorUI.Components.Layout;

internal interface ITreeMenuContext
{
    TreeMenuOrientation Orientation { get; }
    TreeMenuTrigger Trigger { get; }
    TreeMenuExpandMode ExpandMode { get; }
    SizeEnum Size { get; }

    bool IsExpanded(string key);
    bool IsActive(string? href);
    Task ToggleAsync(string key);
    Task ExpandAsync(string key);
    Task CollapseAsync(string key);
    Task CollapseAllAsync();

    void RegisterItem(TreeMenuItemRegistration registration);
}

internal sealed class TreeMenuItemRegistration
{
    public required string Key { get; init; }
    public string? Text { get; init; }
    public string? Icon { get; init; }
    public string? Href { get; init; }
    public string? Target { get; init; }
    public NavLinkMatch Match { get; init; } = NavLinkMatch.Prefix;
    public bool Disabled { get; init; }
    public bool InitiallyExpanded { get; init; }
    public RenderFragment? NodeContent { get; init; }
    public RenderFragment? ChildContent { get; init; }
    public string? ParentKey { get; init; }
    public EventCallback OnClick { get; init; }
}