using CdCSharp.BlazorUI.Core.Css;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;

public interface IHasRipple : IJsBehavior
{
    bool DisableRipple { get; set; }
    CssColor? RippleColor { get; set; }
    int? RippleDurationMs { get; set; }

    ElementReference GetRippleContainer();
}