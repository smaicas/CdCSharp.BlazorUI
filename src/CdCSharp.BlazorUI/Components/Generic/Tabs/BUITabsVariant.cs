namespace CdCSharp.BlazorUI.Components;

public sealed class BUITabsVariant : Variant
{
    public static readonly BUITabsVariant Underline = new("Underline");
    public static readonly BUITabsVariant Pills = new("Pills");
    public static readonly BUITabsVariant Enclosed = new("Enclosed");

    public BUITabsVariant(string name) : base(name)
    {
    }

    public static BUITabsVariant Custom(string name) => new(name);
}