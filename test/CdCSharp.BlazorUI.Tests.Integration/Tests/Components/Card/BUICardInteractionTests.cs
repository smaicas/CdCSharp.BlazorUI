using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Card;

[Trait("Component Interaction", "BUICard")]
public class BUICardInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Invoke_OnClick_When_Clickable_And_Card_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        int clicks = 0;

        // Arrange
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicks++))
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Act
        cut.Find(".bui-card").Click();

        // Assert
        clicks.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Invoke_OnClick_When_Not_Clickable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        int clicks = 0;

        // Arrange
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Clickable, false)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicks++))
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Act
        cut.Find(".bui-card").Click();

        // Assert
        clicks.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Throw_When_Clickable_Without_OnClick(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Act
        Action act = () => cut.Find(".bui-card").Click();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Invoke_OnClick_On_Every_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        int clicks = 0;

        // Arrange
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicks++))
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        // Act
        cut.Find(".bui-card").Click();
        cut.Find(".bui-card").Click();
        cut.Find(".bui-card").Click();

        // Assert
        clicks.Should().Be(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Stop_Invoking_OnClick_After_Disabling_Clickable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        int clicks = 0;

        // Arrange
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Clickable, true)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicks++))
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));

        cut.Find(".bui-card").Click();
        clicks.Should().Be(1);

        // Act — flip to non-clickable
        cut.Render(p => p
            .Add(c => c.Clickable, false)
            .Add(c => c.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicks++))
            .Add(c => c.ChildContent, b => b.AddContent(0, "Body")));
        cut.Find(".bui-card").Click();

        // Assert — subsequent click ignored
        clicks.Should().Be(1);
    }
}
