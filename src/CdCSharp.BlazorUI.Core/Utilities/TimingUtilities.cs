// File: Core/Utilities/TimingUtilities.cs
namespace CdCSharp.BlazorUI.Core.Utilities;

public static class TimingUtilities
{
    public static Action<T> Debounce<T>(Action<T> action, TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(action);

        int last = 0;
        return arg =>
        {
            int current = Interlocked.Increment(ref last);
            Task.Delay(interval).ContinueWith(_ =>
            {
                if (current == last)
                    action(arg);
            });
        };
    }

    public static Action Debounce(Action action, TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(action);

        int last = 0;
        return () =>
        {
            int current = Interlocked.Increment(ref last);
            Task.Delay(interval).ContinueWith(_ =>
            {
                if (current == last)
                    action();
            });
        };
    }
}