using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Forms;

public sealed class TreeDropdownItemRegistration
{
    public required string Key { get; init; }
    public string? Text { get; init; }
    public string? Icon { get; init; }
    public bool IsDisabled { get; init; }
    public bool InitiallyExpanded { get; init; }
    public object? Value { get; init; }
    public RenderFragment? NodeContent { get; init; }
    public string? ParentKey { get; init; }
}
