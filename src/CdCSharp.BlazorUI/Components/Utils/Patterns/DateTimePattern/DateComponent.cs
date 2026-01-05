namespace CdCSharp.BlazorUI.Components.Utils.Patterns.DateTimePattern;

public sealed class DateComponent
{
    public DateComponentType Type { get; set; }
    public int MinDigits { get; set; }
    public int MaxDigits { get; set; }
    public string? SeparatorValue { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
}
