using CdCSharp.BlazorUI.BuildTools.Pipeline;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
public class TransitionsCssGenerator : IAssetGenerator
{
    private readonly BuildContext _context;

    public string Name => "Transitions CSS";

    public TransitionsCssGenerator(BuildContext context)
    {
        _context = context;
    }

    public async Task GenerateAsync()
    {
        // Use existing generator
        string css = CssTransitionClasses.GetCss();

        string outputPath = _context.GetFullPath("CssBundle/transition-classes.css");
        await File.WriteAllTextAsync(outputPath, css);
    }
}

public static class CssTransitionClasses
{
    public static string GetCss() => @"
/* ========================================
   Transition Classes
   Auto-generated - Do not edit manually
   ======================================== */

/* Base transition setup */
ui-component[data-ui-transitions] {
    transition: all var(--ui-transition-duration, 200ms)
                var(--ui-transition-easing, ease-in-out)
                var(--ui-transition-delay, 0ms);
}

/* Hover transitions */
ui-component[data-ui-transitions~=""ui-transition-hover-scale""]:hover {
    transform: scale(var(--ui-transition-hover-scale, 1.05));
}

ui-component[data-ui-transitions~=""ui-transition-hover-rotate""]:hover {
    transform: rotate(var(--ui-transition-hover-rotate, 5deg));
}

ui-component[data-ui-transitions~=""ui-transition-hover-fade""]:hover {
    opacity: var(--ui-transition-hover-opacity, 0.8);
}

ui-component[data-ui-transitions~=""ui-transition-hover-shadow""]:hover {
    box-shadow: var(--ui-transition-hover-shadow, 0 4px 8px rgba(0, 0, 0, 0.15));
}

ui-component[data-ui-transitions~=""ui-transition-hover-color""]:hover {
    color: var(--ui-transition-hover-color, var(--palette-primary));
}

ui-component[data-ui-transitions~=""ui-transition-hover-background""]:hover {
    background: var(--ui-transition-hover-background, var(--palette-primary-light));
}

ui-component[data-ui-transitions~=""ui-transition-hover-translate""]:hover {
    transform: translate(var(--ui-transition-hover-x, 0),
                         var(--ui-transition-hover-y, -2px));
}

ui-component[data-ui-transitions~=""ui-transition-hover-blur""]:hover {
    filter: blur(var(--ui-transition-hover-amount, 2px));
}

ui-component[data-ui-transitions~=""ui-transition-hover-border""]:hover {
    border: var(--ui-transition-hover-border);
}

ui-component[data-ui-transitions~=""ui-transition-hover-backdropblur""]:hover {
    backdrop-filter: blur(var(--ui-transition-hover-amount, 8px));
    -webkit-backdrop-filter: blur(var(--ui-transition-hover-amount, 8px));
}

/* Combined hover effects */
ui-component[data-ui-transitions~=""ui-transition-hover-lift""]:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
}

ui-component[data-ui-transitions~=""ui-transition-hover-glow""]:hover {
    box-shadow: 0 0 20px var(--ui-transition-hover-color, rgba(59, 130, 246, 0.5));
    transform: scale(1.02);
}

/* Focus transitions */
ui-component[data-ui-transitions~=""ui-transition-focus-scale""]:focus-within {
    transform: scale(var(--ui-transition-focus-scale, 1.05));
}

ui-component[data-ui-transitions~=""ui-transition-focus-shadow""]:focus-within {
    box-shadow: var(--ui-transition-focus-shadow, 0 0 0 3px rgba(59, 130, 246, 0.3));
    outline: none;
}

ui-component[data-ui-transitions~=""ui-transition-focus-border""]:focus-within {
    border: var(--ui-transition-focus-border);
}

/* Active transitions */
ui-component[data-ui-transitions~=""ui-transition-active-scale""]:active {
    transform: scale(var(--ui-transition-active-scale, 0.98));
}

/* Disabled transitions */
ui-component[data-ui-transitions~=""ui-transition-disabled-fade""]:has(:disabled) {
    opacity: var(--ui-transition-disabled-opacity, 0.5);
    cursor: not-allowed;
}

/* Prefers reduced motion */
@media (prefers-reduced-motion: reduce) {
    ui-component[data-ui-transitions] {
        transition: none !important;
    }
}
";
}