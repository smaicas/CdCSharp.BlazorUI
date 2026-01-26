namespace CdCSharp.BlazorUI.Components;

public class DataGridVariant : Variant
{
    public static readonly DataGridVariant Default = new("Default");
    public static readonly DataGridVariant Striped = new("Striped");
    public static readonly DataGridVariant Bordered = new("Bordered");
    public static readonly DataGridVariant StripedBordered = new("StripedBordered");

    public DataGridVariant(string name) : base(name) { }

    public static DataGridVariant Custom(string name) => new(name);
}