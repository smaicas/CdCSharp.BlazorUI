using CdCSharp.BlazorUI.Components;

namespace CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;

public interface IHasDensity
{
    DensityEnum Density { get; set; }
}