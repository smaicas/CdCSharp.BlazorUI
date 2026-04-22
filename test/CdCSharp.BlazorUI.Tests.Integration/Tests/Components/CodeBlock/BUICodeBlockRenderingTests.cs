using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.SyntaxHighlight;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Rendering", "BUICodeBlock")]
public class BUICodeBlockRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("code-block");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Header(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert
        cut.Find(".bui-code-block__header").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content_Area(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert
        cut.Find(".bui-code-block__content").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Default_Language_As_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;")
            .Add(c => c.Language, SyntaxHighlightLanguage.CSharp));

        // Assert — default title = Language.ToString().ToUpperInvariant()
        cut.Find(".bui-code-block__title").TextContent.Should().Be("CSHARP");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Custom_Title_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;")
            .Add(c => c.Title, "My Snippet"));

        // Assert
        cut.Find(".bui-code-block__title").TextContent.Should().Be("My Snippet");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Copy_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;"));

        // Assert — _BUIBtn renders a <button>
        cut.Find("button").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "var x = 1;")
            .Add(c => c.Size, BUISize.Large));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Code_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "Hello World"));

        // Assert — highlighted content contains the code text
        cut.Find(".bui-code-block__content").InnerHtml.Should().Contain("Hello World");
    }
}
