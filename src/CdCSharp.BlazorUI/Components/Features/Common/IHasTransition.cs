using CdCSharp.BlazorUI.Components.Features.Transitions;

namespace CdCSharp.BlazorUI.Components.Features.Common;

public interface IHasTransitions
{
    UITransitions? Transitions { get; set; }
}