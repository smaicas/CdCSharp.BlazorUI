using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Components.Utils;

public interface ITextPatternJsCallback
{
    Task NotifyTextChanged(string text);

    Task<bool> ValidatePartial(int index, string text);
}

public class TextPatternCallbacksRelay : IDisposable
{
    private readonly ITextPatternJsCallback _callbacks;
    [DynamicDependency("NotifyTextChanged")]
    [DynamicDependency("ValidatePartial")]
    public TextPatternCallbacksRelay(ITextPatternJsCallback callbacks)
    {
        _callbacks = callbacks;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<TextPatternCallbacksRelay> DotNetReference { get; }
    public void Dispose() => DotNetReference.Dispose();

    [JSInvokable]
    public Task NotifyTextChanged(string text) => _callbacks.NotifyTextChanged(text);

    [JSInvokable]
    public Task ValidatePartial(int index, string text) => _callbacks.ValidatePartial(index, text);
}
