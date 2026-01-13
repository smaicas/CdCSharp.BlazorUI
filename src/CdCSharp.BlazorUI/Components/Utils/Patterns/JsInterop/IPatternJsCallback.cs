namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public interface IPatternJsCallback
{
    Task OnPaste(string text);

    Task OnSpanBlur(int index);

    Task<bool> OnSpanComplete(int index, string value);

    Task OnSpanFocus(int index);

    Task OnSpanInput(int index, string value);

    Task OnToggleClick(int index);
}