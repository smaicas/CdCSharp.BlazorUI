namespace CdCSharp.BlazorUI.Components.Layout;

public class ModalReference
{
    private readonly Action<ModalReference> _onClose;
    private readonly TaskCompletionSource<object?> _resultSource = new();

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