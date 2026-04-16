using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Interaction", "BUIInputSwitch")]
public class BUIInputSwitchInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act — click label wrapping the switch
        cut.Find("label.bui-switch").Click();

        // Assert
        captured.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Off_On_Second_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = true;
        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Value, true)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("label.bui-switch").Click();

        // Assert
        captured.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Toggle_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool captured = false;
        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Value, false)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("label.bui-switch").Click();

        // Assert
        captured.Should().BeFalse();
    }
}
