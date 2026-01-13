using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class TransitionsCssGenerator : IAssetGenerator
{

    public string Name => "Transitions CSS";

    public string FileName => "_transition-classes.css";

    public async Task<string> GetContent() => """
/* ========================================
   Transition Classes
   Auto-generated - Do not edit manually
   ======================================== */

/* Base transition setup */
bui-component[data-bui-transitions] {
    transition: all var(--bui-transition-duration, 200ms)
                var(--bui-transition-easing, ease-in-out)
                var(--bui-transition-delay, 0ms);
}

/* Hover transitions */
bui-component[data-bui-transitions~="bui-transition-hover-scale"]:hover {
    transform: scale(var(--bui-transition-hover-scale, 1.05));
}

bui-component[data-bui-transitions~="bui-transition-hover-rotate"]:hover {
    transform: rotate(var(--bui-transition-hover-rotate, 5deg));
}

bui-component[data-bui-transitions~="bui-transition-hover-fade"]:hover {
    opacity: var(--bui-transition-hover-opacity, 0.8);
}

bui-component[data-bui-transitions~="bui-transition-hover-shadow"]:hover {
    box-shadow: var(--bui-transition-hover-shadow, 0 4px 8px rgba(0, 0, 0, 0.15));
}

bui-component[data-bui-transitions~="bui-transition-hover-color"]:hover {
    color: var(--bui-transition-hover-color, var(--palette-primary));
}

bui-component[data-bui-transitions~="bui-transition-hover-background"]:hover {
    background: var(--bui-transition-hover-background, var(--palette-primary-light));
}

bui-component[data-bui-transitions~="bui-transition-hover-translate"]:hover {
    transform: translate(var(--bui-transition-hover-x, 0),
                         var(--bui-transition-hover-y, -2px));
}

bui-component[data-bui-transitions~="bui-transition-hover-blur"]:hover {
    filter: blur(var(--bui-transition-hover-amount, 2px));
}

bui-component[data-bui-transitions~="bui-transition-hover-border"]:hover {
    border: var(--bui-transition-hover-border);
}

bui-component[data-bui-transitions~="bui-transition-hover-backdropblur"]:hover {
    backdrop-filter: blur(var(--bui-transition-hover-amount, 8px));
    -webkit-backdrop-filter: blur(var(--bui-transition-hover-amount, 8px));
}

/* Combined hover effects */
bui-component[data-bui-transitions~="bui-transition-hover-lift"]:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
}

bui-component[data-bui-transitions~="bui-transition-hover-glow"]:hover {
    box-shadow: 0 0 20px var(--bui-transition-hover-color, rgba(59, 130, 246, 0.5));
    transform: scale(1.02);
}

/* Focus transitions */
bui-component[data-bui-transitions~="bui-transition-focus-scale"]:focus-within {
    transform: scale(var(--bui-transition-focus-scale, 1.05));
}

bui-component[data-bui-transitions~="bui-transition-focus-shadow"]:focus-within {
    box-shadow: var(--bui-transition-focus-shadow, 0 0 0 3px rgba(59, 130, 246, 0.3));
    outline: none;
}

bui-component[data-bui-transitions~="bui-transition-focus-border"]:focus-within {
    border: var(--bui-transition-focus-border);
}

/* Active transitions */
bui-component[data-bui-transitions~="bui-transition-active-scale"]:active {
    transform: scale(var(--bui-transition-active-scale, 0.98));
}

/* Disabled transitions */
bui-component[data-bui-transitions~="bui-transition-disabled-fade"]:has(:disabled) {
    opacity: var(--bui-transition-disabled-opacity, 0.5);
    cursor: not-allowed;
}

/* Prefers reduced motion */
@media (prefers-reduced-motion: reduce) {
    bui-component[data-bui-transitions] {
        transition: none !important;
    }
}
""";
}
