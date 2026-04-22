namespace CdCSharp.BlazorUI.Components.Forms;

public sealed class BUIInputVariant : Variant
{
    public static readonly BUIInputVariant Filled = new("Filled");
    public static readonly BUIInputVariant Outlined = new("Outlined");
    public static readonly BUIInputVariant Standard = new("Standard");
    public static readonly BUIInputVariant Flat = new("Flat");

    public BUIInputVariant(string name) : base(name)
    {
    }

    public static BUIInputVariant Custom(string name) => new(name);
}