using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public interface IPatternJsInterop
{
    ValueTask DisposePatternAsync(string componentId);

    ValueTask FocusFirstEditableAsync(string componentId);

    ValueTask FocusSpanAsync(string componentId, int index);

    ValueTask InitializePatternAsync(
                    ElementReference containerBox,
        DotNetObjectReference<PatternCallbacksRelay> dotnetReference,
        string componentId);

    ValueTask SelectSpanContentAsync(string componentId, int index);

    ValueTask SetCaretToEndAsync(string componentId, int index);

    ValueTask UpdateSpanValueAsync(string componentId, int index, string value);
}

public sealed class PatternJsInterop
    : ModuleJsInteropBase, IPatternJsInterop
{
    public PatternJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.TextPattern)
    {
    }

    public async ValueTask DisposePatternAsync(string componentId)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "disposePattern",
            componentId);
    }

    public async ValueTask FocusFirstEditableAsync(string componentId)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;
        await module.InvokeVoidAsync("focusFirstEditable", componentId);
    }

    public async ValueTask FocusSpanAsync(string componentId, int index)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "focusSpan",
            componentId,
            index);
    }

    public async ValueTask InitializePatternAsync(
                    ElementReference containerBox,
        DotNetObjectReference<PatternCallbacksRelay> dotnetReference,
        string componentId)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "initializePattern",
            containerBox,
            dotnetReference,
            componentId);
    }

    public async ValueTask SelectSpanContentAsync(string componentId, int index)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "selectSpanContent",
            componentId,
            index);
    }

    public async ValueTask SetCaretToEndAsync(string componentId, int index)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "setCaretToEnd",
            componentId,
            index);
    }

    public async ValueTask UpdateSpanValueAsync(string componentId, int index, string value)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "updateSpanValue",
            componentId,
            index,
            value);
    }
}