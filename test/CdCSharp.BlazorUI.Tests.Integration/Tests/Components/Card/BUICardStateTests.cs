using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Card;

[Trait("Component State", "BUICard")]
public class BUICardStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Content_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "First")));

        cut.Find(".bui-card__content").TextContent.Should().Contain("First");

        // Act
        cut.Render(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Second")));

        // Assert
        cut.Find(".bui-card__content").TextContent.Should().Contain("Second");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Shadow_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Shadow, ShadowStyle.Create(4, 8))
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-shadow").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Border_InlineVar(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Border, BorderStyle.Create().All("1px", BorderStyleType.Solid, "red"))
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-border");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnClick_When_Clickable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool clicked = false;
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, _ => clicked = true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Act
        cut.Find(".bui-card").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_UserAttributes_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .AddUnmatched("data-testid", "my-card")
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Act
        cut.Render(p => p
            .AddUnmatched("data-testid", "my-card")
            .Add(c => c.ChildContent, b => b.AddContent(0, "updated")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-testid").Should().Be("my-card");
    }
}
