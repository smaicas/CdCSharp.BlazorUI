using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Badge;

[Trait("Component Rendering", "BUIBadge")]
public class BUIBadgeRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "New")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("badge");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Bui_Badge_Span(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "3")));

        // Assert
        cut.Find("span.bui-badge").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "42")));

        // Assert
        cut.Find("span.bui-badge").TextContent.Should().Contain("42");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dot_Attribute_When_No_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — no ChildContent = dot mode
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>();

        // Assert — data-bui-dot emitted when no content
        cut.Find("bui-component").GetAttribute("data-bui-dot").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Dot_Attribute_When_Content_Present(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "5")));

        // Assert — no dot when content is present
        cut.Find("bui-component").GetAttribute("data-bui-dot").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Circular_Attribute_When_Circular_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.Circular, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "1")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-circular").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Background_Color_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.BackgroundColor, "red")
            .Add(c => c.ChildContent, b => b.AddContent(0, "!")));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-background");
    }
}
