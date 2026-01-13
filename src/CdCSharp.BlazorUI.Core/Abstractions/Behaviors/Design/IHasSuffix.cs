using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

public interface IHasSuffix
{
    string? SuffixText { get; set; }
    string? SuffixIcon { get; set; }
    CssColor? SuffixColor { get; set; }
    CssColor? SuffixBackgroundColor { get; set; }
}
