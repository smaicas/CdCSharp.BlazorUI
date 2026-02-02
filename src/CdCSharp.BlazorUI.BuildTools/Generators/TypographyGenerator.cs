using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class TypographyGenerator : IAssetGenerator
{
    public string FileName => "_typography.css";
    public string Name => "Typography";

    public async Task<string> GetContent()
    {
        return $$"""
/* ========================================
   Typography System
   Auto-generated - Do not edit manually
   ======================================== */

:root {
    {{FeatureDefinitions.Typography.FontFamily}}: {{FeatureDefinitions.Typography.FontFamilyValue}};
    {{FeatureDefinitions.Typography.FontFamilyHeading}}: {{FeatureDefinitions.Typography.FontFamilyHeadingValue}};
    {{FeatureDefinitions.Typography.FontMono}}: {{FeatureDefinitions.Typography.FontMonoValue}};

    /* Fluid typography: scales from 0.875rem (640px) to 1.125rem (1536px) */
    {{FeatureDefinitions.Typography.FontSizeBase}}: clamp(0.875rem, 0.75rem + 0.25vw, 1.125rem);

    {{FeatureDefinitions.Typography.LineHeight}}: {{FeatureDefinitions.Typography.LineHeightValue}};
    {{FeatureDefinitions.Typography.LineHeightHeading}}: {{FeatureDefinitions.Typography.LineHeightHeadingValue}};
}

/* ========================================
   Headings - Scale: 1.25 ratio (Major Third)
   ======================================== */

h1, h2, h3, h4, h5, h6 {
    font-family: var({{FeatureDefinitions.Typography.FontFamilyHeading}});
    font-weight: 700;
    line-height: var({{FeatureDefinitions.Typography.LineHeightHeading}});
}

h1 { font-size: 2.441em; }
h2 { font-size: 1.953em; }
h3 { font-size: 1.563em; }
h4 { font-size: 1.25em; }
h5 { font-size: 1em; }
h6 { font-size: 0.875em; }

/* ========================================
   Paragraph & Inline Text
   ======================================== */

p { line-height: var({{FeatureDefinitions.Typography.LineHeight}}); }
small { font-size: 0.875em; }
strong, b { font-weight: 700; }
em, i { font-style: italic; }

mark {
    background-color: var(--palette-highlight, #fef08a);
    padding-inline: 0.25em;
    padding-block: 0.125em;
}

/* ========================================
   Links
   ======================================== */

a {
    color: var(--palette-primary);
    transition: color 150ms ease;
}

a:hover { text-decoration: underline; }

a:focus-visible {
    outline: 2px solid var(--palette-primary);
    outline-offset: var(--bui-highlight-outline-offset);
}

/* ========================================
   Code & Preformatted
   ======================================== */

code, kbd, samp {
    font-family: var({{FeatureDefinitions.Typography.FontMono}});
    font-size: 0.875em;
    background-color: var(--palette-surface);
    padding-inline: 0.375em;
    padding-block: 0.125em;
    border-radius: 4px;
}

pre {
    font-family: var({{FeatureDefinitions.Typography.FontMono}});
    font-size: 0.875em;
    line-height: 1.6;
    background-color: var(--palette-surface);
    padding: 1em;
    border-radius: 4px;
    overflow-x: auto;
    white-space: pre;
}

pre code {
    background-color: transparent;
    padding: 0;
    border-radius: 0;
}

/* ========================================
   Blockquote & HR
   ======================================== */

blockquote {
    border-inline-start: 4px solid var(--palette-primary);
    padding-inline-start: 1em;
    font-style: italic;
    color: var(--palette-surfacecontrast);
}

hr {
    border: none;
    border-block-start: 1px solid var(--palette-border, currentColor);
    opacity: 0.2;
}

/* ========================================
   Selection
   ======================================== */

::selection {
    background-color: var(--palette-primary);
    color: var(--palette-primarycontrast);
}
""";
    }
}