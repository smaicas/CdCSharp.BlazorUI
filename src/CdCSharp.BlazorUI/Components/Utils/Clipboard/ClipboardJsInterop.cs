using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Utils;

public interface IClipboardJsInterop
{
    ValueTask CopyTextAsync(string text);
}

public sealed class ClipboardJsInterop
    : ModuleJsInteropBase, IClipboardJsInterop
{
    public ClipboardJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Clipboard)
    {
    }

    public async ValueTask CopyTextAsync(string text)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("copyText", text);
    }
}