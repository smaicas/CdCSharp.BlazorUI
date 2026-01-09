using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputTextAreaVariant : Variant
{
    public static readonly BUIInputTextAreaVariant Outlined = new("Outlined");
    public static readonly BUIInputTextAreaVariant Filled = new("Filled");
    public static readonly BUIInputTextAreaVariant Standard = new("Standard");

    public BUIInputTextAreaVariant(string name) : base(name) { }

    public static BUIInputTextAreaVariant Custom(string name) => new(name);
}
