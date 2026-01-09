using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Forms.TextArea.JsInterop;

public interface ITextAreaJsInterop
{
    ValueTask InitializeAutoResizeAsync(ElementReference textarea, string textareaId);
    ValueTask DisposeAutoResizeAsync(string textareaId);
}

public sealed class TextAreaJsInterop : ModuleJsInteropBase, ITextAreaJsInterop
{
    public TextAreaJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.TextArea)
    {
    }

    public async ValueTask InitializeAutoResizeAsync(ElementReference textarea, string textareaId)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("initializeAutoResize", textarea, textareaId);
    }

    public async ValueTask DisposeAutoResizeAsync(string textareaId)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("disposeAutoResize", textareaId);
    }
}
