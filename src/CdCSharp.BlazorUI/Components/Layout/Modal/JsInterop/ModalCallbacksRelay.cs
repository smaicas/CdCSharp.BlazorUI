using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.Components.Layout.Modal.JsInterop;

public sealed class ModalCallbacksRelay : IDisposable
{
    private readonly IModalJsCallback _callback;

    [DynamicDependency(nameof(OnEscapePressed))]
    [DynamicDependency(nameof(OnOverlayClick))]
    public ModalCallbacksRelay(IModalJsCallback callback)
    {
        _callback = callback;
        DotNetReference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<ModalCallbacksRelay> DotNetReference { get; }

    [JSInvokable]
    public Task OnEscapePressed() => _callback.OnEscapePressed();

    [JSInvokable]
    public Task OnOverlayClick() => _callback.OnOverlayClick();

    public void Dispose() => DotNetReference.Dispose();
}
