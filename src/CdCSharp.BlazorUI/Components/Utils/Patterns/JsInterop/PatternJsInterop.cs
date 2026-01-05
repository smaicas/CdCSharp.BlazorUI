using CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;
using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public interface IPatternJsInterop
{
    ValueTask TextPatternAddDynamicAsync(
        ElementReference containerBox,
        IEnumerable<ElementPattern> elements,
        DotNetObjectReference<TextPatternCallbacksRelay> dotnetReference,
        string notifyChangedTextCallback,
        string validatePartialCallback);
}

public sealed class PatternJsInterop
    : ModuleJsInteropBase, IPatternJsInterop
{
    public PatternJsInterop(IJSRuntime jsRuntime)
        : base(jsRuntime, JSModulesReference.TextPattern)
    {
    }

    public async ValueTask TextPatternAddDynamicAsync(
        ElementReference containerBox,
        IEnumerable<ElementPattern> elements,
        DotNetObjectReference<TextPatternCallbacksRelay> dotnetReference,
        string notifyChangedTextCallback,
        string validatePartialCallback)
    {
        await IsModuleTaskLoaded.Task;
        IJSObjectReference module = await ModuleTask.Value;

        await module.InvokeVoidAsync(
            "TextPatternAddDynamic",
            containerBox,
            elements,
            dotnetReference,
            notifyChangedTextCallback,
            validatePartialCallback);
    }
}