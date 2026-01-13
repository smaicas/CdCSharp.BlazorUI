using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components;

public class BUIBadgeVariant : Variant
{
    public static readonly BUIBadgeVariant Dot = new("Dot");
    public static readonly BUIBadgeVariant Filled = new("Filled");
    public static readonly BUIBadgeVariant Outlined = new("Outlined");
    public static readonly BUIBadgeVariant Soft = new("Soft");

    public BUIBadgeVariant(string name) : base(name)
    {
    }

    public static BUIBadgeVariant Custom(string name) => new(name);
}