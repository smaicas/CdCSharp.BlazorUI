using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

/// <summary>
/// Generates typography CSS including:
/// - Font family variables
/// - Responsive font sizing via media queries
/// - Heading styles (h1-h6)
/// - Text element styles (p, small, strong, em, code, pre, blockquote, a)
/// 
/// Import order: after _reset.css
/// </summary>
[ExcludeFromCodeCoverage]
[AssetGenerator]
public class TypographyGenerator : IAssetGenerator
{
    public string Name => "Typography";
    public string FileName => "_typography.css";

    public async Task<string> GetContent()
    {
        return $$"""
/* ========================================
   Typography System
   Auto-generated - Do not edit manually
   ======================================== */

:root {
    /* Font families */
    {{FeatureDefinitions.Typography.FontFamily}}: {{FeatureDefinitions.Typography.FontFamilyValue}};
    {{FeatureDefinitions.Typography.FontFamilyHeading}}: {{FeatureDefinitions.Typography.FontFamilyHeadingValue}};
    {{FeatureDefinitions.Typography.FontMono}}: {{FeatureDefinitions.Typography.FontMonoValue}};
    
    /* Base font size - default for medium screens */
    {{FeatureDefinitions.Typography.FontSizeBase}}: {{FeatureDefinitions.Typography.FontSizeBaseValue}};
    
    /* Line heights */
    {{FeatureDefinitions.Typography.LineHeight}}: {{FeatureDefinitions.Typography.LineHeightValue}};
    {{FeatureDefinitions.Typography.LineHeightHeading}}: {{FeatureDefinitions.Typography.LineHeightHeadingValue}};
}

/* ========================================
   Responsive Font Sizing
   Mobile-first approach
   ======================================== */

@media (max-width: 640px) {
    :root {
        {{FeatureDefinitions.Typography.FontSizeBase}}: 0.875rem;
    }
}

@media (min-width: 1024px) {
    :root {
        {{FeatureDefinitions.Typography.FontSizeBase}}: 1rem;
    }
}

@media (min-width: 1280px) {
    :root {
        {{FeatureDefinitions.Typography.FontSizeBase}}: 1.0625rem;
    }
}

@media (min-width: 1536px) {
    :root {
        {{FeatureDefinitions.Typography.FontSizeBase}}: 1.125rem;
    }
}

/* ========================================
   Headings
   Scale: 1.25 ratio (Major Third)
   ======================================== */

h1, h2, h3, h4, h5, h6 {
    font-family: var({{FeatureDefinitions.Typography.FontFamilyHeading}});
    font-weight: 700;
    line-height: var({{FeatureDefinitions.Typography.LineHeightHeading}});
    color: inherit;
}

h1 {
    font-size: 2.441em;
}

h2 {
    font-size: 1.953em;
}

h3 {
    font-size: 1.563em;
}

h4 {
    font-size: 1.25em;
}

h5 {
    font-size: 1em;
}

h6 {
    font-size: 0.875em;
}

/* ========================================
   Paragraph & Inline Text
   ======================================== */

p {
    line-height: var({{FeatureDefinitions.Typography.LineHeight}});
}

small {
    font-size: 0.875em;
}

strong, b {
    font-weight: 700;
}

em, i {
    font-style: italic;
}

mark {
    background-color: var(--palette-highlight, #fef08a);
    color: inherit;
    padding: 0.125em 0.25em;
}

/* ========================================
   Links
   ======================================== */

a {
    color: var(--palette-primary);
    text-decoration: none;
    transition: color 150ms ease;
}

a:hover {
    text-decoration: underline;
}

a:focus-visible {
    outline: 2px solid var(--palette-primary);
    outline-offset: 2px;
}

/* ========================================
   Code & Preformatted
   ======================================== */

code, kbd, samp {
    font-family: var({{FeatureDefinitions.Typography.FontMono}});
    font-size: 0.875em;
    background-color: var(--palette-surface);
    padding: 0.125em 0.375em;
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
   Blockquote
   ======================================== */

blockquote {
    border-left: 4px solid var(--palette-primary);
    padding-left: 1em;
    font-style: italic;
    color: var(--palette-surfacecontrast);
}

/* ========================================
   Horizontal Rule
   ======================================== */

hr {
    border: none;
    border-top: 1px solid var(--palette-border, currentColor);
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