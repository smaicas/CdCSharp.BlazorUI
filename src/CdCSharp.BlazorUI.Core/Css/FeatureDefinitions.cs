namespace CdCSharp.BlazorUI.Core.Css;

/// <summary>
/// Central registry of all data attributes, CSS variables and class names used by features
/// </summary>
public static class FeatureDefinitions
{
    public static class Tags
    {
        public const string Component = "bui-component";
    }

    /// <summary>
    /// Data attribute names used by features
    /// </summary>
    public static class DataAttributes
    {
        // Component identification
        public const string Component = "data-bui-component";
        public const string Variant = "data-bui-variant";

        // Common features
        public const string Size = "data-bui-size";
        public const string Density = "data-bui-density";
        public const string FullWidth = "data-bui-fullwidth";
        public const string Elevation = "data-bui-elevation";
        public const string Transitions = "data-bui-transitions";

        // State features
        public const string Loading = "data-bui-loading";
        public const string Disabled = "data-bui-disabled";
        public const string ReadOnly = "data-bui-readonly";
        public const string Required = "data-bui-required";
        public const string Error = "data-bui-error";

        // Behavior features
        public const string Ripple = "data-bui-ripple";
    }

    /// <summary>
    /// CSS variable names used by features
    /// </summary>
    public static class CssVariables
    {
        // Colors
        public const string BackgroundColor = "--bui-bg-color";
        public const string Color = "--bui-color";

        // Ripple
        public const string RippleColor = "--bui-ripple-color";
        public const string RippleDuration = "--bui-ripple-duration";

        // Borders - General
        public const string BorderWidth = "--bui-border-width";
        public const string BorderStyle = "--bui-border-style";
        public const string BorderColor = "--bui-border-color";
        public const string BorderRadius = "--bui-border-radius";

        // Borders - Individual sides
        public const string BorderTopWidth = "--bui-border-top-width";
        public const string BorderTopStyle = "--bui-border-top-style";
        public const string BorderTopColor = "--bui-border-top-color";

        public const string BorderRightWidth = "--bui-border-right-width";
        public const string BorderRightStyle = "--bui-border-right-style";
        public const string BorderRightColor = "--bui-border-right-color";

        public const string BorderBottomWidth = "--bui-border-bottom-width";
        public const string BorderBottomStyle = "--bui-border-bottom-style";
        public const string BorderBottomColor = "--bui-border-bottom-color";

        public const string BorderLeftWidth = "--bui-border-left-width";
        public const string BorderLeftStyle = "--bui-border-left-style";
        public const string BorderLeftColor = "--bui-border-left-color";

        // Transitions
        public const string TransitionDuration = "--bui-transition-duration";
        public const string TransitionEasing = "--bui-transition-easing";
        public const string TransitionDelay = "--bui-transition-delay";

        // Layout
        public const string DensitySpacingMultiplier = "--bui-density-spacing-multiplier";
    }

    /// <summary>
    /// CSS class names for component parts (BEM style)
    /// </summary>
    public static class CssClasses
    {
        // Ripple effect
        public const string Ripple = "bui-ripple";

        // Button parts
        public const string ButtonText = "bui-button__text";

        // Input parts
        public const string InputLabel = "bui-input__label";
        public const string InputContainer = "bui-input__container";
        public const string InputRequired = "bui-input__required";
        public const string InputLoading = "bui-input__loading";
        public const string InputValidation = "bui-input__validation";
        public const string InputHelperText = "bui-input__helper-text";

        // Loading indicator parts
        public const string LoadingSpinner = "bui-loading-spinner";
        public const string LoadingLinear = "bui-loading-linear";
        public const string LoadingLinearBar = "bui-loading-linear__bar";
        public const string LoadingCircular = "bui-loading-circular";
        public const string LoadingDot = "bui-loading-dot";
        public const string LoadingPulse = "bui-loading-pulse";
    }

    /// <summary>
    /// Size values
    /// </summary>
    public static class SizeValues
    {
        public const string Small = "small";
        public const string Medium = "medium";
        public const string Large = "large";
    }

    /// <summary>
    /// Density values
    /// </summary>
    public static class DensityValues
    {
        public const string Comfortable = "comfortable";
        public const string Standard = "standard";
        public const string Compact = "compact";
    }
}