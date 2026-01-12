using CdCSharp.BlazorUI.Core.Components.Selection;

namespace CdCSharp.BlazorUI.Components.Forms.Dropdown;

public interface IDropdownContainer
{
    bool IsMultiple { get; }
    Type ElementType { get; }
    void RegisterOption(ISelectionOption option);
    void UnregisterOption(ISelectionOption option);
}