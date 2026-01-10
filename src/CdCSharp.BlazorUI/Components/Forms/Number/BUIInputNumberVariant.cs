using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputNumberVariant : Variant
{
    public static readonly BUIInputNumberVariant Outlined = new("Outlined");
    public static readonly BUIInputNumberVariant Filled = new("Filled");
    public static readonly BUIInputNumberVariant Standard = new("Standard");

    public BUIInputNumberVariant(string name) : base(name) { }

    public static BUIInputNumberVariant Custom(string name) => new(name);
}
