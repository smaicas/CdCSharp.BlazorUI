namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public sealed class SpanState
{
    public int Index { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
    public int MaxLength { get; set; }
    public string AllowedChars { get; set; } = string.Empty; // "d" = digits, "w" = letters, "" = any

    // Validator for complete value
    public Func<string, bool>? Validator { get; set; }

    public bool IsComplete => !IsEditable || Value.Length == MaxLength;
    public string DisplayValue => string.IsNullOrEmpty(Value) ? Placeholder : Value;
}
