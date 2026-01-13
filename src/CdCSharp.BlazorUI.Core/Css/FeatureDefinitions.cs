namespace CdCSharp.BlazorUI.Core.Css;

/// <summary>
/// Central definition of all CSS-related constants used by the BUI component library.
/// Organized by purpose and usage context.
/// </summary>
public static class FeatureDefinitions
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 1: HTML STRUCTURE
    // Custom HTML elements used by the library.
    // Used in: All component .razor files as the root wrapper element.
    // ═══════════════════════════════════════════════════════════════════════════

    public static class Tags
    {
        /// <summary>
        /// Root custom element for all BUI components.
        /// Provides isolation and consistent styling anchor.
        /// </summary>
        public const string Component = "bui-component";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 2: DATA ATTRIBUTES
    // Used for CSS targeting via [data-*] selectors and component state management.
    // Set by: BUIComponentAttributesBuilder based on IHas* interfaces.
    // Used in: Global CSS generators and component-specific CSS files.
    // ═══════════════════════════════════════════════════════════════════════════

    public static class DataAttributes
    {
        // --- Component identification ---
        /// <summary>Identifies the component type (e.g., "button", "input-text")</summary>
        public const string Component = "data-bui-component";

        /// <summary>Current variant of the component (e.g., "outlined", "filled")</summary>
        public const string Variant = "data-bui-variant";

        // --- Design attributes ---
        /// <summary>Component size: small, medium, large. Sets --bui-size-multiplier.</summary>
        public const string Size = "data-bui-size";

        /// <summary>Component density: compact, standard, comfortable. Affects gap/padding.</summary>
        public const string Density = "data-bui-density";

        /// <summary>Whether component spans full width of container.</summary>
        public const string FullWidth = "data-bui-fullwidth";

        /// <summary>Elevation level (0-24). Generates box-shadow.</summary>
        public const string Elevation = "data-bui-elevation";

        /// <summary>Space-separated list of transition class names.</summary>
        public const string Transitions = "data-bui-transitions";

        // --- State attributes ---
        /// <summary>Component is in loading state.</summary>
        public const string Loading = "data-bui-loading";

        /// <summary>Component is disabled.</summary>
        public const string Disabled = "data-bui-disabled";

        /// <summary>Component is read-only (inputs).</summary>
        public const string ReadOnly = "data-bui-readonly";

        /// <summary>Component is required (inputs).</summary>
        public const string Required = "data-bui-required";

        /// <summary>Component has validation error.</summary>
        public const string Error = "data-bui-error";

        /// <summary>Input label is in floated position.</summary>
        public const string Floated = "data-bui-floated";

        // --- Behavior attributes ---
        /// <summary>Whether ripple effect is enabled.</summary>
        public const string Ripple = "data-bui-ripple";

        // --- Component family attributes ---
        /// <summary>Marks component as part of input family for shared styles.</summary>
        public const string InputBase = "data-bui-input-base";

        /// <summary>Marks component as part of dropdown family for shared styles.</summary>
        public const string DropdownBase = "data-bui-dropdown-base";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 3: DESIGN TOKENS
    // Global design system values defined in :root via DesignTokensGenerator.
    // These are foundational values used across the entire application.
    // ═══════════════════════════════════════════════════════════════════════════

    public static class Tokens
    {
        /// <summary>
        /// Z-index scale for stacking contexts.
        /// Used for: overlays, modals, tooltips, dropdowns.
        /// </summary>
        public static class ZIndex
        {
            public const string Dropdown = "--bui-z-dropdown";
            public const string Sticky = "--bui-z-sticky";
            public const string Modal = "--bui-z-modal";
            public const string Tooltip = "--bui-z-tooltip";
            public const string Toast = "--bui-z-toast";

            public const string DropdownValue = "1000";
            public const string StickyValue = "1100";
            public const string ModalValue = "1300";
            public const string TooltipValue = "1400";
            public const string ToastValue = "1500";
        }

        /// <summary>
        /// Opacity values for visual states.
        /// Used for: disabled states, placeholders, hover effects.
        /// </summary>
        public static class Opacity
        {
            public const string Disabled = "--bui-opacity-disabled";
            public const string Placeholder = "--bui-opacity-placeholder";
            public const string Hover = "--bui-opacity-hover";

            public const string DisabledValue = "0.5";
            public const string PlaceholderValue = "0.5";
            public const string HoverValue = "0.8";
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 4: TYPOGRAPHY TOKENS
    // Defined in :root via TypographyGenerator.
    // Font sizes are responsive via media queries.
    // ═══════════════════════════════════════════════════════════════════════════

    public static class Typography
    {
        public const string FontFamily = "--bui-font-family";
        public const string FontFamilyHeading = "--bui-font-family-heading";
        public const string FontMono = "--bui-font-mono";
        public const string FontSizeBase = "--bui-font-size-base";
        public const string LineHeight = "--bui-line-height";
        public const string LineHeightHeading = "--bui-line-height-heading";

        public const string FontFamilyValue = "system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, sans-serif";
        public const string FontFamilyHeadingValue = "var(--bui-font-family)";
        public const string FontMonoValue = "ui-monospace, \"Cascadia Mono\", \"SF Mono\", Consolas, monospace";
        public const string FontSizeBaseValue = "1rem";
        public const string LineHeightValue = "1.5";
        public const string LineHeightHeadingValue = "1.2";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 5: COMPONENT VARIABLES
    // CSS custom properties set on bui-component elements based on data attributes.
    // Set by: BaseComponentGenerator and family-specific generators.
    // ═══════════════════════════════════════════════════════════════════════════

    public static class ComponentVariables
    {
        /// <summary>
        /// Size-related variables set by [data-bui-size].
        /// Only the multiplier is set globally; components use it in their isolated CSS.
        /// </summary>
        public static class Size
        {
            /// <summary>
            /// Scale factor for size calculations.
            /// Small=0.85, Medium=1, Large=1.25
            /// Components multiply their dimensions by this value.
            /// </summary>
            public const string Multiplier = "--bui-size-multiplier";
        }

        /// <summary>
        /// Density-related variables set by [data-bui-density].
        /// Affects spacing between elements via gap.
        /// </summary>
        public static class Density
        {
            public const string Gap = "--bui-density-gap";
        }

        /// <summary>
        /// Input family specific variables.
        /// Used by: InputFamilyGenerator for shared input styles.
        /// </summary>
        public static class Input
        {
            public const string Height = "--input-height";
            public const string PaddingX = "--input-padding-x";
            public const string PaddingY = "--input-padding-y";
            public const string BorderColor = "--input-border-color";
            public const string BorderWidth = "--input-border-width";
            public const string BorderRadius = "--input-border-radius";
            public const string Background = "--input-bg";
            public const string Transition = "--input-transition";
            public const string LabelColor = "--input-label-color";
            public const string LabelFocusColor = "--input-label-focus-color";
            public const string LabelErrorColor = "--input-label-error-color";
            public const string LabelScale = "--input-label-scale";
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 6: INLINE VARIABLES
    // CSS custom properties set via style="" attribute for per-instance customization.
    // Set by: BUIComponentAttributesBuilder when component has specific properties.
    // ═══════════════════════════════════════════════════════════════════════════

    public static class InlineVariables
    {
        // --- Color overrides ---
        public const string BackgroundColor = "--bui-inline-bg";
        public const string Color = "--bui-inline-color";

        // --- Border overrides (from IHasBorder) ---
        public const string Border = "--bui-inline-border";
        public const string BorderRadius = "--bui-inline-border-radius";
        public const string BorderTop = "--bui-inline-border-top";
        public const string BorderRight = "--bui-inline-border-right";
        public const string BorderBottom = "--bui-inline-border-bottom";
        public const string BorderLeft = "--bui-inline-border-left";

        // --- Effect overrides ---
        public const string ElevationShadowColor = "--bui-inline-elevation-shadow-color";
        public const string RippleColor = "--bui-inline-ripple-color";
        public const string RippleDuration = "--bui-inline-ripple-duration";
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 7: CSS CLASSES
    // Utility and component-specific class names.
    // Used in: Component templates and global utilities.
    // ═══════════════════════════════════════════════════════════════════════════

    public static class CssClasses
    {
        /// <summary>Visually hidden but accessible to screen readers.</summary>
        public const string VisuallyHidden = "bui-visually-hidden";

        /// <summary>Ripple effect element class.</summary>
        public const string Ripple = "bui-ripple";

        /// <summary>Input family component class names.</summary>
        public static class Input
        {
            public const string Wrapper = "bui-input__wrapper";
            public const string Field = "bui-input__field";
            public const string Label = "bui-input__label";
            public const string Required = "bui-input__required";
            public const string Fieldset = "bui-input__fieldset";
            public const string Legend = "bui-input__legend";
            public const string HelperText = "bui-input__helper-text";
            public const string Validation = "bui-input__validation";
            public const string Loading = "bui-input__loading";
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // SECTION 8: ATTRIBUTE VALUES
    // Valid string values for data attributes.
    // Used in: Generators and components for consistency.
    // ═══════════════════════════════════════════════════════════════════════════

    public static class Values
    {
        public static class Size
        {
            public const string Small = "small";
            public const string Medium = "medium";
            public const string Large = "large";
        }

        public static class Density
        {
            public const string Compact = "compact";
            public const string Standard = "standard";
            public const string Comfortable = "comfortable";
        }

        public static class Variant
        {
            public const string Outlined = "outlined";
            public const string Filled = "filled";
            public const string Standard = "standard";
        }
    }
}