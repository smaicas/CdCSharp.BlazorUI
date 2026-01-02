namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Transitions;

public interface IHasTransitions
{
    BUITransitions? Transitions { get; set; }
}