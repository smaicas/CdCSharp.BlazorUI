namespace CdCSharp.BlazorUI.Components.Features.Common;

public interface IHasDensity
{
    DensityEnum Density { get; set; }
}

public enum DensityEnum
{
    Comfortable,
    Standard,
    Compact
}