namespace CdCSharp.BlazorUI.Components.Layout.Modal.JsInterop;

public interface IModalJsCallback
{
    Task OnEscapePressed();

    Task OnOverlayClick();
}