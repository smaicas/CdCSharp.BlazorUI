using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Components;

public interface IHasRipple : IJsBehavior
{
    bool DisableRipple { get; set; }
    CssColor? RippleColor { get; set; }
    int? RippleDurationMs { get; set; }

    ElementReference GetRippleContainer();
}