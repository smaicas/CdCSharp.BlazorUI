using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

public interface IHasElevation
{
    int? Elevation { get; set; }
    CssColor? ElevationShadowColor { get; set; }
}