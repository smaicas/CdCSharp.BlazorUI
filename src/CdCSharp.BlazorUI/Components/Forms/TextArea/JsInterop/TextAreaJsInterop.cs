using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Forms;

public interface ITextAreaJsInterop
{
    ValueTask DisposeAutoResizeAsync(string textareaId);

    ValueTask InitializeAutoResizeAsync(ElementReference textarea, string textareaId);
}

public sealed class TextAreaJsInterop : ModuleJsInteropBase, ITextAreaJsInterop
{
    public TextAreaJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.TextArea)
    {
    }

    public async ValueTask DisposeAutoResizeAsync(string textareaId)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("disposeAutoResize", textareaId);
    }

    public async ValueTask InitializeAutoResizeAsync(ElementReference textarea, string textareaId)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("initializeAutoResize", textarea, textareaId);
    }
}