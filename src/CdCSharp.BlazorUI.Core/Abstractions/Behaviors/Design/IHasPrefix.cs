using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

public interface IHasPrefix
{
    string? PrefixText { get; set; }
    string? PrefixIcon { get; set; }
    CssColor? PrefixColor { get; set; }
    CssColor? PrefixBackgroundColor { get; set; }
}
