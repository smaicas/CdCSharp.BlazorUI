using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputSelectVariant : Variant
{
    public static readonly BUIInputSelectVariant Filled = new("Filled");
    public static readonly BUIInputSelectVariant Outlined = new("Outlined");
    public static readonly BUIInputSelectVariant Standard = new("Standard");

    public BUIInputSelectVariant(string name) : base(name)
    {
    }

    public static BUIInputSelectVariant Custom(string name) => new(name);
}