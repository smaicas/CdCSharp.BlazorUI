using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Forms;

public class BUIInputRadioVariant : Variant
{
    public static readonly BUIInputRadioVariant Default = new("Default");

    public BUIInputRadioVariant(string name) : base(name) { }

    public static BUIInputRadioVariant Custom(string name) => new(name);
}
