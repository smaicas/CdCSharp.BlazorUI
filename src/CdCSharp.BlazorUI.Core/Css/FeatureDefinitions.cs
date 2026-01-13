namespace CdCSharp.BlazorUI.Core.Css;

public static class FeatureDefinitions
{
    public static class Tags
    {
        public const string Component = "bui-component";
    }

    public static class DataAttributes
    {
        public const string Component = "data-bui-component";
        public const string Variant = "data-bui-variant";

        public const string Size = "data-bui-size";
        public const string Density = "data-bui-density";
        public const string FullWidth = "data-bui-fullwidth";
        public const string Elevation = "data-bui-elevation";
        public const string Transitions = "data-bui-transitions";

        public const string Loading = "data-bui-loading";
        public const string Disabled = "data-bui-disabled";
        public const string ReadOnly = "data-bui-readonly";
        public const string Required = "data-bui-required";
        public const string Error = "data-bui-error";
        public const string Floated = "data-bui-floated";

        public const string Ripple = "data-bui-ripple";

        public const string InputBase = "data-bui-input-base";
        public const string DropdownBase = "data-bui-dropdown-base";
    }

    public static class Tokens
    {
        public static class Spacing
        {
            public const string Space1 = "--bui-space-1";
            public const string Space2 = "--bui-space-2";
            public const string Space3 = "--bui-space-3";
            public const string Space4 = "--bui-space-4";
            public const string Space5 = "--bui-space-5";
            public const string Space6 = "--bui-space-6";

            public const string Space1Value = "0.25rem";
            public const string Space2Value = "0.5rem";
            public const string Space3Value = "0.75rem";
            public const string Space4Value = "1rem";
            public const string Space5Value = "1.5rem";
            public const string Space6Value = "2rem";
        }

        public static class Typography
        {
            public const string FontFamily = "--bui-font-family";
            public const string FontMono = "--bui-font-mono";
            public const string FontSizeSm = "--bui-font-size-sm";
            public const string FontSizeMd = "--bui-font-size-md";
            public const string FontSizeLg = "--bui-font-size-lg";
            public const string LineHeight = "--bui-line-height";

            public const string FontFamilyValue = "system-ui, -apple-system, BlinkMacSystemFont, \"Segoe UI\", Roboto, sans-serif";
            public const string FontMonoValue = "ui-monospace, \"Cascadia Mono\", \"SF Mono\", Consolas, monospace";
            public const string FontSizeSmValue = "0.875rem";
            public const string FontSizeMdValue = "1rem";
            public const string FontSizeLgValue = "1.125rem";
            public const string LineHeightValue = "1.5";
        }

        public static class Radius
        {
            public const string Sm = "--bui-radius-sm";
            public const string Md = "--bui-radius-md";
            public const string Lg = "--bui-radius-lg";
            public const string Full = "--bui-radius-full";

            public const string SmValue = "4px";
            public const string MdValue = "8px";
            public const string LgValue = "12px";
            public const string FullValue = "9999px";
        }

        public static class Transition
        {
            public const string Fast = "--bui-transition-fast";
            public const string Normal = "--bui-transition-normal";
            public const string Slow = "--bui-transition-slow";

            public const string FastValue = "150ms ease";
            public const string NormalValue = "200ms ease";
            public const string SlowValue = "300ms ease";
        }

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

        public static class Shadow
        {
            public const string Sm = "--bui-shadow-sm";
            public const string Md = "--bui-shadow-md";
            public const string Lg = "--bui-shadow-lg";

            public const string SmValue = "0 1px 2px var(--palette-shadow)";
            public const string MdValue = "0 4px 8px var(--palette-shadow)";
            public const string LgValue = "0 8px 24px var(--palette-shadow)";
        }

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

    public static class InlineVariables
    {
        public const string BackgroundColor = "--bui-inline-bg";
        public const string Color = "--bui-inline-color";

        // =====================================
        // Border system (shorthand)
        // =====================================

        public const string Border = "--bui-inline-border";
        public const string BorderRadius = "--bui-inline-border-radius";

        public const string BorderTop = "--bui-inline-border-top";
        public const string BorderRight = "--bui-inline-border-right";
        public const string BorderBottom = "--bui-inline-border-bottom";
        public const string BorderLeft = "--bui-inline-border-left";

        public const string ElevationShadowColor = "--bui-inline-elevation-shadow-color";
        public const string RippleColor = "--bui-inline-ripple-color";
        public const string RippleDuration = "--bui-inline-ripple-duration";
    }

    public static class SizeVariables
    {
        public const string Font = "--bui-size-font";
        public const string Icon = "--bui-size-icon";
        public const string Height = "--bui-size-height";
        public const string PaddingX = "--bui-size-padding-x";
        public const string PaddingY = "--bui-size-padding-y";
    }

    public static class DensityVariables
    {
        public const string Gap = "--bui-density-gap";
        public const string Padding = "--bui-density-padding";
    }

    public static class InputVariables
    {
        public const string Height = "--input-height";
        public const string PaddingX = "--input-padding-x";
        public const string PaddingY = "--input-padding-y";
        public const string Radius = "--input-radius";
        public const string BorderColor = "--input-border-color";
        public const string Background = "--input-bg";
        public const string Transition = "--input-transition";
    }

    public static class CssClasses
    {
        public const string VisuallyHidden = "bui-visually-hidden";
        public const string Ripple = "bui-ripple";

        public static class Input
        {
            public const string Label = "bui-input__label";
            public const string Required = "bui-input__required";
            public const string Wrapper = "bui-input__wrapper";
            public const string Field = "bui-input__field";
            public const string Helper = "bui-input__helper";
            public const string Validation = "bui-input__validation";
        }
    }

    public static class SizeValues
    {
        public const string Small = "small";
        public const string Medium = "medium";
        public const string Large = "large";
    }

    public static class DensityValues
    {
        public const string Compact = "compact";
        public const string Standard = "standard";
        public const string Comfortable = "comfortable";
    }

    public static class VariantValues
    {
        public const string Outlined = "outlined";
        public const string Filled = "filled";
        public const string Standard = "standard";
    }
}