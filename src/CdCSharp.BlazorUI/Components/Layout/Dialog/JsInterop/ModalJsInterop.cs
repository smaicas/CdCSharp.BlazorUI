using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Layout;

public interface IModalJsInterop
{
    ValueTask LockScrollAsync();

    ValueTask ReleaseFocusAsync();

    ValueTask TrapFocusAsync(ElementReference element);

    ValueTask UnlockScrollAsync();

    ValueTask WaitForAnimationEndAsync(ElementReference element, int fallbackMs);
}

public sealed class ModalJsInterop : ModuleJsInteropBase, IModalJsInterop
{
    public ModalJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Modal)
    {
    }

    public async ValueTask LockScrollAsync()
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("lockScroll");
    }

    public async ValueTask ReleaseFocusAsync()
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("releaseFocus");
    }

    public async ValueTask TrapFocusAsync(ElementReference element)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("trapFocus", element);
    }

    public async ValueTask UnlockScrollAsync()
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("unlockScroll");
    }

    public async ValueTask WaitForAnimationEndAsync(ElementReference element, int fallbackMs)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("waitForAnimationEnd", element, fallbackMs);
    }
}