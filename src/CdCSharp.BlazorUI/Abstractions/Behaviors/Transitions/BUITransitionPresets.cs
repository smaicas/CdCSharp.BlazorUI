using CdCSharp.BlazorUI.Abstractions.Behaviors.Transitions;

namespace CdCSharp.BlazorUI.Components;

public static class BUITransitionPresets
{
    // Focus states
    public static BUITransitions AccessibleFocus => new BUITransitionsBuilder()
        .OnFocus().Shadow("0 0 0 3px rgba(59, 130, 246, 0.5)", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(150);
        })
        .And()
        .OnFocus().Scale(1.02f)
        .Build();

    // Bounce effects
    public static BUITransitions BounceIn => new BUITransitionsBuilder()
        .OnHover().Scale(1.1f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(600);
            options.Easing = easing => easing.CubicBezier().Bounce();
        })
        .Build();

    // Card effects
    public static BUITransitions CardHover => new BUITransitionsBuilder()
        .OnHover().Lift(options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(300);
            options.Easing = easing => easing.CubicBezier().MaterialStandard();
        })
        .And()
        .OnHover().Shadow("0 10px 30px rgba(0,0,0,0.2)")
        .Build();

    // Gradient shift
    public static BUITransitions ColorShift => new BUITransitionsBuilder()
        .OnHover().Shadow("0 0 20px rgba(139, 92, 246, 0.5)", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(400);
        })
        .Build();

    // Disabled states
    public static BUITransitions DisabledState => new BUITransitionsBuilder()
        .OnDisabled().Fade(0.5f)
        .And()
        .OnDisabled().Blur("1px")
        .Build();

    public static BUITransitions ElasticScale => new BUITransitionsBuilder()
            .OnHover().Scale(1.05f, options =>
            {
                options.Duration = TimeSpan.FromMilliseconds(800);
                options.Easing = easing => easing.CubicBezier().Elastic();
            })
            .Build();

    public static BUITransitions FocusRing => new BUITransitionsBuilder()
            .OnFocus().Shadow("0 0 0 3px rgba(59, 130, 246, 0.3)")
            .Build();

    public static BUITransitions GlassMorphism => new BUITransitionsBuilder()
            .OnHover()
                .BackdropBlur("16px")
                .And()
            .OnHover()
                .Shadow("0 8px 32px rgba(0, 0, 0, 0.1)")
                .And()
            .OnHover()
                .Scale(1.02f)
            .Build();

    public static BUITransitions HoverFade => new BUITransitionsBuilder()
            .OnHover().Fade()
            .Build();

    public static BUITransitions HoverGlow => new BUITransitionsBuilder()
            .OnHover().Glow()
            .Build();

    public static BUITransitions HoverLift => new BUITransitionsBuilder()
            .OnHover().Lift()
            .Build();

    // Basic single transitions
    public static BUITransitions HoverScale => new BUITransitionsBuilder()
        .OnHover().Scale()
        .Build();

    public static BUITransitions HoverShadow => new BUITransitionsBuilder()
        .OnHover().Shadow()
        .Build();

    // Combined transitions
    public static BUITransitions Interactive => new BUITransitionsBuilder()
        .OnHover().Lift()
        .And()
        .OnFocus().Shadow("0 0 0 3px rgba(59, 130, 246, 0.3)")
        .And()
        .OnActive().Scale(0.98f)
        .Build();

    // Material Design
    public static BUITransitions MaterialButton => new BUITransitionsBuilder()
        .OnHover().Shadow("0 2px 4px rgba(0,0,0,0.2)", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(200);
            options.Easing = easing => easing.CubicBezier().MaterialStandard();
        })
        .And()
        .OnActive().Scale(0.96f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(100);
            options.Easing = easing => easing.CubicBezier().MaterialAccelerate();
        })
        .Build();

    public static BUITransitions ModernGlass => new BUITransitionsBuilder()
            .OnHover()
            .BackdropBlur("12px")
            .And()
        .OnHover()
            .Shadow("0 8px 32px rgba(0, 0, 0, 0.1)")
        .Build();

    // Modern effects
    public static BUITransitions Neumorphism => new BUITransitionsBuilder()
        .OnHover().Shadow("8px 8px 16px rgba(0,0,0,0.1), -8px -8px 16px rgba(255,255,255,0.7)")
        .Build();

    // 3D effects
    public static BUITransitions Perspective => new BUITransitionsBuilder()
        .OnHover().Rotate("5deg", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(300);
            options.Easing = easing => easing.CubicBezier().BackOut();
        })
        .And()
        .OnHover().Scale(1.05f)
        .Build();

    // Complex combinations
    public static BUITransitions PremiumButton => new BUITransitionsBuilder()
        .OnHover().Scale(1.05f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(200);
            options.Easing = easing => easing.CubicBezier().MaterialStandard();
        })
        .And()
        .OnHover().Shadow("0 5px 20px rgba(0,0,0,0.3)")
        .And()
        .OnHover().Glow("rgba(255, 215, 0, 0.5)")
        .And()
        .OnActive().Scale(0.98f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(50);
        })
        .Build();

    // Loading states
    public static BUITransitions Pulse => new BUITransitionsBuilder()
        .OnHover().Scale(1.0f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(1500);
            options.Easing = easing => easing.EaseInOut();
        })
        .Build();

    public static BUITransitions Shake => new BUITransitionsBuilder()
        .OnHover().Translate("-2px", "0", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(100);
            options.Easing = easing => easing.Linear();
        })
        .Build();

    public static BUITransitions Skeleton => new BUITransitionsBuilder()
            .OnHover().Fade(0.6f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(1000);
            options.Easing = easing => easing.EaseInOut();
        })
        .Build();

    // Micro interactions
    public static BUITransitions Wiggle => new BUITransitionsBuilder()
        .OnHover().Rotate("3deg", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(150);
            options.Easing = easing => easing.EaseInOut();
        })
        .Build();

    // Builder for custom transitions
    public static BUITransitionsBuilder Create() => new();
}