using CdCSharp.BlazorUI.Core.Theming.Effects.Builders;

namespace CdCSharp.BlazorUI.Core.Theming.Effects;

public class CssTransition : CssEffect
{
    public string Property { get; set; }
    public TimeSpan Duration { get; set; }
    public CssEasing Easing { get; set; }
    public TimeSpan Delay { get; set; }

    public CssTransition(string property, TimeSpan duration, CssEasing? easing = null, TimeSpan? delay = null)
    {
        Property = property;
        Duration = duration;
        Easing = easing ?? CssEasing.Ease;
        Delay = delay ?? TimeSpan.Zero;
    }

    public override string ToCss() =>
        $"transition: {Property} {Duration.TotalMilliseconds}ms {Easing} {Delay.TotalMilliseconds}ms";

    public override string ToInlineCss() => ToCss();

    // Builder pattern
    public static CssTransitionBuilder For(string property) => new(property);
}
