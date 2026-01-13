using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

public interface IHasShadow
{
    ShadowStyle? Shadow { get; set; }
}

public readonly struct ShadowLine
{
    public readonly CssColor Color;
    public readonly bool Inset;
    public readonly float Opacity;
    public readonly int X, Y, Blur, Spread;

    internal ShadowLine(
        int x, int y, int blur, int spread,
        float opacity, CssColor color, bool inset)
    {
        X = x;
        Y = y;
        Blur = blur;
        Spread = spread;
        Opacity = opacity;
        Color = color;
        Inset = inset;
    }
}

public sealed class ShadowStyle
{
    private readonly List<ShadowLine> _lines = [];

    private ShadowStyle()
    { }

    internal IReadOnlyList<ShadowLine> Lines => _lines;

    public static ShadowStyle Create(
            int y,
        int blur,
        float opacity = 0.2f,
        int x = 0,
        int spread = 0,
        CssColor? color = null,
        bool inset = false)
    {
        ShadowStyle style = new();
        style._lines.Add(new ShadowLine(
            x, y, blur, spread,
            opacity,
            color ?? new CssColor(0, 0, 0, 255),
            inset));

        return style;
    }

    public ShadowStyle Add(
        int y,
        int blur,
        float opacity = 0.2f,
        int x = 0,
        int spread = 0,
        CssColor? color = null,
        bool inset = false)
    {
        _lines.Add(new ShadowLine(
            x, y, blur, spread,
            opacity,
            color ?? new CssColor(0, 0, 0, 255),
            inset));

        return this;
    }
}

public static class BUIShadowPresets
{
    /// <summary>
    /// Generates realistic Material Design-inspired elevation shadows. Each elevation level (0-24)
    /// has been calibrated for visual consistency.
    /// </summary>
    public static ShadowStyle Elevation(int level, CssColor? color = null)
    {
        color ??= BUIColor.Palette.Shadow;
        level = Math.Clamp(level, 0, 24);

        if (level == 0)
            return ShadowStyle.Create(0, 0, 0f, color: color);

        (int keyY, int keyBlur, float keyOpacity) = GetKeyShadow(level);
        (int ambientY, int ambientBlur, float ambientOpacity) = GetAmbientShadow(level);

        return ShadowStyle
            .Create(keyY, keyBlur, keyOpacity, color: color)
            .Add(ambientY, ambientBlur, ambientOpacity, color: color);
    }

    private static (int y, int blur, float opacity) GetAmbientShadow(int level)
    {
        // Ambient shadow: soft, diffuse, represents scattered ambient light
        return level switch
        {
            0 => (0, 0, 0.00f),
            1 => (1, 2, 0.12f),
            2 => (2, 3, 0.12f),
            3 => (3, 4, 0.12f),
            4 => (4, 5, 0.12f),
            5 => (4, 8, 0.13f),
            6 => (5, 10, 0.14f),
            7 => (5, 11, 0.14f),
            8 => (6, 12, 0.14f),
            9 => (7, 14, 0.14f),
            10 => (8, 16, 0.15f),
            11 => (9, 16, 0.15f),
            12 => (10, 18, 0.16f),
            13 => (11, 20, 0.16f),
            14 => (12, 22, 0.16f),
            15 => (13, 23, 0.16f),
            16 => (14, 24, 0.16f),
            17 => (15, 26, 0.17f),
            18 => (16, 28, 0.17f),
            19 => (17, 30, 0.17f),
            20 => (18, 32, 0.17f),
            21 => (19, 34, 0.18f),
            22 => (19, 35, 0.18f),
            23 => (20, 36, 0.18f),
            24 => (20, 38, 0.18f),

            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }

    private static (int y, int blur, float opacity) GetKeyShadow(int level)
    {
        // Key shadow: directional, represents direct light occlusion Based on Material Design 3
        // elevation system
        return level switch
        {
            0 => (0, 0, 0.00f),
            1 => (1, 3, 0.20f),
            2 => (2, 4, 0.20f),
            3 => (3, 5, 0.20f),
            4 => (4, 6, 0.20f),
            5 => (5, 7, 0.20f),
            6 => (6, 8, 0.20f),
            7 => (7, 9, 0.21f),
            8 => (8, 10, 0.22f),
            9 => (9, 12, 0.22f),
            10 => (10, 13, 0.23f),
            11 => (11, 13, 0.23f),
            12 => (12, 14, 0.24f),
            13 => (13, 15, 0.24f),
            14 => (14, 16, 0.24f),
            15 => (15, 17, 0.24f),
            16 => (16, 18, 0.24f),
            17 => (17, 19, 0.25f),
            18 => (18, 20, 0.25f),
            19 => (19, 21, 0.25f),
            20 => (20, 22, 0.25f),
            21 => (21, 22, 0.26f),
            22 => (22, 23, 0.26f),
            23 => (23, 23, 0.26f),
            24 => (24, 24, 0.26f),

            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
    }
}