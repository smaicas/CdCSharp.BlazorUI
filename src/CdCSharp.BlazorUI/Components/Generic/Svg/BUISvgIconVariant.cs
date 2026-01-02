using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components;

public class BUISvgIconVariant : Variant
{
    public static readonly BUISvgIconVariant Default = new("Default");

    public BUISvgIconVariant(string name) : base(name)
    {
    }

    public static BUISvgIconVariant Custom(string name) => new(name);
}