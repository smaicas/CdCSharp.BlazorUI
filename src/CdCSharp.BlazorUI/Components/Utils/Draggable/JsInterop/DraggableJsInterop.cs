using CdCSharp.BlazorUI.Abstractions;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components;

internal interface IDraggableJsInterop
{
    ValueTask StartDragAsync(
        ElementReference element,
        DotNetObjectReference<DraggableCallbacksRelay> dotnetReference,
        string componentId);

    ValueTask StopDragAsync(string componentId);
}

internal sealed class DraggableJsInterop : ModuleJsInteropBase, IDraggableJsInterop
{
    public DraggableJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Draggable)
    {
    }

    public async ValueTask StartDragAsync(
        ElementReference element,
        DotNetObjectReference<DraggableCallbacksRelay> dotnetReference,
        string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("startDrag", element, dotnetReference, componentId);
    }

    public async ValueTask StopDragAsync(string componentId)
    {
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("stopDrag", componentId);
    }
}