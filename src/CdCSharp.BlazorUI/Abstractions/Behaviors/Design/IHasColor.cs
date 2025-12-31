using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Abstractions.Behaviors.Design;

public interface IHasColor
{
    CssColor? Color { get; set; }
}
