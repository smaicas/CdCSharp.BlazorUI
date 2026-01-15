namespace CdCSharp.BlazorUI.Components.Layout;

public interface IModalJsCallback
{
    Task OnEscapePressed();

    Task OnOverlayClick();
}