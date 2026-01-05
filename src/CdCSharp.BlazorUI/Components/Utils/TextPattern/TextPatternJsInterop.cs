using CdCSharp.BlazorUI.Core.Abstractions.JSInterop;
using CdCSharp.BlazorUI.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Components.Utils;

public interface ITextPatternJsInterop
{
    ValueTask TextPatternAddDynamicAsync(
        ElementReference containerBox,
        IEnumerable<ElementPattern> elements,
        DotNetObjectReference<TextPatternCallbacksRelay> dotnetReference,
        string notifyChangedTextCallback,
        string validatePartialCallback);
}

public class TextPatternJsInterop(IJSRuntime jsRuntime)
    : ModuleJsInteropBase(jsRuntime, JSModulesReference.TextPattern), ITextPatternJsInterop
{
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

public sealed class ElementPattern
{
    public ElementPattern(
        string pattern,
        string value,
        int length,
        string defaultValue,
        bool isSeparator,
        bool isEditable
    )
    {
        Pattern = pattern;
        Value = value;
        Length = length;
        DefaultValue = defaultValue;
        IsSeparator = isSeparator;
        IsEditable = isEditable;
    }

    public string DefaultValue { get; set; }
    public bool IsEditable { get; set; }
    public bool IsSeparator { get; set; }
    public int Length { get; set; }
    public string Pattern { get; set; }
    public string Value { get; set; }
}