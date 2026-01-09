using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Layout.Modal.JsInterop;

public interface IModalJsInterop
{
    ValueTask InitializeAsync(
        ElementReference overlayElement,
        DotNetObjectReference<ModalCallbacksRelay> dotnetReference,
        string hostId,
        bool closeOnEscape,
        bool closeOnOverlayClick);

    ValueTask UpdateOptionsAsync(
        string hostId,
        bool closeOnEscape,
        bool closeOnOverlayClick);

    ValueTask DisposeAsync(string hostId);

    ValueTask TrapFocusAsync(ElementReference element);
    ValueTask ReleaseFocusAsync();
}

public sealed class ModalJsInterop : ModuleJsInteropBase, IModalJsInterop
{
    public ModalJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Modal)
    {
    }

    public async ValueTask InitializeAsync(
        ElementReference overlayElement,
        DotNetObjectReference<ModalCallbacksRelay> dotnetReference,
        string hostId,
        bool closeOnEscape,
        bool closeOnOverlayClick)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "initialize",
            overlayElement,
            dotnetReference,
            hostId,
            closeOnEscape,
            closeOnOverlayClick);
    }

    public async ValueTask UpdateOptionsAsync(
        string hostId,
        bool closeOnEscape,
        bool closeOnOverlayClick)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "updateOptions",
            hostId,
            closeOnEscape,
            closeOnOverlayClick);
    }

    public async ValueTask DisposeAsync(string hostId)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("dispose", hostId);
    }

    public async ValueTask TrapFocusAsync(ElementReference element)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("trapFocus", element);
    }

    public async ValueTask ReleaseFocusAsync()
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("releaseFocus");
    }
}
