namespace CdCSharp.BlazorUI.Components.Utils.Patterns.DateTimePattern;

public sealed class ParsedDatePattern
{
    public string OriginalFormat { get; set; } = string.Empty;
    public string RegexPattern { get; set; } = string.Empty;
    public List<DateComponent> Components { get; set; } = [];
}
