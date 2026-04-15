namespace CdCSharp.BlazorUI.Components;

/// <summary>
/// Central definition of all CSS-related constants used by the BUI component library. Organized by
/// purpose and usage context.
/// </summary>
public static class FeatureDefinitions
{
    public static class ComponentVariables
    {
        /// <summary>
        /// Density-related variables set by [data-bui-density]. Affects spacing between elements
        /// via gap.
        /// </summary>
        public static class Density
        {
            public static string Multiplier => "--bui-density-multiplier";
        }

        /// <summary>
        /// Size-related variables set by [data-bui-size]. Only the multiplier is set globally;
        /// components use it in their isolated CSS.
        /// </summary>
        public static class Size
        {
            /// <summary>
            /// Scale factor for size calculations. Small=0.85, Medium=1, Large=1.25 Components
            /// multiply their dimensions by this value.
            /// </summary>
            public const string Multiplier = "--bui-size-multiplier";
        }
    }

    public static class CssClasses
    {
        /// <summary>
        /// Ripple effect element class.
        /// </summary>
        public const string Ripple = "bui-ripple";

        /// <summary>
        /// Visually hidden but accessible to screen readers.
        /// </summary>
        public const string SrOnly = "sr-only";

        /// <summary>
        /// Input family component class names.
        /// </summary>
        public static class Input
        {
            public const string Wrapper = "bui-input__wrapper";
            public const string Field = "bui-input__field";
            public const string Label = "bui-input__label";
            public const string Required = "bui-input__required";
            public const string HelperText = "bui-input__helper-text";
            public const string Validation = "bui-input__validation";

            public const string AddonPrefix = "bui-input__addon--prefix";
            public const string AddonSuffix = "bui-input__addon--suffix";

            public const string Outline = "bui-input__outline";
            public const string OutlineLeading = "bui-input__outline-leading";
            public const string OutlineNotch = "bui-input__outline-notch";
            public const string OutlineTrailing = "bui-input__outline-trailing";
        }

        /// <summary>
        /// Picker family component class names.
        /// </summary>
        public static class Picker
        {
            public const string Row = "bui-picker__row";
            public const string Title = "bui-picker__title";
            public const string Grid = "bui-picker__grid";
            public const string Cell = "bui-picker__cell";
            public const string CellSelected = "bui-picker__cell--selected";
            public const string CellMuted = "bui-picker__cell--muted";
            public const string Input = "bui-picker__input";
            public const string Separator = "bui-picker__separator";
            public const string Slider = "bui-picker__slider";
            public const string Preview = "bui-picker__preview";
        }
    }

    public static class DataAttributes
    {
        // --- Component identification ---
        /// <summary>
        /// Identifies the component type (e.g., "button", "input-text")
        /// </summary>
        public const string Component = "data-bui-component";

        /// <summary>
        /// Component density: compact, standard, comfortable. Affects gap/padding.
        /// </summary>
        public const string Density = "data-bui-density";

        /// <summary>
        /// Component is disabled.
        /// </summary>
        public const string Disabled = "data-bui-disabled";

        /// <summary>
        /// Component is active.
        /// </summary>
        public const string Active = "data-bui-active";

        /// <summary>
        /// Marks component as part of dropdown family for shared styles.
        /// </summary>
        public const string DropdownBase = "data-bui-dropdown-base";

        /// <summary>
        /// Component has validation error.
        /// </summary>
        public const string Error = "data-bui-error";

        /// <summary>
        /// Input label is in floated position.
        /// </summary>
        public const string Floated = "data-bui-floated";

        /// <summary>
        /// Whether component spans full width of container.
        /// </summary>
        public const string FullWidth = "data-bui-fullwidth";

        // --- Component family attributes ---
        /// <summary>
        /// Marks component as part of input family for shared styles.
        /// </summary>
        public const string InputBase = "data-bui-input-base";

        /// <summary>
        /// Marks component as part of picker family for shared styles.
        /// </summary>
        public const string PickerBase = "data-bui-picker";

        /// <summary>
        /// Marks component as part of data collection family for shared styles.
        /// </summary>
        public const string DataCollectionBase = "data-bui-data-collection";

        // --- State attributes ---
        /// <summary>
        /// Component is in loading state.
        /// </summary>
        public const string Loading = "data-bui-loading";

        /// <summary>
        /// Component is read-only (inputs).
        /// </summary>
        public const string ReadOnly = "data-bui-readonly";

        /// <summary>
        /// Component is required (inputs).
        /// </summary>
        public const string Required = "data-bui-required";

        // --- Behavior attributes ---
        /// <summary>
        /// Whether ripple effect is enabled.
        /// </summary>
        public const string Ripple = "data-bui-ripple";

