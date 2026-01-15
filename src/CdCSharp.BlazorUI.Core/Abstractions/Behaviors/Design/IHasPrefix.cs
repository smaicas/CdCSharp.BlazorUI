namespace CdCSharp.BlazorUI.Components;

public interface IHasPrefix
{
    string? PrefixText { get; set; }
    string? PrefixIcon { get; set; }
    CssColor? PrefixColor { get; set; }
    CssColor? PrefixBackgroundColor { get; set; }
}
