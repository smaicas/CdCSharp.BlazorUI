using CdCSharp.BlazorUI.Abstractions;

namespace CdCSharp.BlazorUI.Components.Forms.Dropdown;

public interface IDropdownContainer
{
    Type ElementType { get; }
    bool IsMultiple { get; }

    void RegisterOption(ISelectionOption option);

    void UnregisterOption(ISelectionOption option);
}