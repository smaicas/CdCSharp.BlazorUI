using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public sealed class PatternCallbacksRelay : IDisposable
{
    private readonly IPatternJsCallback _callback;

    [DynamicDependency(nameof(OnSpanInput))]
    [DynamicDependency(nameof(OnSpanComplete))]
    [DynamicDependency(nameof(OnSpanFocus))]
    [DynamicDependency(nameof(OnSpanBlur))]
    [DynamicDependency(nameof(OnPaste))]
    public PatternCallbacksRelay(IPatternJsCallback callback)
    {
        _callback = callback;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<PatternCallbacksRelay> DotNetReference { get; }

    [JSInvokable]
    public Task OnSpanInput(int index, string value)
        => _callback.OnSpanInput(index, value);

    [JSInvokable]
    public Task<bool> OnSpanComplete(int index, string value)
        => _callback.OnSpanComplete(index, value);

    [JSInvokable]
    public Task OnSpanFocus(int index)
        => _callback.OnSpanFocus(index);

    [JSInvokable]
    public Task OnSpanBlur(int index)
        => _callback.OnSpanBlur(index);

    [JSInvokable]
    public Task OnPaste(string text)
        => _callback.OnPaste(text);

    public void Dispose()
    {
        DotNetReference.Dispose();
    }
}