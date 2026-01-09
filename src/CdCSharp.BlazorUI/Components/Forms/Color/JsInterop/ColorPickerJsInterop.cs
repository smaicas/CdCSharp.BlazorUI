using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Forms.Color.JsInterop;


public interface IColorPickerJsInterop
{
    ValueTask<double[]> GetRelativePositionAsync(ElementReference element, double clientX, double clientY);
    ValueTask SetHandlerPositionAsync(ElementReference handler, double x, double y);
}

public sealed class ColorPickerJsInterop : ModuleJsInteropBase, IColorPickerJsInterop
{
    public ColorPickerJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.ColorPicker)
    {
    }

    public async ValueTask<double[]> GetRelativePositionAsync(ElementReference element, double clientX, double clientY)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;
        return await module.InvokeAsync<double[]>("getRelativePosition", element, clientX, clientY);
    }

    public async ValueTask SetHandlerPositionAsync(ElementReference handler, double x, double y)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("setHandlerPosition", handler, x, y);
    }
}