using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class TransitionsCssGenerator : IAssetGenerator
{
    public string FileName => "_transition-classes.css";
    public string Name => "Transitions CSS";

    public async Task<string> GetContent() => """
/* ========================================
   Transition Classes
   Auto-generated - Do not edit manually
   ======================================== */

/* === BASE TRANSITION SETUP === */

bui-component[data-bui-transitions] {
    --_t-duration: var(--bui-transition-duration, 200ms);
    --_t-easing: var(--bui-transition-easing, ease-in-out);
    --_t-delay: var(--bui-transition-delay, 0ms);
    
    --_hover-transform: none;
    --_hover-opacity: 1;
    --_hover-filter: none;
    --_hover-shadow: none;
    --_hover-color: inherit;
    --_hover-bg: inherit;
    --_hover-border: inherit;
    --_hover-backdrop: none;
    
    transition: 
        transform var(--_t-duration) var(--_t-easing) var(--_t-delay),
        opacity var(--_t-duration) var(--_t-easing) var(--_t-delay),
        filter var(--_t-duration) var(--_t-easing) var(--_t-delay),
        box-shadow var(--_t-duration) var(--_t-easing) var(--_t-delay),
        color var(--_t-duration) var(--_t-easing) var(--_t-delay),
        background var(--_t-duration) var(--_t-easing) var(--_t-delay),
        border var(--_t-duration) var(--_t-easing) var(--_t-delay),
        backdrop-filter var(--_t-duration) var(--_t-easing) var(--_t-delay);
}

/* === HOVER STATE APPLICATION === */

bui-component[data-bui-transitions]:hover:not(:has(:disabled)) {
    transform: var(--_hover-transform);
    opacity: var(--_hover-opacity);
    filter: var(--_hover-filter);
    box-shadow: var(--_hover-shadow);
    color: var(--_hover-color);
    background: var(--_hover-bg);
    border: var(--_hover-border);
    backdrop-filter: var(--_hover-backdrop);
    -webkit-backdrop-filter: var(--_hover-backdrop);
}

/* === HOVER TRANSITION DEFINITIONS === */

bui-component[data-bui-transitions~="bui-transition-hover-scale"] {
    --_hover-transform: scale(var(--bui-transition-hover-scale, 1.05));
}

bui-component[data-bui-transitions~="bui-transition-hover-rotate"] {
    --_hover-transform: rotate(var(--bui-transition-hover-rotate, 5deg));
}

bui-component[data-bui-transitions~="bui-transition-hover-translate"] {
    --_hover-transform: translate(var(--bui-transition-hover-x, 0), var(--bui-transition-hover-y, -2px));
}

bui-component[data-bui-transitions~="bui-transition-hover-fade"] {
    --_hover-opacity: var(--bui-transition-hover-opacity, 0.8);
}

bui-component[data-bui-transitions~="bui-transition-hover-shadow"] {
    --_hover-shadow: var(--bui-transition-hover-shadow, 0 4px 8px rgba(0,0,0,0.15));
}

bui-component[data-bui-transitions~="bui-transition-hover-color"] {
    --_hover-color: var(--bui-transition-hover-color, var(--palette-primary));
}

bui-component[data-bui-transitions~="bui-transition-hover-background"] {
    --_hover-bg: var(--bui-transition-hover-background, var(--palette-primary-light));
}

bui-component[data-bui-transitions~="bui-transition-hover-blur"] {
    --_hover-filter: blur(var(--bui-transition-hover-amount, 2px));
}

bui-component[data-bui-transitions~="bui-transition-hover-border"] {
    --_hover-border: var(--bui-transition-hover-border);
}

bui-component[data-bui-transitions~="bui-transition-hover-backdropblur"] {
    --_hover-backdrop: blur(var(--bui-transition-hover-amount, 8px));
}

/* === COMBINED HOVER EFFECTS === */

bui-component[data-bui-transitions~="bui-transition-hover-lift"] {
    --_hover-transform: translateY(-4px);
    --_hover-shadow: 0 8px 16px rgba(0,0,0,0.2);
}

bui-component[data-bui-transitions~="bui-transition-hover-glow"] {
    --_hover-shadow: 0 0 20px var(--bui-transition-hover-color, rgba(59,130,246,0.5));
    --_hover-transform: scale(1.02);
}

/* === FOCUS STATE === */

bui-component[data-bui-transitions]:focus-within {
    --_focus-transform: none;
    --_focus-shadow: none;
    --_focus-border: inherit;
}

bui-component[data-bui-transitions~="bui-transition-focus-scale"] {
    --_focus-transform: scale(var(--bui-transition-focus-scale, 1.05));
}

bui-component[data-bui-transitions~="bui-transition-focus-shadow"] {
    --_focus-shadow: var(--bui-transition-focus-shadow, 0 0 0 3px rgba(59,130,246,0.3));
}

bui-component[data-bui-transitions~="bui-transition-focus-border"] {
    --_focus-border: var(--bui-transition-focus-border);
}

bui-component[data-bui-transitions]:focus-within {
    transform: var(--_focus-transform);
    box-shadow: var(--_focus-shadow);
    border: var(--_focus-border);
    outline: none;
}

/* === ACTIVE STATE === */

bui-component[data-bui-transitions~="bui-transition-active-scale"]:active {
    transform: scale(var(--bui-transition-active-scale, 0.98));
}

/* === DISABLED STATE === */

bui-component[data-bui-transitions~="bui-transition-disabled-fade"]:has(:disabled) {
    opacity: var(--bui-transition-disabled-opacity, 0.5);
    cursor: not-allowed;
}

/* === REDUCED MOTION === */

@media (prefers-reduced-motion: reduce) {
    bui-component[data-bui-transitions] {
        transition: none;
    }
}
""";
}