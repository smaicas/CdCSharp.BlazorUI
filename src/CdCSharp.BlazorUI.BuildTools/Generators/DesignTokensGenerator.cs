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
       Stacking context layers.
       Usage: overlays, modals, tooltips.
       ======================================== */
    {{FeatureDefinitions.Tokens.ZIndex.Dropdown}}: {{FeatureDefinitions.Tokens.ZIndex.DropdownValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Sticky}}: {{FeatureDefinitions.Tokens.ZIndex.StickyValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Modal}}: {{FeatureDefinitions.Tokens.ZIndex.ModalValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Tooltip}}: {{FeatureDefinitions.Tokens.ZIndex.TooltipValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Toast}}: {{FeatureDefinitions.Tokens.ZIndex.ToastValue}};

    /* ========================================
       OPACITY STATES
       Visual feedback values.
       Usage: disabled, placeholder, hover.
       ======================================== */
    {{FeatureDefinitions.Tokens.Opacity.Disabled}}: {{FeatureDefinitions.Tokens.Opacity.DisabledValue}};
    {{FeatureDefinitions.Tokens.Opacity.Placeholder}}: {{FeatureDefinitions.Tokens.Opacity.PlaceholderValue}};
    {{FeatureDefinitions.Tokens.Opacity.Hover}}: {{FeatureDefinitions.Tokens.Opacity.HoverValue}};

    /* ========================================
       OUTLINE HIGHLIGHT
       Focus indicator for accessibility.
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
""";
    }
}