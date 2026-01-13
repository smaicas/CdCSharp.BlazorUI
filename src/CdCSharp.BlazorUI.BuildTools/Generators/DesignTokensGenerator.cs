using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class DesignTokensGenerator : IAssetGenerator
{
    public string Name => "Design Tokens";
    public string FileName => "_tokens.css";

    public async Task<string> GetContent()
    {

        return $$"""
/* ========================================
   Design Tokens
   Auto-generated - Do not edit manually
   ======================================== */

:root {
    /* === SPACING SCALE === */
    {{FeatureDefinitions.Tokens.Spacing.Space1}}: {{FeatureDefinitions.Tokens.Spacing.Space1Value}};
    {{FeatureDefinitions.Tokens.Spacing.Space2}}: {{FeatureDefinitions.Tokens.Spacing.Space2Value}};
    {{FeatureDefinitions.Tokens.Spacing.Space3}}: {{FeatureDefinitions.Tokens.Spacing.Space3Value}};
    {{FeatureDefinitions.Tokens.Spacing.Space4}}: {{FeatureDefinitions.Tokens.Spacing.Space4Value}};
    {{FeatureDefinitions.Tokens.Spacing.Space5}}: {{FeatureDefinitions.Tokens.Spacing.Space5Value}};
    {{FeatureDefinitions.Tokens.Spacing.Space6}}: {{FeatureDefinitions.Tokens.Spacing.Space6Value}};

    /* === TYPOGRAPHY === */
    {{FeatureDefinitions.Tokens.Typography.FontFamily}}: {{FeatureDefinitions.Tokens.Typography.FontFamilyValue}};
    {{FeatureDefinitions.Tokens.Typography.FontMono}}: {{FeatureDefinitions.Tokens.Typography.FontMonoValue}};
    {{FeatureDefinitions.Tokens.Typography.FontSizeSm}}: {{FeatureDefinitions.Tokens.Typography.FontSizeSmValue}};
    {{FeatureDefinitions.Tokens.Typography.FontSizeMd}}: {{FeatureDefinitions.Tokens.Typography.FontSizeMdValue}};
    {{FeatureDefinitions.Tokens.Typography.FontSizeLg}}: {{FeatureDefinitions.Tokens.Typography.FontSizeLgValue}};
    {{FeatureDefinitions.Tokens.Typography.LineHeight}}: {{FeatureDefinitions.Tokens.Typography.LineHeightValue}};

    /* === BORDER RADIUS === */
    {{FeatureDefinitions.Tokens.Radius.Sm}}: {{FeatureDefinitions.Tokens.Radius.SmValue}};
    {{FeatureDefinitions.Tokens.Radius.Md}}: {{FeatureDefinitions.Tokens.Radius.MdValue}};
    {{FeatureDefinitions.Tokens.Radius.Lg}}: {{FeatureDefinitions.Tokens.Radius.LgValue}};
    {{FeatureDefinitions.Tokens.Radius.Full}}: {{FeatureDefinitions.Tokens.Radius.FullValue}};

    /* === TRANSITIONS === */
    {{FeatureDefinitions.Tokens.Transition.Fast}}: {{FeatureDefinitions.Tokens.Transition.FastValue}};
    {{FeatureDefinitions.Tokens.Transition.Normal}}: {{FeatureDefinitions.Tokens.Transition.NormalValue}};
    {{FeatureDefinitions.Tokens.Transition.Slow}}: {{FeatureDefinitions.Tokens.Transition.SlowValue}};

    /* === Z-INDEX SCALE === */
    {{FeatureDefinitions.Tokens.ZIndex.Dropdown}}: {{FeatureDefinitions.Tokens.ZIndex.DropdownValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Sticky}}: {{FeatureDefinitions.Tokens.ZIndex.StickyValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Modal}}: {{FeatureDefinitions.Tokens.ZIndex.ModalValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Tooltip}}: {{FeatureDefinitions.Tokens.ZIndex.TooltipValue}};
    {{FeatureDefinitions.Tokens.ZIndex.Toast}}: {{FeatureDefinitions.Tokens.ZIndex.ToastValue}};

    /* === SHADOWS === */
    {{FeatureDefinitions.Tokens.Shadow.Sm}}: {{FeatureDefinitions.Tokens.Shadow.SmValue}};
    {{FeatureDefinitions.Tokens.Shadow.Md}}: {{FeatureDefinitions.Tokens.Shadow.MdValue}};
    {{FeatureDefinitions.Tokens.Shadow.Lg}}: {{FeatureDefinitions.Tokens.Shadow.LgValue}};

    /* === OPACITY === */
    {{FeatureDefinitions.Tokens.Opacity.Disabled}}: {{FeatureDefinitions.Tokens.Opacity.DisabledValue}};
    {{FeatureDefinitions.Tokens.Opacity.Placeholder}}: {{FeatureDefinitions.Tokens.Opacity.PlaceholderValue}};
    {{FeatureDefinitions.Tokens.Opacity.Hover}}: {{FeatureDefinitions.Tokens.Opacity.HoverValue}};
}
""";
    }
}
