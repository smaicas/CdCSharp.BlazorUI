using CdCSharp.BlazorUI.Core.Abstractions.Components.Variants;

namespace CdCSharp.BlazorUI.Components;

public class BUILoadingIndicatorVariant : Variant
{
    public static readonly BUILoadingIndicatorVariant CircularProgress = new("CircularProgress");
    public static readonly BUILoadingIndicatorVariant Dots = new("Dots");
    public static readonly BUILoadingIndicatorVariant LinearIndeterminate = new("LinearIndeterminate");
    public static readonly BUILoadingIndicatorVariant Pulse = new("Pulse");
    public static readonly BUILoadingIndicatorVariant Spinner = new("Spinner");

    public BUILoadingIndicatorVariant(string name) : base(name)
    {
    }

    public static BUILoadingIndicatorVariant Custom(string name) => new(name);
}