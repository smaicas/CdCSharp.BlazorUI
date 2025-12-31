using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Abstractions.Behaviors.Design;

public interface IHasBackgroundColor
{
    CssColor? BackgroundColor { get; set; }
}