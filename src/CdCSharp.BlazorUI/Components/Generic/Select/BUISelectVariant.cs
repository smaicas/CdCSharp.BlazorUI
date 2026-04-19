namespace CdCSharp.BlazorUI.Components;

public sealed class BUISelectVariant : Variant
{
    public static readonly BUISelectVariant Default = new("default");

    private BUISelectVariant(string value) : base(value) { }
}
