using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Components.Utils.Patterns.JsInterop;

public sealed class TextPatternCallbacksRelay : IDisposable
{
    private readonly ITextPatternJsCallback _callbacks;

    [DynamicDependency(nameof(NotifyTextChanged))]
    [DynamicDependency(nameof(ValidatePartial))]
    public TextPatternCallbacksRelay(ITextPatternJsCallback callbacks)
    {
        _callbacks = callbacks;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<TextPatternCallbacksRelay> DotNetReference { get; }

    public void Dispose() => DotNetReference.Dispose();

    [JSInvokable]
    public Task NotifyTextChanged(string text)
        => _callbacks.NotifyTextChanged(text);

    [JSInvokable]
    public Task<bool> ValidatePartial(int index, string text)
        => _callbacks.ValidatePartial(index, text);
}
