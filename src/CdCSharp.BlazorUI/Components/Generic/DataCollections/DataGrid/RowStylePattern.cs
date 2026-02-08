namespace CdCSharp.BlazorUI.Components;

public sealed class RowStylePattern
{
    private readonly Func<int, RowStyle?> _selector;

    private RowStylePattern(Func<int, RowStyle?> selector)
    {
        _selector = selector;
    }

    internal RowStyle? GetStyleForIndex(int index) => _selector(index);

    public static RowStylePattern Alternating(
        string? evenBackground = null,
        string? oddBackground = null)
    {
        return new RowStylePattern(index =>
        {
            if (index % 2 == 0 && evenBackground != null)
                return new RowStyle { BackgroundColor = evenBackground };
            if (index % 2 != 0 && oddBackground != null)
                return new RowStyle { BackgroundColor = oddBackground };
            return null;
        });
    }

    public static RowStylePattern EveryNth(
        int n,
        string backgroundColor)
    {
        return new RowStylePattern(index =>
            index % n == 0 ? new RowStyle { BackgroundColor = backgroundColor } : null);
    }

    public static RowStylePattern Custom(Func<int, RowStyle?> selector)
    {
        return new RowStylePattern(selector);
    }
}

public sealed class RowStyle
{
    public string? BackgroundColor { get; set; }
    public BorderStyle? Border { get; set; }
}

public static class BUIRowPatternPresets
{
    public static RowStylePattern Striped =>
        RowStylePattern.Alternating(
            evenBackground: "var(--_dc-header-bg)",
            oddBackground: null);

    public static RowStylePattern StripedReversed =>
        RowStylePattern.Alternating(
            evenBackground: null,
            oddBackground: "var(--_dc-header-bg)");

    public static RowStylePattern Every3rd =>
        RowStylePattern.EveryNth(3, "var(--_dc-header-bg)");

    public static RowStylePattern Every4th =>
        RowStylePattern.EveryNth(4, "var(--_dc-header-bg)");

    public static RowStylePattern Every5th =>
        RowStylePattern.EveryNth(5, "var(--_dc-header-bg)");
}