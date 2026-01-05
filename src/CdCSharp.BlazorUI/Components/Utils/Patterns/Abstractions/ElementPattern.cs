namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public sealed class ElementPattern
{
    public ElementPattern(
        string pattern,
        string value,
        int length,
        string defaultValue,
        bool isSeparator,
        bool isEditable)
    {
        Pattern = pattern;
        Value = value;
        Length = length;
        DefaultValue = defaultValue;
        IsSeparator = isSeparator;
        IsEditable = isEditable;
    }

    public string Pattern { get; set; }
    public string Value { get; set; }
    public int Length { get; set; }
    public string DefaultValue { get; set; }
    public bool IsSeparator { get; set; }
    public bool IsEditable { get; set; }
}
