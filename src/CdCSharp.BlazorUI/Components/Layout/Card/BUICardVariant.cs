namespace CdCSharp.BlazorUI.Components.Layout;

public class BUICardVariant : Variant
{
    public static readonly BUICardVariant Default = new("Default");

    public BUICardVariant(string name) : base(name)
    {
    }

    public static BUICardVariant Custom(string name) => new(name);
}