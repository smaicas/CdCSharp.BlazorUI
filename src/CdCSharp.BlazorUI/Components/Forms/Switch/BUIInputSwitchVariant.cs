using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputSwitchVariant : Variant
{
    public static readonly BUIInputSwitchVariant Default = new("Default");

    public BUIInputSwitchVariant(string name) : base(name) { }

    public static BUIInputSwitchVariant Custom(string name) => new(name);
}
