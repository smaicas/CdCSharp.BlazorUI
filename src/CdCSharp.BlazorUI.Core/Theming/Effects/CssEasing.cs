namespace CdCSharp.BlazorUI.Core.Theming.Effects;

public class CssEasing
{
    private readonly string _value;

    private CssEasing(string value) => _value = value;

    public static readonly CssEasing Linear = new("linear");
    public static readonly CssEasing Ease = new("ease");
    public static readonly CssEasing EaseIn = new("ease-in");
    public static readonly CssEasing EaseOut = new("ease-out");
    public static readonly CssEasing EaseInOut = new("ease-in-out");

    // Cubic bezier personalizado
    public static CssEasing CubicBezier(double x1, double y1, double x2, double y2) =>
        new($"cubic-bezier({x1}, {y1}, {x2}, {y2})");

    public override string ToString() => _value;

    // Implicit conversion
    public static implicit operator string(CssEasing easing) => easing._value;
}

public enum AnimationDirection
{
    Normal,
    Reverse,
    Alternate,
    AlternateReverse
}

public enum AnimationFillMode
{
    None,
    Forwards,
    Backwards,
    Both
}