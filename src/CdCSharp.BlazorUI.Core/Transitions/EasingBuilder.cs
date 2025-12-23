namespace CdCSharp.BlazorUI.Core.Transitions;

public class EasingBuilder
{
    private string _value = "ease";

    internal EasingBuilder() { }

    // Predefined easings
    public EasingBuilder Linear() { _value = "linear"; return this; }
    public EasingBuilder Ease() { _value = "ease"; return this; }
    public EasingBuilder EaseIn() { _value = "ease-in"; return this; }
    public EasingBuilder EaseOut() { _value = "ease-out"; return this; }
    public EasingBuilder EaseInOut() { _value = "ease-in-out"; return this; }

    // Cubic bezier
    public CubicBezierBuilder CubicBezier() => new(this);

    // Steps
    public StepsBuilder Steps(int count) => new(this, count);

    // Custom
    public EasingBuilder Custom(string value) { _value = value; return this; }

    internal void SetValue(string value) => _value = value;

    public string Build() => _value;

    // Implicit conversion
    public static implicit operator string(EasingBuilder builder) => builder.Build();
}

public class CubicBezierBuilder
{
    private readonly EasingBuilder _parent;
    private double _x1, _y1, _x2, _y2;

    internal CubicBezierBuilder(EasingBuilder parent) => _parent = parent;

    public CubicBezierBuilder WithControlPoints(double x1, double y1, double x2, double y2)
    {
        _x1 = x1; _y1 = y1; _x2 = x2; _y2 = y2;
        return this;
    }

    // Predefined curves
    public EasingBuilder MaterialStandard() => WithControlPoints(0.4, 0.0, 0.2, 1).Build();
    public EasingBuilder MaterialDecelerate() => WithControlPoints(0.0, 0.0, 0.2, 1).Build();
    public EasingBuilder MaterialAccelerate() => WithControlPoints(0.4, 0.0, 1, 1).Build();
    public EasingBuilder MaterialSharp() => WithControlPoints(0.4, 0.0, 0.6, 1).Build();

    public EasingBuilder Bounce() => WithControlPoints(0.68, -0.55, 0.265, 1.55).Build();
    public EasingBuilder Elastic() => WithControlPoints(0.68, -0.6, 0.32, 1.6).Build();
    public EasingBuilder BackOut() => WithControlPoints(0.175, 0.885, 0.32, 1.275).Build();

    public EasingBuilder Build()
    {
        _parent.SetValue(FormattableString.Invariant($"cubic-bezier({_x1:F3}, {_y1:F3}, {_x2:F3}, {_y2:F3})"));
        return _parent;
    }
}

public class StepsBuilder
{
    private readonly EasingBuilder _parent;
    private readonly int _count;
    private string _position = "end";

    internal StepsBuilder(EasingBuilder parent, int count)
    {
        _parent = parent;
        _count = count;
    }

    public StepsBuilder Start() { _position = "start"; return this; }
    public StepsBuilder End() { _position = "end"; return this; }
    public StepsBuilder JumpStart() { _position = "jump-start"; return this; }
    public StepsBuilder JumpEnd() { _position = "jump-end"; return this; }
    public StepsBuilder JumpNone() { _position = "jump-none"; return this; }
    public StepsBuilder JumpBoth() { _position = "jump-both"; return this; }

    public EasingBuilder Build()
    {
        _parent.SetValue($"steps({_count}, {_position})");
        return _parent;
    }
}

public static class Easing
{
    public static EasingBuilder Create() => new();

    // Quick access to common easings
    public static readonly string Linear = "linear";
    public static readonly string Ease = "ease";
    public static readonly string EaseIn = "ease-in";
    public static readonly string EaseOut = "ease-out";
    public static readonly string EaseInOut = "ease-in-out";
}