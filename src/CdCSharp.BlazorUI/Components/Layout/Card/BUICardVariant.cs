using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Layout;

public class BUICardVariant : Variant
{
    public static readonly BUICardVariant Elevated = new("Elevated");
    public static readonly BUICardVariant Outlined = new("Outlined");
    public static readonly BUICardVariant Flat = new("Flat");

    public BUICardVariant(string name) : base(name)
    {
    }

    public static BUICardVariant Custom(string name) => new(name);
}
