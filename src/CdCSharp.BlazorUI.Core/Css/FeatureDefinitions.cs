namespace CdCSharp.BlazorUI.Core.Css;

/// <summary>
/// Central registry of all data attributes, CSS variables and class names used by features
/// </summary>
public static class FeatureDefinitions
{
    /// <summary>
    /// Data attribute names used by features
    /// </summary>
    public static class DataAttributes
    {
        // Component identification
        public const string Component = "data-ui-component";
        public const string Variant = "data-ui-variant";

        // Common features
        public const string Size = "data-ui-size";
        public const string Density = "data-ui-density";
        public const string FullWidth = "data-ui-fullwidth";
        public const string Elevation = "data-ui-elevation";
        public const string Transitions = "data-ui-transitions";

        // State features
        public const string Loading = "data-ui-loading";
        public const string Disabled = "data-ui-disabled";
        public const string ReadOnly = "data-ui-readonly";
        public const string Required = "data-ui-required";
        public const string Error = "data-ui-error";

        // Behavior features
        public const string Ripple = "data-ui-ripple";
    }

    /// <summary>
    /// CSS variable names used by features
    /// </summary>
    public static class CssVariables
    {
        // Colors
        public const string BackgroundColor = "--ui-bg-color";
        public const string Color = "--ui-color";

        // Ripple
        public const string RippleColor = "--ui-ripple-color";
        public const string RippleDuration = "--ui-ripple-duration";

        // Borders - General
        public const string BorderWidth = "--ui-border-width";
        public const string BorderStyle = "--ui-border-style";
        public const string BorderColor = "--ui-border-color";
        public const string BorderRadius = "--ui-border-radius";

        // Borders - Individual sides
        public const string BorderTopWidth = "--ui-border-top-width";
        public const string BorderTopStyle = "--ui-border-top-style";
        public const string BorderTopColor = "--ui-border-top-color";

        public const string BorderRightWidth = "--ui-border-right-width";
        public const string BorderRightStyle = "--ui-border-right-style";
        public const string BorderRightColor = "--ui-border-right-color";

        public const string BorderBottomWidth = "--ui-border-bottom-width";
        public const string BorderBottomStyle = "--ui-border-bottom-style";
        public const string BorderBottomColor = "--ui-border-bottom-color";

        public const string BorderLeftWidth = "--ui-border-left-width";
        public const string BorderLeftStyle = "--ui-border-left-style";
        public const string BorderLeftColor = "--ui-border-left-color";

        // Transitions
        public const string TransitionDuration = "--ui-transition-duration";
        public const string TransitionEasing = "--ui-transition-easing";
        public const string TransitionDelay = "--ui-transition-delay";

        // Layout
        public const string DensitySpacingMultiplier = "--ui-density-spacing-multiplier";
    }

    /// <summary>
    /// CSS class names for component parts (BEM style)
    /// </summary>
    public static class CssClasses
    {
        // Ripple effect
        public const string Ripple = "ui-ripple";

        // Button parts
        public const string ButtonText = "ui-button__text";

        // Input parts
        public const string InputLabel = "ui-input__label";
        public const string InputContainer = "ui-input__container";
        public const string InputRequired = "ui-input__required";
        public const string InputLoading = "ui-input__loading";
        public const string InputValidation = "ui-input__validation";
        public const string InputHelperText = "ui-input__helper-text";

        // Loading indicator parts
        public const string LoadingSpinner = "ui-loading-spinner";
        public const string LoadingLinear = "ui-loading-linear";
        public const string LoadingLinearBar = "ui-loading-linear__bar";
        public const string LoadingCircular = "ui-loading-circular";
        public const string LoadingDot = "ui-loading-dot";
        public const string LoadingPulse = "ui-loading-pulse";
    }
}