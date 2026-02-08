namespace CdCSharp.BlazorUI.Components;

public static class BUITransitionPresets
{
    public static BUITransitions HoverScale => new BUITransitionsBuilder()
        .OnHover().Scale()
        .Build();

    public static BUITransitions HoverShadow => new BUITransitionsBuilder()
        .OnHover().BoxShadow(BUIShadowPresets.Elevation(4))
        .Build();

    public static BUITransitions HoverFade => new BUITransitionsBuilder()
        .OnHover().Opacity()
        .Build();

    public static BUITransitions HoverLift => new BUITransitionsBuilder()
        .OnHover()
            .Translate("0", "-4px")
            .BoxShadow(BUIShadowPresets.Elevation(4))
        .Build();

    public static BUITransitions HoverGlow => new BUITransitionsBuilder()
        .OnHover()
            .BoxShadow(ShadowStyle.Create(
                    y: 0,
                    blur: 20,
                    opacity: 0.5f,
                    x: 0,
                    spread: 0,
                    color: PaletteColor.Shadow,
                    inset: false
                ))
            .Scale(1.02f)
        .Build();

    public static BUITransitions CardHover => new BUITransitionsBuilder()
        .OnHover()
            .Translate("0", "-4px", t =>
            {
                t.Duration = TimeSpan.FromMilliseconds(300);
                t.Easing = e => e.CubicBezier().MaterialStandard();
            })
            .BoxShadow(BUIShadowPresets.Elevation(8))
        .Build();

    public static BUITransitions FocusRing => new BUITransitionsBuilder()
        .OnFocus().BoxShadow(BUIShadowPresets.Elevation(2))
        .Build();

    public static BUITransitions Interactive => new BUITransitionsBuilder()
        .OnHover()
            .Translate("0", "-4px")
            .BoxShadow(BUIShadowPresets.Elevation(4))
        .And()
        .OnFocus().BoxShadow(BUIShadowPresets.Elevation(2))
        .And()
        .OnActive().Scale(0.98f)
        .Build();

    public static BUITransitions MaterialButton => new BUITransitionsBuilder()
        .OnHover().BoxShadow(BUIShadowPresets.Elevation(4), t =>
        {
            t.Duration = TimeSpan.FromMilliseconds(200);
            t.Easing = e => e.CubicBezier().MaterialStandard();
        })
        .And()
        .OnActive().Scale(0.96f, t =>
        {
            t.Duration = TimeSpan.FromMilliseconds(100);
            t.Easing = e => e.CubicBezier().MaterialAccelerate();
        })
        .Build();

    public static BUITransitions PremiumButton => new BUITransitionsBuilder()
        .OnHover()
            .Scale(1.05f, t =>
            {
                t.Duration = TimeSpan.FromMilliseconds(200);
                t.Easing = e => e.CubicBezier().MaterialStandard();
            })
            .BoxShadow(
                BUIShadowPresets.Elevation(12))
        .And()
        .OnActive().Scale(0.98f, t => t.Duration = TimeSpan.FromMilliseconds(50))
        .Build();

    public static BUITransitions GlassMorphism => new BUITransitionsBuilder()
        .OnHover()
            .BackdropFilter("blur(16px)")
            .BoxShadow(BUIShadowPresets.Elevation(6))
            .Scale(1.02f)
        .Build();

    public static BUITransitions Neumorphism => new BUITransitionsBuilder()
        .OnHover().BoxShadow(
            ShadowStyle.Create(8, 16, 0.1f, x: 8)
                .Add(-8, 16, 0.7f, x: -8, color: "#ffffff"))
        .Build();

    public static BUITransitionsBuilder Create() => new();
}