namespace CdCSharp.BlazorUI.Components.Layout;

public sealed class ToastOptions
{
    public ToastPosition Position { get; init; } = ToastPosition.TopRight;
    public ToastAnimation Animation { get; init; } = ToastAnimation.Default;
    public bool AutoDismiss { get; init; } = true;
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(5);
    public bool Closable { get; init; } = true;
    public Action? OnClick { get; init; }
    public Action? OnClose { get; init; }
    public string? CssClass { get; init; }

    public static ToastOptions Default => new();

    public static ToastOptions Persistent => new()
    {
        AutoDismiss = false,
        Closable = true
    };

    public static ToastOptions Quick => new()
    {
        AutoDismiss = true,
        Duration = TimeSpan.FromSeconds(2)
    };

    public static ToastOptions Long => new()
    {
        AutoDismiss = true,
        Duration = TimeSpan.FromSeconds(10)
    };
}

public enum ToastPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}