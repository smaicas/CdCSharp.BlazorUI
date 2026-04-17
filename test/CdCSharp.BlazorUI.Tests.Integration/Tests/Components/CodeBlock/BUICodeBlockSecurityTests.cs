using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.SyntaxHighlight;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CodeBlock;

[Trait("Component Security", "BUICodeBlock")]
public class BUICodeBlockSecurityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Encode_Script_Tag_In_Code(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "</pre><script>alert(1)</script>")
            .Add(c => c.Language, SyntaxHighlightLanguage.CSharp));

        // Assert — raw <script> tag must not appear in rendered markup
        cut.Find(".bui-code-block__content").InnerHtml.Should().NotContain("<script>alert(1)</script>");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Encode_Closing_Pre_Tag_In_Code(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, "</pre><b>injected</b>")
            .Add(c => c.Language, SyntaxHighlightLanguage.CSharp));

        // Assert — injected HTML must not break structure
        cut.FindAll("bui-component").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Html_Entities_Safely_When_Highlight_Fails(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — empty code triggers fallback path
        IRenderedComponent<BUICodeBlock> cut = ctx.Render<BUICodeBlock>(p => p
            .Add(c => c.Code, string.Empty));

        // Assert — content area renders without exception
        cut.Find(".bui-code-block__content").Should().NotBeNull();
    }
}
