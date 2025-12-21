using CdCSharp.BlazorUI.Core.Effects.Builders;

namespace CdCSharp.BlazorUI.Core.Effects;

public static class UIEffectPresets
{
    // Hover effects
    public static SimpleUIEffects HoverScale => SimpleEffectBuilder.Create()
        .OnHover(e =>
        {
            e.Properties["transform"] = "scale(1.05)";
        })
        .Build();

    public static SimpleUIEffects HoverFade => SimpleEffectBuilder.Create()
        .OnHover(e =>
        {
            e.Properties["opacity"] = "0.7";
        })
        .Build();

    public static SimpleUIEffects HoverShadow => SimpleEffectBuilder.Create()
        .OnHover(e =>
        {
            e.Properties["box-shadow"] = "0 4px 8px rgba(0, 0, 0, 0.2)";
        })
        .Build();

    public static SimpleUIEffects HoverLift => SimpleEffectBuilder.Create()
        .OnHover(e =>
        {
            e.Properties["transform"] = "translateY(-4px)";
            e.Properties["box-shadow"] = "0 8px 16px rgba(0, 0, 0, 0.2)";
        })
        .Build();

    public static SimpleUIEffects HoverGlow => SimpleEffectBuilder.Create()
        .OnHover(e =>
        {
            e.Properties["transform"] = "scale(1.02)";
            e.Properties["box-shadow"] = "0 0 20px rgba(59, 130, 246, 0.5)";
            e.Duration = TimeSpan.FromMilliseconds(200);
        })
        .Build();

    // Click effects
    public static SimpleUIEffects ClickPulse => SimpleEffectBuilder.Create()
        .OnActive(e =>
        {
            e.AnimationName = "ui-effect-pulse";
            e.Duration = TimeSpan.FromMilliseconds(600);
        })
        .Build();

    public static SimpleUIEffects ClickShake => SimpleEffectBuilder.Create()
        .OnActive(e =>
        {
            e.AnimationName = "ui-effect-shake";
            e.Duration = TimeSpan.FromMilliseconds(500);
        })
        .Build();

    // Focus effects
    public static SimpleUIEffects FocusGlow => SimpleEffectBuilder.Create()
        .OnFocus(e =>
        {
            e.Properties["box-shadow"] = "0 0 0 3px rgba(59, 130, 246, 0.3)";
        })
        .Build();

    // Combined effects
    public static SimpleUIEffects Interactive => SimpleEffectBuilder.Create()
        .OnHover(e =>
        {
            e.Properties["transform"] = "scale(1.05)";
            e.Properties["box-shadow"] = "0 4px 8px rgba(0, 0, 0, 0.2)";
        })
        .OnActive(e =>
        {
            e.Properties["transform"] = "scale(0.98)";
        })
        .OnFocus(e =>
        {
            e.Properties["box-shadow"] = "0 0 0 3px rgba(59, 130, 246, 0.3)";
        })
        .Build();

    // Factory method for custom effects
    public static SimpleEffectBuilder Create() => SimpleEffectBuilder.Create();
}

//public enum EffectTrigger
//{
//    Hover,
//    HoverEnd,
//    Click,
//    DoubleClick,
//    MouseDown,
//    MouseUp,
//    Focus,
//    Blur,
//    Active,
//    Disabled,
//    Enabled,
//    Appear,
//    Disappear,
//    TouchStart,
//    TouchEnd,
//    Custom
//}