using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Core.Transitions;

namespace CdCSharp.BlazorUI.Core.Theming.Css;

public class CssClassesReference
{
    public const string InputRoot = "ui-input-root";

    //UITextButton
    public const string UIButton = "ui-button";

    //UISvgIcon
    public const string UISvgIcon = "ui-svg-icon";

    public const string UISvgIcon_SizeLarge = "ui-svg-icon-l";
    public const string UISvgIcon_SizeMedium = "ui-svg-icon-m";
    public const string UISvgIcon_SizeSmall = "ui-svg-icon-s";
    public const string UISvgIcon_SizeXLarge = "ui-svg-icon-xl";
    public const string UISvgIcon_SizeXXLarge = "ui-svg-icon-xxl";

    // UIThemeSwitch
    public const string UIThemeSwitch = "ui-theme-switch";

    public static string HasTransitions => "ui-has-transitions";

    public static string Specific(string componentBaseClass, string specific)
        => $"{componentBaseClass.ToLowerInvariant()}--{specific}";

    public static string Transition(TransitionTrigger trigger, TransitionType type)
        => $"ui-transition-{trigger.ToString().ToLower()}-{type.ToString().ToLower()}";

    public static string Variant(string componentBaseClass, Variant variant)
                    => $"{componentBaseClass.ToLowerInvariant()}--{variant.ToString().ToLowerInvariant()}";
}