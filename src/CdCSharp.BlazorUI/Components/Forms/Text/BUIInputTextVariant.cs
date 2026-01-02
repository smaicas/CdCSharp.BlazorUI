using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputTextVariant : Variant
{
    public static readonly BUIInputTextVariant Outlined = new("Outlined");
    public static readonly BUIInputTextVariant Filled = new("Filled");
    public static readonly BUIInputTextVariant Standard = new("Standard");

    public BUIInputTextVariant(string name) : base(name) { }

    public static BUIInputTextVariant Custom(string name) => new(name);
}
