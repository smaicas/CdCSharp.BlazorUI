namespace CdCSharp.BlazorUI.Components.Features.Loading;

public interface IHasLoading
{
    bool IsLoading { get; set; }
    UILoadingIndicatorVariant? LoadingIndicatorVariant { get; set; }
}

public enum UILoadingIndicatorType
{
    Spinner,
    LinearIndeterminate,
    CircularProgress,
    Dots,
    Pulse
}