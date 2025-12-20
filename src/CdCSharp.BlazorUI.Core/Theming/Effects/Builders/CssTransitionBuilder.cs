namespace CdCSharp.BlazorUI.Core.Theming.Effects.Builders;

public class CssTransitionBuilder
{
    private readonly string _property;
    private TimeSpan _duration = TimeSpan.FromMilliseconds(300);
    private CssEasing _easing = CssEasing.Ease;
    private TimeSpan _delay = TimeSpan.Zero;

    public CssTransitionBuilder(string property) => _property = property;

    public CssTransitionBuilder Duration(TimeSpan duration)
    {
        _duration = duration;
        return this;
    }

    public CssTransitionBuilder DurationMs(int milliseconds)
    {
        _duration = TimeSpan.FromMilliseconds(milliseconds);
        return this;
    }

    public CssTransitionBuilder Easing(CssEasing easing)
    {
        _easing = easing;
        return this;
    }

    public CssTransitionBuilder Delay(TimeSpan delay)
    {
        _delay = delay;
        return this;
    }

    public CssTransition Build() => new(_property, _duration, _easing, _delay);

    // Implicit conversion
    public static implicit operator CssTransition(CssTransitionBuilder builder) => builder.Build();
}