namespace CdCSharp.BlazorUI.Core.Theming.Effects;

public class CssTransform : CssEffect
{
    private readonly List<string> _transforms = [];

    public CssTransform()
    {
        Name = "transform";
    }

    public CssTransform Scale(double value)
    {
        _transforms.Add($"scale({value})");
        return this;
    }

    public CssTransform ScaleX(double value)
    {
        _transforms.Add($"scaleX({value})");
        return this;
    }

    public CssTransform ScaleY(double value)
    {
        _transforms.Add($"scaleY({value})");
        return this;
    }

    public CssTransform Rotate(double degrees)
    {
        _transforms.Add($"rotate({degrees}deg)");
        return this;
    }

    public CssTransform TranslateX(double pixels)
    {
        _transforms.Add($"translateX({pixels}px)");
        return this;
    }

    public CssTransform TranslateY(double pixels)
    {
        _transforms.Add($"translateY({pixels}px)");
        return this;
    }

    public CssTransform Translate(double x, double y)
    {
        _transforms.Add($"translate({x}px, {y}px)");
        return this;
    }

    public override string ToCss() => $"transform: {string.Join(" ", _transforms)}";
    public override string ToInlineCss() => ToCss();
}