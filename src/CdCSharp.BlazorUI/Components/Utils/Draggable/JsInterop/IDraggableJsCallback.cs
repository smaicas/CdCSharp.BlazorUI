namespace CdCSharp.BlazorUI.Components;

public interface IDraggableJsCallback
{
    Task OnMouseMove(double clientX, double clientY);

    Task OnMouseUp(double clientX, double clientY);
}