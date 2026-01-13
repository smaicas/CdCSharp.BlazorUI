using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputDropdownTreeVariant : Variant
{
    public static readonly BUIInputDropdownTreeVariant Filled = new("Filled");
    public static readonly BUIInputDropdownTreeVariant Outlined = new("Outlined");
    public static readonly BUIInputDropdownTreeVariant Standard = new("Standard");

    public BUIInputDropdownTreeVariant(string name) : base(name)
    {
    }

    public static BUIInputDropdownTreeVariant Custom(string name) => new(name);
}