        /// <summary>
        /// Whether shadow is applied (activates shadow CSS).
        /// </summary>
        public const string Shadow = "data-bui-shadow";

        // --- Design attributes ---
        /// <summary>
        /// Component size: small, medium, large. Sets --bui-size-multiplier.
        /// </summary>
        public const string Size = "data-bui-size";

        /// <summary>
        /// Space-separated list of transition class names.
        /// </summary>
        public const string Transitions = "data-bui-transitions";

        /// <summary>
        /// Current variant of the component (e.g., "outlined", "filled")
        /// </summary>
        public const string Variant = "data-bui-variant";

        /// <summary>
        /// Placement of auxiliary buttons within a component (e.g., "left", "right").
        /// </summary>
        public const string ButtonPlacement = "data-bui-button-placement";
    }

    public static class InlineVariables
    {
        // --- Color overrides (from IHasColor/IHasBackgroundColor) ---
        public const string BackgroundColor = "--bui-inline-background";
        public const string Color = "--bui-inline-color";

        // --- Border overrides (from IHasBorder) ---
        public const string Border = "--bui-inline-border";

        public const string BorderBottom = "--bui-inline-border-bottom";
        public const string BorderLeft = "--bui-inline-border-left";
        public const string BorderRadius = "--bui-inline-border-radius";
        public const string BorderRight = "--bui-inline-border-right";
        public const string BorderTop = "--bui-inline-border-top";

        // --- Effect overrides (from IHasRipple) ---
        public const string RippleColor = "--bui-inline-ripple-color";

        public const string RippleDuration = "--bui-inline-ripple-duration";

        // --- Shadow variables (from IHasShadow) ---
        public const string Shadow = "--bui-inline-shadow";

        // --- Prefix/Suffix overrides (from IHasPrefix/IHasSuffix) ---
        public const string PrefixColor = "--bui-inline-prefix-color";
        public const string PrefixBackgroundColor = "--bui-inline-prefix-background";
        public const string SuffixColor = "--bui-inline-suffix-color";
        public const string SuffixBackgroundColor = "--bui-inline-suffix-background";
    }

    public static class Tags
    {
        /// <summary>
        /// Root custom element for all BUI components. Provides isolation and consistent styling anchor.
        /// </summary>
        public const string Component = "bui-component";
    }

    public static class Tokens
    {
        /// <summary>
        /// Opacity values for visual states. Used for: disabled states, placeholders, hover effects.
        /// </summary>
        public static class Opacity
        {
            public const string Disabled = "--bui-opacity-disabled";
            public const string DisabledValue = "0.5";
            public const string Placeholder = "--bui-opacity-placeholder";
            public const string PlaceholderValue = "0.5";
        }

        /// <summary>
        /// Z-index scale for stacking contexts. Used for: overlays, modals, tooltips, dropdowns.
        /// </summary>
        public static class ZIndex
        {
            public const string Dropdown = "--bui-z-dropdown";
            public const string DropdownValue = "1000";
            public const string Modal = "--bui-z-modal";
            public const string ModalValue = "1300";
            public const string Sticky = "--bui-z-sticky";
            public const string StickyValue = "1100";
            public const string Toast = "--bui-z-toast";
            public const string ToastValue = "1500";
            public const string Tooltip = "--bui-z-tooltip";
            public const string TooltipValue = "1400";

        }
    }

    public static class Typography
    {
        public const string FontFamily = "--bui-font-family";
        public const string FontFamilyHeading = "--bui-font-family-heading";
        public const string FontFamilyHeadingValue = "var(--bui-font-family)";
        public const string FontFamilyValue = "system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, sans-serif";
        public const string FontMono = "--bui-font-mono";
        public const string FontMonoValue = "ui-monospace, \"Cascadia Mono\", \"SF Mono\", Consolas, monospace";
        public const string FontSizeBase = "--bui-font-size-base";
        public const string FontSizeBaseValue = "1rem";
        public const string LineHeight = "--bui-line-height";
        public const string LineHeightHeading = "--bui-line-height-heading";
        public const string LineHeightHeadingValue = "1.2";
        public const string LineHeightValue = "1.5";
    }

    public static class Values
    {
        public static class Density
        {
            public const string Comfortable = "comfortable";
            public const string Compact = "compact";
            public const string Standard = "standard";
        }

        public static class Size
        {
            public const string Large = "large";
            public const string Medium = "medium";
            public const string Small = "small";
        }

        public static class Variant
        {
            public const string Filled = "filled";
            public const string Outlined = "outlined";
            public const string Standard = "standard";
        }
    }
}