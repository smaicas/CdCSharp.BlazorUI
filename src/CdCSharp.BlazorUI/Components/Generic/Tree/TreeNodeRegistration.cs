using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public sealed class TreeNodeRegistration
{
    public required string Key { get; init; }
    public string? Text { get; init; }
    public string? Icon { get; init; }
    public bool Disabled { get; init; }
    public bool InitiallyExpanded { get; init; }
    public object? Data { get; init; }
    public RenderFragment? NodeContent { get; init; }
    public RenderFragment? ChildNodes { get; init; }
    public string? ParentKey { get; init; }
}
