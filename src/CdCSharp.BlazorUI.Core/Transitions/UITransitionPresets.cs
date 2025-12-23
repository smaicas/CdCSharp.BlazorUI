namespace CdCSharp.BlazorUI.Core.Transitions;

public static class UITransitionPresets
{
    // Single transitions
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

    // Builder for custom transitions
    public static UITransitionsBuilder Create() => new();
}
