namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public sealed class PatternState
{
    public List<SpanState> Spans { get; set; } = [];

    public bool IsComplete => Spans
        .Where(s => s.IsEditable)
        .All(s => s.IsComplete && !string.IsNullOrEmpty(s.Value) && s.Value != s.DefaultValue);

    public string GetFullText()
    {
        return string.Join("", Spans.Select(s => s.DisplayValue));
    }

    public string GetActualText()
    {
        // Only return text if all editable spans have real values (not defaults)
        bool hasAllValues = Spans
            .Where(s => s.IsEditable)
            .All(s => !string.IsNullOrEmpty(s.Value) && s.Value != s.DefaultValue);

        if (!hasAllValues)
            return string.Empty;

        return string.Join("", Spans.Select(s =>
            s.IsSeparator ? s.DefaultValue : s.Value
        ));
    }
}