namespace CdCSharp.BlazorUI.Core.Theming.Effects;

public class CssAnimation : CssEffect
{
    public string KeyframeName { get; set; }
    public TimeSpan Duration { get; set; }
    public CssEasing Easing { get; set; }
    public TimeSpan Delay { get; set; }
    public int IterationCount { get; set; } = 1;
    public AnimationDirection Direction { get; set; } = AnimationDirection.Normal;
    public AnimationFillMode FillMode { get; set; } = AnimationFillMode.None;

    public CssAnimation()
    {
        Name = "animation";
        Easing = CssEasing.Ease;
        Delay = TimeSpan.Zero;
    }

    public override string ToCss()
    {
        string direction = Direction.ToString().ToLowerInvariant().Replace("_", "-");
        string fillMode = FillMode.ToString().ToLowerInvariant();
        string iterations = IterationCount == -1 ? "infinite" : IterationCount.ToString();

        return $"animation: {KeyframeName} {Duration.TotalMilliseconds}ms {Easing} {Delay.TotalMilliseconds}ms {iterations} {direction} {fillMode}";
    }

    public override string ToInlineCss() => ToCss();
}
