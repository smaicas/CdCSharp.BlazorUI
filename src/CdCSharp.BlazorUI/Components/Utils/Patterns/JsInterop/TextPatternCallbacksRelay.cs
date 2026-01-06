using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public sealed class PatternCallbacksRelay : IDisposable
{
    private readonly IPatternJsCallback _callbacks;

    [DynamicDependency(nameof(HandleSpanInput))]
    [DynamicDependency(nameof(HandleSpanFocus))]
    [DynamicDependency(nameof(HandleSpanBlur))]
    [DynamicDependency(nameof(HandlePaste))]
    public PatternCallbacksRelay(IPatternJsCallback callbacks)
    {
        _callbacks = callbacks;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<PatternCallbacksRelay> DotNetReference { get; }

    public void Dispose() => DotNetReference.Dispose();

    [JSInvokable]
    public Task HandleSpanInput(int index, string value)
        => _callbacks.HandleSpanInput(index, value);

    [JSInvokable]
    public Task HandleSpanFocus(int index)
        => _callbacks.HandleSpanFocus(index);

    [JSInvokable]
    public Task HandleSpanBlur(int index)
        => _callbacks.HandleSpanBlur(index);

    [JSInvokable]
    public Task HandlePaste(string text)
        => _callbacks.HandlePaste(text);
}
