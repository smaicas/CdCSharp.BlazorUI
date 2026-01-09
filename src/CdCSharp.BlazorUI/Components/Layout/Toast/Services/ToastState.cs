using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components.Layout.Toast.Services;

public sealed class ToastState
{
    public Guid Id { get; } = Guid.NewGuid();
    public required RenderFragment Content { get; init; }
    public required ToastOptions Options { get; init; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime StartedAt { get; set; }
    public bool IsClosing { get; set; }
    public bool IsPaused { get; set; }
    public TimeSpan ElapsedBeforePause { get; set; } = TimeSpan.Zero;
    public CancellationTokenSource? DismissTokenSource { get; set; }

    public ToastState()
    {
        StartedAt = DateTime.UtcNow;
    }

    public TimeSpan RemainingTime
    {
        get
        {
            if (!Options.AutoDismiss) return TimeSpan.Zero;

            TimeSpan elapsed = IsPaused
                ? ElapsedBeforePause
                : ElapsedBeforePause + (DateTime.UtcNow - StartedAt);

            TimeSpan remaining = Options.Duration - elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    public double ProgressPercentage
    {
        get
        {
            if (!Options.AutoDismiss || Options.Duration == TimeSpan.Zero) return 0;

            TimeSpan elapsed = IsPaused
                ? ElapsedBeforePause
                : ElapsedBeforePause + (DateTime.UtcNow - StartedAt);

            double percentage = (elapsed.TotalMilliseconds / Options.Duration.TotalMilliseconds) * 100;
            return Math.Clamp(percentage, 0, 100);
        }
    }
}
