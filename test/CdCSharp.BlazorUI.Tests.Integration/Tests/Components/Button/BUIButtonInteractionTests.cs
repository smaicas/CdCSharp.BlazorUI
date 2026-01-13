using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.Consumers;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Button;

[Trait("Component Interaction", "BUIButton")]
public class BUIButtonInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Click_Events(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int clickCount = 0;
        IRenderedComponent<TestBUIButtonConsumer> cut = ctx.Render<TestBUIButtonConsumer>(p => p
            .Add(c => c.ButtonText, "Click Me")
            .Add(c => c.OnButtonClicked, count => clickCount = count));

        // Act
        cut.Find("button").Click();

        // Assert
        clickCount.Should().Be(1);
        cut.Find(".click-count").TextContent.Should().Contain("1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_Click_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool clicked = false;
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Disabled")
            .Add(c => c.Disabled, true)
            .Add(c => c.OnClick, _ => clicked = true));

        // Act
        cut.Find("button").Click();

        // Assert
        clicked.Should().BeFalse();
    }
}