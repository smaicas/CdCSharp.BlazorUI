using CdCSharp.BlazorUI.Core.Css;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;

public interface IHasRipple : IJsBehavior
{
    bool DisableRipple { get; set; }
    CssColor? RippleColor { get; set; }
    int? RippleDuration { get; set; }
}