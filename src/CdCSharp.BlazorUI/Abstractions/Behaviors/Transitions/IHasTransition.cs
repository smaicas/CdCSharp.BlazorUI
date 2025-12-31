namespace CdCSharp.BlazorUI.Abstractions.Behaviors.Transitions;

public interface IHasTransitions
{
    BUITransitions? Transitions { get; set; }
}