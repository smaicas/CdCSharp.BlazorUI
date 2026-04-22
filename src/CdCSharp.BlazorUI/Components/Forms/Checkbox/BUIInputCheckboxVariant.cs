namespace CdCSharp.BlazorUI.Components.Forms;

public sealed class BUIInputCheckboxVariant : Variant
{
    public static readonly BUIInputCheckboxVariant Default = new("Default");

    public BUIInputCheckboxVariant(string name) : base(name)
    {
    }

    public static BUIInputCheckboxVariant Custom(string name) => new(name);
}