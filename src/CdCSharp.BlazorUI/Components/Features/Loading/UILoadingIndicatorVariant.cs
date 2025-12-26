using CdCSharp.BlazorUI.Components.Abstractions;

namespace CdCSharp.BlazorUI.Components.Features.Loading;

public class UILoadingIndicatorVariant : Variant
{
    public static readonly UILoadingIndicatorVariant Spinner = new("Spinner");
    public static readonly UILoadingIndicatorVariant LinearIndeterminate = new("LinearIndeterminate");
    public static readonly UILoadingIndicatorVariant CircularProgress = new("CircularProgress");
    public static readonly UILoadingIndicatorVariant Dots = new("Dots");
    public static readonly UILoadingIndicatorVariant Pulse = new("Pulse");

    public UILoadingIndicatorVariant(string name) : base(name)
    {
    }

    public static UILoadingIndicatorVariant Custom(string name) => new(name);
}
