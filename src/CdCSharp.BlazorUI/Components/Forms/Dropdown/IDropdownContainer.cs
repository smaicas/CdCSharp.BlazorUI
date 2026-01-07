using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Forms.Dropdown;

public interface IDropdownContainer
{
    bool IsMultiple { get; }
    Type ElementType { get; }
    void RegisterOption(IDropdownOption option);
    void UnregisterOption(IDropdownOption option);
}

public interface IDropdownOption
{
    object? RawValue { get; }
    string DisplayText { get; }
    bool IsDisabled { get; }
    RenderFragment? Content { get; }
}