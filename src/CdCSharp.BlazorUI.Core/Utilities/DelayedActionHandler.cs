// File: Core/Utilities/DelayedActionHandler.cs
namespace CdCSharp.BlazorUI.Core.Utilities;

public sealed class DelayedActionHandler : IDisposable
{
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;

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
        Cancel();
    }

    public async Task ExecuteWithDelayAsync(Func<Task> action, TimeSpan delay)
    {
        Cancel();

        CancellationTokenSource cts;
        lock (_lock)
        {
            _cts = new CancellationTokenSource();
            cts = _cts;
        }

        try
        {
            await Task.Delay(delay, cts.Token);
            if (!cts.Token.IsCancellationRequested)
            {
                await action();
            }
        }
        catch (TaskCanceledException)
        {
        }
    }
}