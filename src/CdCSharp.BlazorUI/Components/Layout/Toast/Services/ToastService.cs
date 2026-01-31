using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CdCSharp.BlazorUI.Components.Layout.Services;

public interface IToastService
{
    event Action? OnChange;

    IReadOnlyList<ToastState> ActiveToasts { get; }

    void Close(Guid toastId);

    void CloseAll();

    void Pause(Guid toastId);

    void Resume(Guid toastId);

    //void Show(RenderFragment content, ToastOptions? options = null);

    void Show(Action<RenderTreeBuilder> builder, ToastOptions? options = null);

    void Show<TComponent>(ToastOptions? options = null) where TComponent : IComponent;

    void Show<TComponent>(IDictionary<string, object?>? parameters, ToastOptions? options = null) where TComponent : IComponent;
}

public sealed class ToastService : IToastService
{
    private readonly object _lock = new();
    private readonly List<ToastState> _toasts = [];

    public event Action? OnChange;

    public IReadOnlyList<ToastState> ActiveToasts
    {
        get
        {
            lock (_lock)
            {
                return _toasts.ToList().AsReadOnly();
            }
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _toasts.Count;
            }
        }
    }

    public void Close(Guid toastId)
    {
        ToastState? toast;
        lock (_lock)
        {
            toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast == null) return;

            toast.IsClosing = true;
            toast.DismissTokenSource?.Cancel();
        }

        NotifyChange();
    }

    public void CloseAll()
    {
        lock (_lock)
        {
            foreach (ToastState toast in _toasts)
            {
                toast.IsClosing = true;
                toast.DismissTokenSource?.Cancel();
            }
        }

        NotifyChange();
    }

    public void Pause(Guid toastId)
    {
        lock (_lock)
        {
            ToastState? toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast == null || toast.IsPaused || !toast.Options.AutoDismiss) return;

            toast.DismissTokenSource?.Cancel();
            toast.ElapsedBeforePause += DateTime.UtcNow - toast.StartedAt;
            toast.IsPaused = true;
        }

        NotifyChange();
    }

    public void Resume(Guid toastId)
    {
        lock (_lock)
        {
            ToastState? toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast == null || !toast.IsPaused || !toast.Options.AutoDismiss) return;

            toast.IsPaused = false;
            toast.StartedAt = DateTime.UtcNow;

            ScheduleDismiss(toast);
        }

        NotifyChange();
    }

    public void ShowFragment(RenderFragment content, ToastOptions? options = null)
    {
        ToastState state = new()
        {
            Content = content,
            Options = options ?? ToastOptions.Default
        };

        AddToast(state);
    }

    public void Show(Action<RenderTreeBuilder> builder, ToastOptions? options = null)
    {
        RenderFragment fragment = new(builder);
        ShowFragment(fragment, options);
    }

    public void Show<TComponent>(ToastOptions? options = null) where TComponent : IComponent
    {
        Show<TComponent>(null, options);
    }

    public void Show<TComponent>(IDictionary<string, object?>? parameters, ToastOptions? options = null) where TComponent : IComponent
    {
        RenderFragment fragment = builder =>
        {
            int seq = 0;
            builder.OpenComponent<TComponent>(seq++);

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object?> param in parameters)
                {
                    builder.AddAttribute(seq++, param.Key, param.Value);
                }
            }

            builder.CloseComponent();
        };

        ShowFragment(fragment, options);
    }

    internal void Remove(Guid toastId)
    {
        ToastState? toast;
        lock (_lock)
        {
            toast = _toasts.FirstOrDefault(t => t.Id == toastId);
            if (toast == null) return;

            toast.DismissTokenSource?.Dispose();
            _toasts.Remove(toast);
        }

        toast.Options.OnClose?.Invoke();
        NotifyChange();
    }

    private void AddToast(ToastState toast)
    {
        lock (_lock)
        {
            _toasts.Add(toast);

            if (toast.Options.AutoDismiss)
            {
                ScheduleDismiss(toast);
            }
        }

        NotifyChange();
    }

    private async Task DismissAfterDelayAsync(Guid toastId, TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            Close(toastId);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void NotifyChange() => OnChange?.Invoke();

    private void ScheduleDismiss(ToastState toast)
    {
        toast.DismissTokenSource?.Cancel();
        toast.DismissTokenSource = new CancellationTokenSource();

        TimeSpan delay = toast.RemainingTime > TimeSpan.Zero
            ? toast.RemainingTime
            : toast.Options.Duration;

        _ = DismissAfterDelayAsync(toast.Id, delay, toast.DismissTokenSource.Token);
    }
}