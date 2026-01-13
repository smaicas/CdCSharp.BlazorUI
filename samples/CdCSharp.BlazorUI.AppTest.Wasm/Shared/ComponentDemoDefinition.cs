using CdCSharp.BlazorUI.SyntaxHighlight;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.AppTest.Wasm.Shared;

public sealed class ComponentDemoDefinition
{
    public string? Code { get; init; }
    public string? CodeTitle { get; init; }
    public RenderFragment Demo { get; init; }

    public SyntaxHighlightLanguage Language { get; init; }
        = SyntaxHighlightLanguage.Razor;
}