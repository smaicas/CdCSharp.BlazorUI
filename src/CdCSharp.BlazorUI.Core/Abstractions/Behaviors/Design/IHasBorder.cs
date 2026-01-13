using CdCSharp.BlazorUI.Core.Css;
using static CdCSharp.BlazorUI.Core.Css.FeatureDefinitions;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

#region Public API

public interface IHasBorder
{
    BorderStyle? Border { get; set; }
}

public sealed class BorderStyle
{
    private Border? _all;
    private Border? _bottom;
    private Border? _left;
    private BorderRadius? _radius;
    private Border? _right;
    private Border? _top;
    private BorderStyle() { }

    public static BorderStyle Create() => new();

    // ---------- ALL ----------
    public BorderStyle All(string width, BorderStyleType style, CssColor color)
    {
        _all = new Border(width, style, color);

        // Definir ALL invalida overrides anteriores
        _top = _right = _bottom = _left = null;

        return this;
    }

    public BorderStyle Bottom(string width, BorderStyleType style, CssColor color)
    {
        _bottom = new Border(width, style, color);
        return this;
    }

    public BorderStyle Left(string width, BorderStyleType style, CssColor color)
    {
        _left = new Border(width, style, color);
        return this;
    }

    // ---------- PRESETS ----------
    public BorderStyle None()
    {
        _all = Border.None;
        _top = _right = _bottom = _left = null;
        _radius = null;
        return this;
    }
    // ---------- RADIUS ----------
    public BorderStyle Radius(int all)
    {
        _radius = BorderRadius.All(all);
        return this;
    }

    public BorderStyle Radius(
            int? topLeft = null,
            int? topRight = null,
            int? bottomRight = null,
            int? bottomLeft = null)
    {
        _radius = new BorderRadius
        {
            TopLeft = topLeft,
            TopRight = topRight,
            BottomRight = bottomRight,
            BottomLeft = bottomLeft
        };
        return this;
    }

    public BorderStyle Right(string width, BorderStyleType style, CssColor color)
    {
        _right = new Border(width, style, color);
        return this;
    }

    // ---------- CSS VARIABLES ----------
    public IDictionary<string, string> ToCssVariables()
    {
        Dictionary<string, string> vars = [];

        if (_all != null && !_all.IsNone)
        {
            vars[InlineVariables.Border] = _all.ToCss();

            if (_radius != null)
                vars[InlineVariables.BorderRadius] = _radius.ToCss();
        }

        AddSide(vars, _top, InlineVariables.BorderTop);
        AddSide(vars, _right, InlineVariables.BorderRight);
        AddSide(vars, _bottom, InlineVariables.BorderBottom);
        AddSide(vars, _left, InlineVariables.BorderLeft);

        return vars;
    }

    // ---------- SIDES ----------
    public BorderStyle Top(string width, BorderStyleType style, CssColor color)
    {
        _top = new Border(width, style, color);
        return this;
    }
    private static void AddSide(
        IDictionary<string, string> vars,
        Border? side,
        string cssVar)
    {
        if (side == null || side.IsNone)
            return;

        vars[cssVar] = side.ToCss();
    }
}

#endregion

#region Internal Models

internal sealed class Border
{
    public Border(string width, BorderStyleType style, CssColor color)
    {
        if (style == BorderStyleType.None || width == "0" || width == "0px")
        {
            Width = "0";
            Style = BorderStyleType.None;
            Color = new(0, 0, 0, 0);
            return;
        }

        Width = width;
        Style = style;
        Color = color;
    }

    public static Border None => new("0", BorderStyleType.None, new(0, 0, 0, 0));

    public CssColor Color { get; }
    public string ColorCss => Color.ToString(ColorOutputFormats.Rgba);
    public bool IsNone => Style == BorderStyleType.None;
    public BorderStyleType Style { get; }
    public string StyleCss => Style.ToString().ToLowerInvariant();
    public string Width { get; }
    public string ToCss()
    {
        if (IsNone)
            return "0";

        return $"{Width} {StyleCss} {ColorCss}";
    }
}

internal sealed class BorderRadius
{
    public int? BottomLeft { get; set; }
    public int? BottomRight { get; set; }
    public int? TopLeft { get; set; }
    public int? TopRight { get; set; }
    public static BorderRadius All(int value)
    {
        value = Math.Max(0, value);

        return new()
        {
            TopLeft = value,
            TopRight = value,
            BottomRight = value,
            BottomLeft = value
        };
    }

    public string ToCss()
    {
        int tl = Math.Max(0, TopLeft ?? 0);
        int tr = Math.Max(0, TopRight ?? tl);
        int br = Math.Max(0, BottomRight ?? tl);
        int bl = Math.Max(0, BottomLeft ?? tl);

        if (tl == tr && tr == br && br == bl)
            return $"{tl}px";

        return $"{tl}px {tr}px {br}px {bl}px";
    }
}

#endregion

#region Enums

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

#endregion
