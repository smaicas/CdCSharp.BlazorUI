using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components;

public class BUITableVariant : Variant
{
    public static readonly BUITableVariant Default = new("Default");
    public static readonly BUITableVariant Striped = new("Striped");
    public static readonly BUITableVariant Bordered = new("Bordered");
    public static readonly BUITableVariant Cards = new("Cards");

    public BUITableVariant(string name) : base(name)
    {
    }

    public static BUITableVariant Custom(string name) => new(name);
}
