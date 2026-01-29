using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

/// <summary>
/// Generates core design tokens: spacing, z-index, opacity. Typography is handled by
/// TypographyGenerator. Border radius, transitions, and shadows are handled by component systems.
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class DesignTokensGenerator : IAssetGenerator
{
    public string FileName => "_tokens.css";
    public string Name => "Design Tokens";

    public async Task<string> GetContent()
{
    return $$"""
:root {
    /* ========================================
       Z-INDEX SCALE
       ======================================== */
    {{FeatureDefinitions.Tokens.ZIndex.Dropdown}}: {{FeatureDefinitions.Tokens.ZIndex.DropdownValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Sticky}}: {{FeatureDefinitions.Tokens.ZIndex.StickyValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Modal}}: {{FeatureDefinitions.Tokens.ZIndex.ModalValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Tooltip}}: {{FeatureDefinitions.Tokens.ZIndex.TooltipValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Toast}}: {{FeatureDefinitions.Tokens.ZIndex.ToastValue}};

    /* ========================================
       OPACITY STATES
       ======================================== */
    {{FeatureDefinitions.Tokens.Opacity.Disabled}}: {{FeatureDefinitions.Tokens.Opacity.DisabledValue}};
    {{FeatureDefinitions.Tokens.Opacity.Placeholder}}: {{FeatureDefinitions.Tokens.Opacity.PlaceholderValue}};
    {{FeatureDefinitions.Tokens.Opacity.Hover}}: {{FeatureDefinitions.Tokens.Opacity.HoverValue}};

    /* ========================================
       OUTLINE HIGHLIGHT
       ======================================== */
    --bui-highlight-outline: 2px solid var(--palette-highlight);
    --bui-highlight-outline-offset: 0px;

    /* ========================================
       SIZE MULTIPLIERS
       ======================================== */
    --bui-small-multiplier: 0.75;
    --bui-medium-multiplier: 1;
    --bui-large-multiplier: 1.25;
}

{{GetRippleStyles()}}
""";
}

    private static string GetRippleStyles() => """
/* ========================================
   RIPPLE EFFECT
   ======================================== */

.bui-ripple {
    position: absolute;
    border-radius: 50%;
    background-color: var(--bui-ripple-color, rgba(255, 255, 255, 0.4));
    transform: scale(0);
    animation: bui-ripple-animation var(--bui-ripple-duration, 600ms) linear;
    pointer-events: none;
}

@keyframes bui-ripple-animation {
    to {
        transform: scale(4);
        opacity: 0;
    }
}
""";
}