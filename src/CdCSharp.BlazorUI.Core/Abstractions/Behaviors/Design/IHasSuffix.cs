namespace CdCSharp.BlazorUI.Components;

public interface IHasSuffix
{
    string? SuffixText { get; set; }
    string? SuffixIcon { get; set; }
    CssColor? SuffixColor { get; set; }
    CssColor? SuffixBackgroundColor { get; set; }
}
