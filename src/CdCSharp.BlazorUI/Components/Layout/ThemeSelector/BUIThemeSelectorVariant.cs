using CdCSharp.BlazorUI.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components.Layout;

public class BUIThemeSelectorVariant : Variant
{
    public static readonly BUIThemeSelectorVariant Default = new("Default");

    public static readonly BUIThemeSelectorVariant SunMoon = new("SunMoon");

    public BUIThemeSelectorVariant(string name) : base(name)
    {
    }

    public static BUIThemeSelectorVariant Custom(string name) => new(name);
}