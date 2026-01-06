namespace CdCSharp.BlazorUI.Components.Utils.Patterns.Abstractions;

public sealed class PatternState
{
    public List<SpanState> Spans { get; set; } = [];

    public bool IsComplete => Spans
        .Where(s => s.IsEditable)
        .All(s => s.IsComplete && !string.IsNullOrEmpty(s.Value) && s.Value != s.Placeholder);

    public string GetFullText()
    {
        return string.Join("", Spans.Select(s => s.DisplayValue));
    }

    public string GetActualText()
    {
        if (!IsComplete) return string.Empty;

        return string.Join("", Spans.Select(s =>
            s.IsEditable ? s.Value : s.Placeholder
        ));
    }
}