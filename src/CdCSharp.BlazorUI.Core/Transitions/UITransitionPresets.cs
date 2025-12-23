namespace CdCSharp.BlazorUI.Core.Transitions;

public static class UITransitionPresets
{
    // Basic single transitions
    public static UITransitions HoverScale => new UITransitionsBuilder()
        .OnHover().Scale()
        .Build();

    public static UITransitions HoverFade => new UITransitionsBuilder()
        .OnHover().Fade()
        .Build();

    public static UITransitions HoverShadow => new UITransitionsBuilder()
        .OnHover().Shadow()
        .Build();

    public static UITransitions HoverLift => new UITransitionsBuilder()
        .OnHover().Lift()
        .Build();

    public static UITransitions HoverGlow => new UITransitionsBuilder()
        .OnHover().Glow()
        .Build();

    public static UITransitions FocusRing => new UITransitionsBuilder()
        .OnFocus().Shadow("0 0 0 3px rgba(59, 130, 246, 0.3)")
        .Build();

    // Combined transitions
    public static UITransitions Interactive => new UITransitionsBuilder()
        .OnHover().Lift()
        .And()
        .OnFocus().Shadow("0 0 0 3px rgba(59, 130, 246, 0.3)")
        .And()
        .OnActive().Scale(0.98f)
        .Build();

    public static UITransitions ModernGlass => new UITransitionsBuilder()
        .OnHover()
            .BackdropBlur("12px")
            .And()
        .OnHover()
            .Shadow("0 8px 32px rgba(0, 0, 0, 0.1)")
        .Build();

    // Material Design
    public static UITransitions MaterialButton => new UITransitionsBuilder()
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

    // Loading states
    public static UITransitions Pulse => new UITransitionsBuilder()
        .OnHover().Scale(1.0f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(1500);
            options.Easing = easing => easing.EaseInOut();
        })
        .Build();

    public static UITransitions Skeleton => new UITransitionsBuilder()
        .OnHover().Fade(0.6f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(1000);
            options.Easing = easing => easing.EaseInOut();
        })
        .Build();

    // Bounce effects
    public static UITransitions BounceIn => new UITransitionsBuilder()
        .OnHover().Scale(1.1f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(600);
            options.Easing = easing => easing.CubicBezier().Bounce();
        })
        .Build();

    public static UITransitions ElasticScale => new UITransitionsBuilder()
        .OnHover().Scale(1.05f, options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(800);
            options.Easing = easing => easing.CubicBezier().Elastic();
        })
        .Build();

    // Micro interactions
    public static UITransitions Wiggle => new UITransitionsBuilder()
        .OnHover().Rotate("3deg", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(150);
            options.Easing = easing => easing.EaseInOut();
        })
        .Build();

    public static UITransitions Shake => new UITransitionsBuilder()
        .OnHover().Translate("-2px", "0", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(100);
            options.Easing = easing => easing.Linear();
        })
        .Build();

    // Modern effects
    public static UITransitions Neumorphism => new UITransitionsBuilder()
        .OnHover().Shadow("8px 8px 16px rgba(0,0,0,0.1), -8px -8px 16px rgba(255,255,255,0.7)")
        .Build();

    public static UITransitions GlassMorphism => new UITransitionsBuilder()
        .OnHover()
            .BackdropBlur("16px")
            .And()
        .OnHover()
            .Shadow("0 8px 32px rgba(0, 0, 0, 0.1)")
            .And()
        .OnHover()
            .Scale(1.02f)
        .Build();

    // Card effects
    public static UITransitions CardHover => new UITransitionsBuilder()
        .OnHover().Lift(options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(300);
            options.Easing = easing => easing.CubicBezier().MaterialStandard();
        })
        .And()
        .OnHover().Shadow("0 10px 30px rgba(0,0,0,0.2)")
        .Build();

    // Gradient shift
    public static UITransitions ColorShift => new UITransitionsBuilder()
        .OnHover().Shadow("0 0 20px rgba(139, 92, 246, 0.5)", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(400);
        })
        .Build();

    // 3D effects
    public static UITransitions Perspective => new UITransitionsBuilder()
        .OnHover().Rotate("5deg", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(300);
            options.Easing = easing => easing.CubicBezier().BackOut();
        })
        .And()
        .OnHover().Scale(1.05f)
        .Build();

    // Focus states
    public static UITransitions AccessibleFocus => new UITransitionsBuilder()
        .OnFocus().Shadow("0 0 0 3px rgba(59, 130, 246, 0.5)", options =>
        {
            options.Duration = TimeSpan.FromMilliseconds(150);
        })
        .And()
        .OnFocus().Scale(1.02f)
        .Build();

    // Disabled states
    public static UITransitions DisabledState => new UITransitionsBuilder()
        .OnDisabled().Fade(0.5f)
        .And()
        .OnDisabled().Blur("1px")
        .Build();

    // Complex combinations
    public static UITransitions PremiumButton => new UITransitionsBuilder()
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

    // Builder for custom transitions
    public static UITransitionsBuilder Create() => new();
}