using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

public interface IHasBorder
{
    BorderStyle? Border { get; set; }
    BorderStyle? BorderTop { get; set; }
    BorderStyle? BorderRight { get; set; }
    BorderStyle? BorderBottom { get; set; }
    BorderStyle? BorderLeft { get; set; }
}

public class BorderStyle
{
    public string Width { get; }
    public BorderStyleType Style { get; }
    public CssColor Color { get; }
    public int? Radius { get; set; }

    public BorderStyle(string width, BorderStyleType style, CssColor color, int? radius = null)
    {
        Width = width;
        Style = style;
        Color = color;
        Radius = radius;
    }

    public string ToCssValue()
    {
        return $"{Width} {Style.ToString().ToLowerInvariant()} {Color.ToString(ColorOutputFormats.Rgba)}";
    }

    public string GetRadiusCssValue()
    {
        return Radius.HasValue ? $"{Radius}px" : string.Empty;
    }

    // Método factory para clonar con nuevo radius
    public BorderStyle WithRadius(int radius)
    {
        return new BorderStyle(Width, Style, Color, radius);
    }
}

public enum BorderStyleType
{
    None,
    Solid,
    Dashed,
    Dotted,
    Double,
    Groove,
    Ridge,
    Inset,
    Outset
}