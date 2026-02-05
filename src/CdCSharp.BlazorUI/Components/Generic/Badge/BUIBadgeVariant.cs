namespace CdCSharp.BlazorUI.Components;

public class BUIBadgeVariant : Variant
{
    public static readonly BUIBadgeVariant Default = new("Default");

    public BUIBadgeVariant(string name) : base(name)
    {
    }

    public static BUIBadgeVariant Custom(string name) => new(name);
}