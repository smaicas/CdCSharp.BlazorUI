namespace CdCSharp.BlazorUI.Core.Transitions;

public interface IHasTransitions
{
    UITransitions? Transitions { get; set; }
}