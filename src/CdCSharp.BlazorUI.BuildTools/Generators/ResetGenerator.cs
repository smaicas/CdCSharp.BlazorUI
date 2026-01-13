using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ResetGenerator : IAssetGenerator
{
    public string Name => "CSS Reset";
    public string FileName => "_reset.css";

    public async Task<string> GetContent()
    {
        return $$"""
/* ========================================
   Minimal CSS Reset
   Auto-generated - Do not edit manually
   ======================================== */

*, *::before, *::after {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
}

html {
    font-size: 16px;
    -webkit-text-size-adjust: 100%;
}

body {
    font-family: var({{FeatureDefinitions.Tokens.Typography.FontFamily}});
    font-size: var({{FeatureDefinitions.Tokens.Typography.FontSizeMd}});
    line-height: var({{FeatureDefinitions.Tokens.Typography.LineHeight}});
    background-color: var(--palette-background);
    color: var(--palette-backgroundcontrast);
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
}

button, input, select, textarea {
    font: inherit;
    color: inherit;
    background: transparent;
    border: none;
    outline: none;
}

button {
    cursor: pointer;
}

a {
    color: inherit;
    text-decoration: none;
}

img, svg, video {
    display: block;
    max-width: 100%;
}

ul, ol {
    list-style: none;
}

[hidden] {
    display: none !important;
}
""";
    }
}
