namespace CdCSharp.BlazorUI.Components.Features.Common;

public interface IHasDensity<TDensity> where TDensity : Enum
{
    TDensity Density { get; set; }
}