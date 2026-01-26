namespace CdCSharp.BlazorUI.Components;

public class DataCardsVariant : Variant
{
    public static readonly DataCardsVariant Default = new("Default");
    public static readonly DataCardsVariant Outlined = new("Outlined");

    public DataCardsVariant(string name) : base(name) { }

    public static DataCardsVariant Custom(string name) => new(name);
}