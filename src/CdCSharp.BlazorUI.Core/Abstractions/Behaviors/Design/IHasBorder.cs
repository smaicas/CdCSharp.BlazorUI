namespace CdCSharp.BlazorUI.Components;

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

    // ---------- SIDES ----------
    public BorderStyle All(string width, BorderStyleType style, string color)
    {
        _all = new Border(width, style, color);
        _top = _right = _bottom = _left = null;
        return this;
    }

    public BorderStyle Top(string width, BorderStyleType style, string color)
    {
        _top = new Border(width, style, color);
        return this;
    }

    public BorderStyle Right(string width, BorderStyleType style, string color)
    {
        _right = new Border(width, style, color);
        return this;
    }

    public BorderStyle Bottom(string width, BorderStyleType style, string color)
    {
        _bottom = new Border(width, style, color);
        return this;
    }

    public BorderStyle Left(string width, BorderStyleType style, string color)
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

    // ---------- CSS VARIABLES (for component system via IHasBorder) ----------
    public IDictionary<string, string> ToCssVariables()
    {
        Dictionary<string, string> vars = [];

        if (_all != null && !_all.IsNone)
        {
            vars[FeatureDefinitions.InlineVariables.Border] = _all.ToCss();
        }

        if (_radius != null)
        {
            vars[FeatureDefinitions.InlineVariables.BorderRadius] = _radius.ToCss();
        }

        if (_top != null && !_top.IsNone)
            vars[FeatureDefinitions.InlineVariables.BorderTop] = _top.ToCss();
        if (_right != null && !_right.IsNone)
            vars[FeatureDefinitions.InlineVariables.BorderRight] = _right.ToCss();
        if (_bottom != null && !_bottom.IsNone)
            vars[FeatureDefinitions.InlineVariables.BorderBottom] = _bottom.ToCss();
        if (_left != null && !_left.IsNone)
            vars[FeatureDefinitions.InlineVariables.BorderLeft] = _left.ToCss();

        return vars;
    }

    // ---------- CSS VALUES (raw values without variable names) ----------
    public BorderCssValues GetCssValues()
    {
        return new BorderCssValues
        {
            All = _all?.ToCss(),
            Top = _top?.ToCss(),
            Right = _right?.ToCss(),
            Bottom = _bottom?.ToCss(),
            Left = _left?.ToCss(),
            Radius = _radius?.ToCss()
        };
    }
}

public sealed class BorderCssValues
{
    public string? All { get; init; }
    public string? Top { get; init; }
    public string? Right { get; init; }
    public string? Bottom { get; init; }
    public string? Left { get; init; }
    public string? Radius { get; init; }
}

#endregion

#region Internal Models

internal sealed class Border
{
    public Border(string width, BorderStyleType style, string color)
    {
        if (style == BorderStyleType.None || width == "0" || width == "0px")
        {
            Width = "0";
            Style = BorderStyleType.None;
            Color = "transparent";
            return;
        }

        Width = width;
        Style = style;
        Color = color;
    }

    public static Border None => new("0", BorderStyleType.None, "transparent");

    public string Color { get; }
    public bool IsNone => Style == BorderStyleType.None;
    public BorderStyleType Style { get; }
    public string StyleCss => Style.ToString().ToLowerInvariant();
    public string Width { get; }

    public string ToCss()
    {
        if (IsNone)
            return "0";

        return $"{Width} {StyleCss} {Color}";
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