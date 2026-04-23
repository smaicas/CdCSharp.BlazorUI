namespace CdCSharp.BlazorUI.Components.Layout;

public sealed class ModalReference
{
    private readonly Action<ModalReference> _onClose;
    // RunContinuationsAsynchronously: prevent awaiter continuations from running inline on the
    // CloseAsync caller thread. Otherwise a costly awaiter of Result would block whoever closed
    // the modal.
    private readonly TaskCompletionSource<object?> _resultSource =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal ModalReference(string id, Action<ModalReference> onClose)
    {
        Id = id;
        _onClose = onClose;
    }

    public string Id { get; }

    internal Task<object?> Result => _resultSource.Task;

    public Task CloseAsync()
    {
        _resultSource.TrySetResult(null);
        _onClose(this);
        return Task.CompletedTask;
    }

    public Task CloseAsync<TResult>(TResult result)
    {
        _resultSource.TrySetResult(result);
        _onClose(this);
        return Task.CompletedTask;
    }

    internal void Cancel() => _resultSource.TrySetCanceled();
}