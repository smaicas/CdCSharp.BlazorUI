namespace CdCSharp.BlazorUI.Components.Utils.Draggable.JsInterop;

public interface IDraggableJsCallback
{
    Task OnMouseMove(double clientX, double clientY);
    Task OnMouseUp(double clientX, double clientY);
}
