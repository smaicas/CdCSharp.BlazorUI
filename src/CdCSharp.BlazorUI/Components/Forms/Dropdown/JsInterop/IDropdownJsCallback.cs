namespace CdCSharp.BlazorUI.Components.Forms.Dropdown.JsInterop;

public interface IDropdownJsCallback
{
    Task OnClickOutside();
    Task OnKeyDown(string key, bool shiftKey, bool ctrlKey);
    Task<DropdownPosition> OnRequestPosition();
}

public record struct DropdownPosition(
    double TriggerTop,
    double TriggerLeft,
    double TriggerWidth,
    double TriggerHeight,
    double ViewportHeight,
    double ViewportWidth,
    double ScrollY);
