using CdCSharp.BlazorUI.SyntaxHighlight;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.AppTest.Wasm.Shared;

public sealed class ComponentDemoDefinition
{
    public required RenderFragment Demo { get; init; }

    public string? Code { get; init; }

    public SyntaxHighlightLanguage Language { get; init; }
        = SyntaxHighlightLanguage.Razor;

    public string? CodeTitle { get; init; }
}
