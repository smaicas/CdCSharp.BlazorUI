using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

public interface IHasBackgroundColor
{
    CssColor? BackgroundColor { get; set; }
}