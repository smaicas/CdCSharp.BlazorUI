using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Gens;

[ExcludeFromCodeCoverage]
public static class CssTransitionClasses
{
    public static string GetCss() => $@"
/* ========================================
   Transition Classes
   Auto-generated - Do not edit manually
   ======================================== */

/* Base transition setup */
.ui-has-transitions {{
    transition: all var(--ui-transition-duration, 200ms)
                var(--ui-transition-easing, ease-in-out)
                var(--ui-transition-delay, 0ms);
}}

{HoverTransitions()}
{FocusTransitions()}
{ActiveTransitions()}
{DisabledTransitions()}
{ModernFeatures()}
{AccessibilityFeatures()}
";

    private static string HoverTransitions() => @"
/* Hover transitions */
.ui-transition-hover-scale:hover {
    transform: scale(var(--ui-transition-hover-scale, 1.05));
}

.ui-transition-hover-rotate:hover {
    transform: rotate(var(--ui-transition-hover-rotate, 5deg));
}

.ui-transition-hover-fade:hover {
    opacity: var(--ui-transition-hover-opacity, 0.8);
}

.ui-transition-hover-shadow:hover {
    box-shadow: var(--ui-transition-hover-shadow, 0 4px 8px rgba(0, 0, 0, 0.15));
}

.ui-transition-hover-color:hover {
    color: var(--ui-transition-hover-color, var(--palette-primary));
}

.ui-transition-hover-background:hover {
    background: var(--ui-transition-hover-background, var(--palette-primary-light));
}

.ui-transition-hover-translate:hover {
    transform: translate(var(--ui-transition-hover-x, 0),
                         var(--ui-transition-hover-y, -2px));
}

.ui-transition-hover-blur:hover {
    filter: blur(var(--ui-transition-hover-amount, 2px));
}

.ui-transition-hover-border:hover {
    border: var(--ui-transition-hover-border);
}

.ui-transition-hover-backdropblur:hover {
    backdrop-filter: blur(var(--ui-transition-hover-amount, 8px));
    -webkit-backdrop-filter: blur(var(--ui-transition-hover-amount, 8px));
}

/* Combined hover effects */
.ui-transition-hover-lift:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
}

.ui-transition-hover-glow:hover {
    box-shadow: 0 0 20px var(--ui-transition-hover-color, rgba(59, 130, 246, 0.5));
    transform: scale(1.02);
}
";

    private static string FocusTransitions() => @"
/* Focus transitions */
.ui-transition-focus-scale:focus {
    transform: scale(var(--ui-transition-focus-scale, 1.05));
}

.ui-transition-focus-shadow:focus {
    box-shadow: var(--ui-transition-focus-shadow, 0 0 0 3px rgba(59, 130, 246, 0.3));
    outline: none;
}

.ui-transition-focus-border:focus {
    border: var(--ui-transition-focus-border);
}
";

    private static string ActiveTransitions() => @"
/* Active transitions */
.ui-transition-active-scale:active {
    transform: scale(var(--ui-transition-active-scale, 0.98));
}
";

    private static string DisabledTransitions() => @"
/* Disabled transitions */
.ui-transition-disabled-fade:disabled {
    opacity: var(--ui-transition-disabled-opacity, 0.5);
    cursor: not-allowed;
}
";

    private static string ModernFeatures() => @"
/* Modern CSS features */
@supports (backdrop-filter: blur(1px)) {
    .ui-transition-hover-backdropblur {
        background: rgba(255, 255, 255, 0.1);
    }
}
";

    private static string AccessibilityFeatures() => @"
/* Prefers reduced motion */
@media (prefers-reduced-motion: reduce) {
    .ui-has-transitions {
        transition: none !important;
    }
}
";
}
