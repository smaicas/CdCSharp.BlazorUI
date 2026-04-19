using CdCSharp.BuildTools;
using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Generators;

[ExcludeFromCodeCoverage]
[AssetGenerator]
public class ScrollBarGenerator : IAssetGenerator
{
    public string FileName => "_scrollbar.css";
    public string Name => "ScrollBar";

    public async Task<string> GetContent()
    {
        return $$"""
/* =========================
   SCROLLBAR GLOBAL
   ========================= */

/* Firefox */
* {
    scrollbar-width: thin;
    scrollbar-color: var(--palette-primary) var(--palette-surface);
}

/* WebKit (Chrome, Edge, Safari) */
*::-webkit-scrollbar {
    width: 10px;
    height: 10px;
}

*::-webkit-scrollbar-track {
    background: var(--palette-surface);
    border-radius: 8px;
}

*::-webkit-scrollbar-thumb {
    background: var(--palette-primary);
    border-radius: 8px;
    border: 2px solid var(--palette-surface);
}

*::-webkit-scrollbar-thumb:hover {
    background: var(--palette-secondary);
}

*::-webkit-scrollbar-thumb:active {
    background: var(--palette-info);
}

*::-webkit-scrollbar-corner {
    background: var(--palette-surface);
}
""";
    }
}

