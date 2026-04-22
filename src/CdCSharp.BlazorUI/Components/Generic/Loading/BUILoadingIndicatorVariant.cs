namespace CdCSharp.BlazorUI.Components;

public sealed class BUILoadingIndicatorVariant : Variant
{
    public static readonly BUILoadingIndicatorVariant Spinner = new("Spinner");
    public static readonly BUILoadingIndicatorVariant CircularProgress = new("CircularProgress");
    public static readonly BUILoadingIndicatorVariant Ring = new("Ring");
    public static readonly BUILoadingIndicatorVariant Dots = new("Dots");
    public static readonly BUILoadingIndicatorVariant Bars = new("Bars");
    public static readonly BUILoadingIndicatorVariant LinearIndeterminate = new("LinearIndeterminate");

    public BUILoadingIndicatorVariant(string name) : base(name)
    {
    }

    public static BUILoadingIndicatorVariant Custom(string name) => new(name);
}