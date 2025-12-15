using CdCSharp.BlazorUI.Core.Variants;

namespace CdCSharp.BlazorUI.Components.Generic.Svg;

public class UISvgIconVariant : Variant
{
    public UISvgIconVariant(string name) : base(name)
    {
    }
    public static readonly UISvgIconVariant Default = new("Default");

    public static UISvgIconVariant Custom(string name) => new(name);
}

