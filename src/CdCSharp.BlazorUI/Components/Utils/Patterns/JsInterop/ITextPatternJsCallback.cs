namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public interface ITextPatternJsCallback
{
    Task NotifyTextChanged(string text);
    Task<bool> ValidatePartial(int index, string text);
}
