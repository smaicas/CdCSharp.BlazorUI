namespace CdCSharp.BlazorUI.Core.Theming.Effects;

public static class UIEffects
{
    // Transiciones comunes
    public static class Transitions
    {
        public static CssTransition FadeIn => CssTransition.For("opacity")
            .DurationMs(300)
            .Easing(CssEasing.EaseOut);

        //public static CssTransition SlideUp => new CssEffectCollection
        //{
        //    CssTransition.For("transform").DurationMs(300).Easing(CssEasing.EaseOut),
        //    CssTransition.For("opacity").DurationMs(300).Easing(CssEasing.EaseOut)
        //};

        public static CssTransition SlideUp => CssTransition.For("transform").DurationMs(300).Easing(CssEasing.EaseOut);

        public static CssTransition Hover => CssTransition.For("all")
            .DurationMs(200)
            .Easing(CssEasing.EaseInOut);
    }

    // Animaciones
    public static class Animations
    {
        public static CssAnimation Pulse => new()
        {
            KeyframeName = "pulse",
            Duration = TimeSpan.FromSeconds(2),
            IterationCount = -1,
            Easing = CssEasing.EaseInOut
        };

        public static CssAnimation Spin => new()
        {
            KeyframeName = "spin",
            Duration = TimeSpan.FromSeconds(1),
            IterationCount = -1,
            Easing = CssEasing.Linear
        };
    }

    // Transforms
    public static CssTransform ScaleOnHover => new CssTransform().Scale(1.05);
}
