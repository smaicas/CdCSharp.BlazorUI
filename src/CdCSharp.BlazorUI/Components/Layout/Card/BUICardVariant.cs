namespace CdCSharp.BlazorUI.Components.Layout;

public class BUICardVariant : Variant
{
    public static readonly BUICardVariant Flat = new("Flat");
    public static readonly BUICardVariant Outlined = new("Outlined");

    public BUICardVariant(string name) : base(name)
    {
    }

    public static BUICardVariant Custom(string name) => new(name);
}