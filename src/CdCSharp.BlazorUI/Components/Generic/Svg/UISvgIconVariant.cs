using CdCSharp.BlazorUI.Components.Abstractions;

namespace CdCSharp.BlazorUI.Components.Generic.Svg;

public class UISvgIconVariant : Variant
{
    public static readonly UISvgIconVariant Default = new("Default");

    public UISvgIconVariant(string name) : base(name)
    {
    }

    public static UISvgIconVariant Custom(string name) => new(name);
}