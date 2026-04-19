namespace CdCSharp.BlazorUI.Core.Utilities;

public sealed class DelayedActionHandler : IDisposable
{
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private bool _disposed;

    public void Cancel()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _disposed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }

    public async Task ExecuteWithDelayAsync(Func<Task> action, TimeSpan delay)
    {
        CancellationTokenSource cts;
        lock (_lock)
        {
            if (_disposed) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            cts = _cts;
        }

        try
        {
            await Task.Delay(delay, cts.Token);
            if (cts.Token.IsCancellationRequested) return;

            lock (_lock)
            {
                if (_disposed) return;
            }

            await action();
        }
        catch (TaskCanceledException)
        {
        }
    }
}