namespace CdCSharp.BlazorUI.Components;

public interface IHasPrefix
{
    string? PrefixText { get; set; }
    string? PrefixIcon { get; set; }
    string? PrefixColor { get; set; }
    string? PrefixBackgroundColor { get; set; }
}
