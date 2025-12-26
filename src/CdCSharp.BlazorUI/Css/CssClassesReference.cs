using CdCSharp.BlazorUI.Components.Abstractions;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Transitions;

namespace CdCSharp.BlazorUI.Css;

public class CssClassesReference
{
    public const string InputRoot = "ui-input-root";

    //UITextButton
    public const string UIButton = "ui-button";
    public const string UIButton__Text = "ui-button__text";
    //UISvgIcon
    public const string UISvgIcon = "ui-svg-icon";

    public const string UISvgIcon_SizeLarge = "ui-svg-icon-l";
    public const string UISvgIcon_SizeMedium = "ui-svg-icon-m";
    public const string UISvgIcon_SizeSmall = "ui-svg-icon-s";
    public const string UISvgIcon_SizeXLarge = "ui-svg-icon-xl";
    public const string UISvgIcon_SizeXXLarge = "ui-svg-icon-xxl";

    // UIThemeSwitch
    public const string UIThemeSwitch = "ui-theme-switch";

    // UILoadingIndicator
    public const string UILoadingIndicator = "ui-loading-indicator";

    // Transitions
    public static string HasTransitions => "ui-has-transitions";

    // General size classes
    public const string SizeSmall = "ui-size-small";
    public const string SizeMedium = "ui-size-medium";
    public const string SizeLarge = "ui-size-large";

    // Density classes
    public const string DensityComfortable = "ui-density-comfortable";
    public const string DensityStandard = "ui-density-standard";
    public const string DensityCompact = "ui-density-compact";

    // State classes
    public const string FullWidth = "ui-full-width";
    public const string Loading = "ui-loading";
    public const string HasRipple = "ui-has-ripple";
    public const string Ripple = "ui-ripple";

    // Elevation helper
    public static string Elevation(int level) => $"ui-elevation-{level}";

    // Size helper
    public static string Size(SizeEnum size)
    {
        return size switch
        {
            SizeEnum.Small => SizeSmall,
            SizeEnum.Medium => SizeMedium,
            SizeEnum.Large => SizeLarge,
            _ => SizeMedium
        };
    }

    // Density helper
    public static string Density(DensityEnum density)
    {
        return density switch
        {
            DensityEnum.Comfortable => DensityComfortable,
            DensityEnum.Standard => DensityStandard,
            DensityEnum.Compact => DensityCompact,
            _ => DensityStandard
        };
    }

    public static string Specific(string componentBaseClass, string specific)
        => $"{componentBaseClass.ToLowerInvariant()}--{specific}";

    public static string Transition(TransitionTrigger trigger, TransitionType type)
        => $"ui-transition-{trigger.ToString().ToLower()}-{type.ToString().ToLower()}";

    public static string Variant(string componentBaseClass, Variant variant)
                    => $"{componentBaseClass.ToLowerInvariant()}--{variant.ToString().ToLowerInvariant()}";
}