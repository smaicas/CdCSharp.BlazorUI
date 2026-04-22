using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.SyntaxHighlight;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component State", "BUICodeBlock")]
public class BUICodeBlockStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Content_When_Code_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "Hello"));

        cut.Find(".bui-code-block__content").InnerHtml.Should().Contain("Hello");

        // Act
        cut.Render(p => p.Add(c => c.Code, "World"));

        // Assert
        cut.Find(".bui-code-block__content").InnerHtml.Should().Contain("World");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Title_When_Language_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, SyntaxHighlightLanguage.CSharp));

        cut.Find(".bui-code-block__title").TextContent.Should().Be("CSHARP");

        // Act
        cut.Render(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, SyntaxHighlightLanguage.Json));

        // Assert
        cut.Find(".bui-code-block__title").TextContent.Should().Be("JSON");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_Attribute_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Size, BUISize.Small));

        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("small");

        // Act
        cut.Render(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Size, BUISize.Large));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Custom_Title_Override_Language_Name(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, SyntaxHighlightLanguage.TypeScript));

        cut.Find(".bui-code-block__title").TextContent.Should().Be("TYPESCRIPT");

        // Act
        cut.Render(p => p
            .Add(c => c.Code, "x")
            .Add(c => c.Language, SyntaxHighlightLanguage.TypeScript)
            .Add(c => c.Title, "My Script"));

        // Assert
        cut.Find(".bui-code-block__title").TextContent.Should().Be("My Script");
    }
}
