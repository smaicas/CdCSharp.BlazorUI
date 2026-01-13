using CdCSharp.BlazorUI.Core.Css;
using System.Globalization;
using static CdCSharp.BlazorUI.Core.Css.FeatureDefinitions;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

public interface IHasShadow
{
    ShadowStyle? Shadow { get; set; }
}

public sealed class ShadowStyle
{
    private CssColor _color = new(0, 0, 0, 255);
    private float _opacity = 0.2f;
    private int _offsetX = 0;
    private int _offsetY = 4;
    private int _blur = 8;
    private int _spread = 0;
    private bool _inset = false;

    private ShadowStyle() { }

    public static ShadowStyle Create() => new();

    public ShadowStyle Color(CssColor color)
    {
        _color = color;
        return this;
    }

    public ShadowStyle Opacity(float opacity)
    {
        _opacity = Math.Clamp(opacity, 0f, 1f);
        return this;
    }

    public ShadowStyle OffsetX(int px)
    {
        _offsetX = px;
        return this;
    }

    public ShadowStyle OffsetY(int px)
    {
        _offsetY = px;
        return this;
    }

    public ShadowStyle Blur(int px)
    {
        _blur = Math.Max(0, px);
        return this;
    }

    public ShadowStyle Spread(int px)
    {
        _spread = px;
        return this;
    }

    public ShadowStyle Inset(bool inset = true)
    {
        _inset = inset;
        return this;
    }

    internal IDictionary<string, string> ToCssVariables()
    {
        Dictionary<string, string> vars = new()
        {
            [InlineVariables.ShadowOffsetX] = $"{_offsetX}px",
            [InlineVariables.ShadowOffsetY] = $"{_offsetY}px",
            [InlineVariables.ShadowBlur] = $"{_blur}px",
            [InlineVariables.ShadowSpread] = $"{_spread}px",
            [InlineVariables.ShadowColor] = _color.ToString(ColorOutputFormats.Rgba),
            [InlineVariables.ShadowOpacity] = _opacity.ToString("F2", CultureInfo.InvariantCulture),
            [InlineVariables.ShadowInset] = _inset ? "inset" : ""
        };

        return vars;
    }
}

public static class BUIShadowPresets
{
    public static ShadowStyle Elevation(int level)
    {
        level = Math.Clamp(level, 0, 24);

        if (level == 0)
        {
            return ShadowStyle.Create()
                .OffsetX(0)
                .OffsetY(0)
                .Blur(0)
                .Spread(0)
                .Opacity(0);
        }

        float elevation = level;

        int offsetY = (int)Math.Round(Math.Sqrt(elevation) * 2);
        int blur = (int)Math.Round(Math.Pow(elevation, 1.3));
        float opacity = Math.Clamp(0.24f - elevation * 0.004f, 0.08f, 0.24f);

        return ShadowStyle.Create()
            .OffsetX(0)
            .OffsetY(offsetY)
            .Blur(blur)
            .Spread(0)
            .Opacity(opacity);
    }
}