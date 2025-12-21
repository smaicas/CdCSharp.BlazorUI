using System.Text;

namespace CdCSharp.BlazorUI.Core.Theming.Themes;

public static class EffectsCssGenerator
{
    public static string GetCss()
    {
        StringBuilder sb = new();

        // Base CSS
        sb.AppendLine(@"
/* =========================================================
   UI Effects System
   ========================================================= */

/* Base transition setup */
.ui-has-effects {
    transition-property: transform, opacity, filter, box-shadow, background-color, border-color;
    transition-duration: var(--effect-duration, 300ms);
    transition-timing-function: var(--effect-easing, ease-in-out);
    transition-delay: var(--effect-delay, 0ms);
}

/* Effect composition */
.ui-has-effects[data-effect-id] {
    will-change: transform, opacity;
}
");

        // Add keyframes for all keyframe effects
        // This would scan assemblies for IEffect implementations
        // For now, manually add the keyframes

        sb.AppendLine(@"
/* Keyframe Effects */
@keyframes ui-effect-pulse {
    0%, 100% { transform: scale(1); }
    50% { transform: scale(1.05); }
}

@keyframes ui-effect-shake {
    0%, 100% { transform: translateX(0); }
    10%, 30%, 50%, 70%, 90% { transform: translateX(-2px); }
    20%, 40%, 60%, 80% { transform: translateX(2px); }
}

@keyframes ui-effect-bounce {
    0%, 20%, 50%, 80%, 100% { transform: translateY(0); }
    40% { transform: translateY(-30px); }
    60% { transform: translateY(-15px); }
}

@keyframes ui-effect-spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
}

@keyframes ui-effect-fade-in {
    from { opacity: 0; }
    to { opacity: 1; }
}

@keyframes ui-effect-fade-out {
    from { opacity: 1; }
    to { opacity: 0; }
}

@keyframes ui-effect-slide-in-left {
    from { transform: translateX(-100%); }
    to { transform: translateX(0); }
}

@keyframes ui-effect-slide-in-right {
    from { transform: translateX(100%); }
    to { transform: translateX(0); }
}

@keyframes ui-effect-slide-in-up {
    from { transform: translateY(100%); }
    to { transform: translateY(0); }
}

@keyframes ui-effect-slide-in-down {
    from { transform: translateY(-100%); }
    to { transform: translateY(0); }
}
");

        return sb.ToString();
    }
}
