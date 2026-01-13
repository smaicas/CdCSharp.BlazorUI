using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Layout;

public sealed class BUIToastVariant : Variant
{
    public static readonly BUIToastVariant Default = new("Default");

    private BUIToastVariant(string name) : base(name)
    {
    }

    public static BUIToastVariant Custom(string name) => new(name);
}