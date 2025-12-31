using CdCSharp.BlazorUI.Components;

namespace CdCSharp.BlazorUI.Abstractions.Behaviors.Design;

public interface IHasDensity
{
    DensityEnum Density { get; set; }
}