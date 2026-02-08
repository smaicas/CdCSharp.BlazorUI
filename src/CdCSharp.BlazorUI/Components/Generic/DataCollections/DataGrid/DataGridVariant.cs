namespace CdCSharp.BlazorUI.Components;

public class DataGridVariant : Variant
{
    public static readonly DataGridVariant Default = new("Default");

    public DataGridVariant(string name) : base(name) { }

    public static DataGridVariant Custom(string name) => new(name);
}