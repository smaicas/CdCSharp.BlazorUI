namespace CdCSharp.BlazorUI.Components;

/// <summary>
/// Defines a styling pattern for rows (DataGrid) or cards (DataCards).
/// CSS-expressible patterns set container-level variables once; custom patterns compute per-item.
/// </summary>
public abstract class RowStylePattern
{
    internal abstract bool IsCssExpressible { get; }
    internal abstract string? GetPatternDataAttribute();
    internal abstract Dictionary<string, string> GetContainerCssVariables();
    internal abstract string? GetItemInlineStyle(int index);

    public static RowStylePattern Alternating(
        string? evenBackground = null,
        string? oddBackground = null)
        => new AlternatingRowStylePattern(evenBackground, oddBackground);

    public static RowStylePattern EveryNth(int n, string backgroundColor)
        => new EveryNthRowStylePattern(n, backgroundColor);

    public static RowStylePattern All(string backgroundColor)
        => new AllRowStylePattern(backgroundColor);

    public static RowStylePattern Custom(Func<int, RowStyle?> selector)
        => new CustomRowStylePattern(selector);
}

internal sealed class AlternatingRowStylePattern : RowStylePattern
{
    private readonly string? _evenBackground;
    private readonly string? _oddBackground;

    internal AlternatingRowStylePattern(string? evenBackground, string? oddBackground)
    {
        _evenBackground = evenBackground;
        _oddBackground = oddBackground;
    }

    internal override bool IsCssExpressible => true;

    internal override string? GetPatternDataAttribute() => "alternating";

    internal override Dictionary<string, string> GetContainerCssVariables()
    {
        Dictionary<string, string> vars = [];
        if (_evenBackground != null)
            vars["--bui-inline-row-pattern-even-bg"] = _evenBackground;
        if (_oddBackground != null)
            vars["--bui-inline-row-pattern-odd-bg"] = _oddBackground;
        return vars;
    }

    internal override string? GetItemInlineStyle(int index)
    {
        if (index % 2 == 0 && _evenBackground != null)
            return $"--bui-inline-row-pattern-bg: {_evenBackground}";
        if (index % 2 != 0 && _oddBackground != null)
            return $"--bui-inline-row-pattern-bg: {_oddBackground}";
        return null;
    }
}

internal sealed class EveryNthRowStylePattern : RowStylePattern
{
    private static readonly HashSet<int> SupportedCssValues = [3, 4, 5];
    private readonly string _backgroundColor;
    private readonly int _n;

    internal EveryNthRowStylePattern(int n, string backgroundColor)
    {
        _n = n;
        _backgroundColor = backgroundColor;
    }

    internal override bool IsCssExpressible => SupportedCssValues.Contains(_n);

    internal override string? GetPatternDataAttribute() =>
        IsCssExpressible ? $"every-{_n}" : null;

    internal override Dictionary<string, string> GetContainerCssVariables()
    {
        if (!IsCssExpressible) return [];
        return new Dictionary<string, string>
        {
            ["--bui-inline-row-pattern-nth-bg"] = _backgroundColor
        };
    }

    internal override string? GetItemInlineStyle(int index)
    {
        if (index % _n == 0)
            return $"--bui-inline-row-pattern-bg: {_backgroundColor}";
        return null;
    }
}

internal sealed class AllRowStylePattern : RowStylePattern
{
    private readonly string _backgroundColor;

    internal AllRowStylePattern(string backgroundColor)
    {
        _backgroundColor = backgroundColor;
    }

    internal override bool IsCssExpressible => true;

    internal override string? GetPatternDataAttribute() => "all";

    internal override Dictionary<string, string> GetContainerCssVariables()
    {
        return new Dictionary<string, string>
        {
            ["--bui-inline-row-pattern-all-bg"] = _backgroundColor
        };
    }

    internal override string? GetItemInlineStyle(int index) =>
        $"--bui-inline-row-pattern-bg: {_backgroundColor}";
}

internal sealed class CustomRowStylePattern : RowStylePattern
{
    private readonly Func<int, RowStyle?> _selector;

    internal CustomRowStylePattern(Func<int, RowStyle?> selector)
    {
        _selector = selector;
    }

    internal override bool IsCssExpressible => false;

    internal override string? GetPatternDataAttribute() => null;

    internal override Dictionary<string, string> GetContainerCssVariables() => [];

    internal override string? GetItemInlineStyle(int index)
    {
        RowStyle? style = _selector(index);
        return style?.ToInlineVariables();
    }
}

public sealed class RowStyle
{
    public string? BackgroundColor { get; set; }
    public BorderStyle? Border { get; set; }

    internal string? ToInlineVariables()
    {
        List<string> vars = [];

        if (BackgroundColor != null)
            vars.Add($"--bui-inline-row-pattern-bg: {BackgroundColor}");

        if (Border != null)
        {
            BorderCssValues values = Border.GetCssValues();
            if (values.All != null)
                vars.Add($"--bui-inline-row-pattern-border: {values.All}");
            if (values.Top != null)
                vars.Add($"--bui-inline-row-pattern-border-top: {values.Top}");
            if (values.Right != null)
                vars.Add($"--bui-inline-row-pattern-border-right: {values.Right}");
            if (values.Bottom != null)
                vars.Add($"--bui-inline-row-pattern-border-bottom: {values.Bottom}");
            if (values.Left != null)
                vars.Add($"--bui-inline-row-pattern-border-left: {values.Left}");
        }

        return vars.Count > 0 ? string.Join("; ", vars) : null;
    }
}

public static class BUIRowPatternPresets
{
    public static RowStylePattern Striped =>
        RowStylePattern.Alternating(
            evenBackground: "var(--_dc-header-bg)");

    public static RowStylePattern StripedReversed =>
        RowStylePattern.Alternating(
            oddBackground: "var(--_dc-header-bg)");

    public static RowStylePattern Every3rd =>
        RowStylePattern.EveryNth(3, "var(--_dc-header-bg)");

    public static RowStylePattern Every4th =>
        RowStylePattern.EveryNth(4, "var(--_dc-header-bg)");

    public static RowStylePattern Every5th =>
        RowStylePattern.EveryNth(5, "var(--_dc-header-bg)");
}