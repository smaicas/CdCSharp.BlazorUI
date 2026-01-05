using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputDropdownVariant : Variant
{
    public static readonly BUIInputDropdownVariant Outlined = new("Outlined");
    public static readonly BUIInputDropdownVariant Filled = new("Filled");
    public static readonly BUIInputDropdownVariant Standard = new("Standard");

    public BUIInputDropdownVariant(string name) : base(name) { }

    public static BUIInputDropdownVariant Custom(string name) => new(name);
}
