using CdCSharp.BlazorUI.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components;

public class BUIButtonVariant : Variant
{
    public static readonly BUIButtonVariant Default = new("Default");

    public BUIButtonVariant(string name) : base(name)
    {
    }

    public static BUIButtonVariant Custom(string name) => new(name);
}