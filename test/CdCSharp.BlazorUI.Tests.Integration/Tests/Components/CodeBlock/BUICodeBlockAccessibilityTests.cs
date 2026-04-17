using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.SyntaxHighlight;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Accessibility", "BUICodeBlock")]
public class BUICodeBlockAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Label_On_Copy_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert
        cut.Find("button").GetAttribute("aria-label").Should().Be("Copy code");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Copy_Button_Have_Type_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert — type=button prevents accidental form submission
        cut.Find("button").GetAttribute("type").Should().Be("button");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Additional_Attributes_To_Root(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;")
            .AddUnmatched("aria-label", "Code snippet"));

        // Assert
        cut.Find("bui-component").GetAttribute("aria-label").Should().Be("Code snippet");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Title_Span_Reflect_Language_For_Screen_Readers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "{}")
            .Add(c => c.Language, SyntaxHighlightLanguage.Json));

        // Assert — visible title helps screen reader context
        cut.Find(".bui-code-block__title").TextContent.Should().Be("JSON");
    }
}
