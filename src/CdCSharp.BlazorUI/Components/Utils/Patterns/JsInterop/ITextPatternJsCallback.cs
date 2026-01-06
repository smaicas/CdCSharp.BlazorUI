namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public interface IPatternJsCallback
{
    Task OnSpanInput(int index, string value);
    Task<bool> OnSpanComplete(int index, string value);
    Task OnSpanFocus(int index);
    Task OnSpanBlur(int index);
    Task OnPaste(string text);
}
