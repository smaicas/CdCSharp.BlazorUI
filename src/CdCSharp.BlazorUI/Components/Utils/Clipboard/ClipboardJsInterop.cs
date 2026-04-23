using CdCSharp.BlazorUI.Abstractions;
using CdCSharp.BlazorUI.Types;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components;

public interface IClipboardJsInterop
{
    ValueTask CopyTextAsync(string text);
}

internal sealed class ClipboardJsInterop
    : ModuleJsInteropBase, IClipboardJsInterop
{
    public ClipboardJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.Clipboard)
    {
    }

    public async ValueTask CopyTextAsync(string text)
    {
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync("copyText", text);
    }
}