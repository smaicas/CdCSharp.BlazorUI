using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Server;

public class BUICultureSelectorVariant : Variant
{
    public static readonly BUICultureSelectorVariant Dropdown = new("Dropdown");
    public static readonly BUICultureSelectorVariant Flags = new("Flags");

    public BUICultureSelectorVariant(string name) : base(name)
    {
    }

    public static BUICultureSelectorVariant Custom(string name) => new(name);
}
