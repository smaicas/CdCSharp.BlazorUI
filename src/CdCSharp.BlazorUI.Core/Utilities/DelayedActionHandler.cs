// File: Core/Utilities/DelayedActionHandler.cs
namespace CdCSharp.BlazorUI.Core.Utilities;

public sealed class DelayedActionHandler : IDisposable
{
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();

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
}