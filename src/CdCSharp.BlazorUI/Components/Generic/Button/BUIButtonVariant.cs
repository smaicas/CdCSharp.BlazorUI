namespace CdCSharp.BlazorUI.Components;

public sealed class BUIButtonVariant : Variant
{
    public static readonly BUIButtonVariant Default = new("Default");

    public BUIButtonVariant(string name) : base(name)
    {
    }

    public static BUIButtonVariant Custom(string name) => new(name);
}