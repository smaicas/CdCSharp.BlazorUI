namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public sealed class SpanState
{
    public int Index { get; set; }
    public string Value { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public bool IsSeparator { get; set; }
    public bool IsEditable { get; set; }
    public int MaxLength { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public bool HasFocus { get; set; }

    public bool IsComplete => !IsEditable || Value.Length == MaxLength;
    public string DisplayValue => string.IsNullOrEmpty(Value) ? DefaultValue : Value;
}
