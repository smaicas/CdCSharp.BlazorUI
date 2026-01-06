namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public interface IPatternJsCallback
{
    Task HandleSpanInput(int index, string value);
    Task HandleSpanFocus(int index);
    Task HandleSpanBlur(int index);
    Task HandlePaste(string text);
}
