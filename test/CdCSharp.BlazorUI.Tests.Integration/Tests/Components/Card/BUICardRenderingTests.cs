using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Card;

[Trait("Component Rendering", "BUICard")]
public class BUICardRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("card");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Header_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Header, b => b.AddContent(0, "Card Title")));

        // Assert
        cut.Find(".bui-card__header").TextContent.Should().Contain("Card Title");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Body_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Card Body")));

        // Assert
        cut.Find(".bui-card__content").TextContent.Should().Contain("Card Body");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Media_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Media, b => b.AddContent(0, "Media")));

        // Assert
        cut.Find(".bui-card__media").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Actions_Slot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Actions, b => b.AddContent(0, "OK")));

        // Assert
        cut.Find(".bui-card__actions").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_MediaHeight_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Media, b => b.AddContent(0, "img"))
            .Add(c => c.MediaHeight, "200px"));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--_card-media-height: 200px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Clickable_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "click me")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-clickable").Should().Be("true");
    }
}
