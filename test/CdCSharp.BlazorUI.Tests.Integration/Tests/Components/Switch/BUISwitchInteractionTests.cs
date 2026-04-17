using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Interaction", "BUISwitch")]
public class BUISwitchInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_To_Active_On_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? capturedValue = null;
        IRenderedComponent<BUISwitch<bool>> cut = null!;
        cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, false)
            .Add(c => c.ValueChanged, v =>
            {
                capturedValue = v;
                cut.Render(p2 => p2
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Value, v));
            }));

        cut.Find("bui-component").GetAttribute("data-bui-active").Should().Be("false");

        // Act
        cut.Find("label").Click();

        // Assert
        capturedValue.Should().Be(true);
        cut.Find("bui-component").GetAttribute("data-bui-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Back_To_Inactive_On_Second_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? capturedValue = null;
        IRenderedComponent<BUISwitch<bool>> cut = null!;
        cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, true)
            .Add(c => c.ValueChanged, v =>
            {
                capturedValue = v;
                cut.Render(p2 => p2
                    .Add(c => c.OptionInactive, false)
                    .Add(c => c.OptionActive, true)
                    .Add(c => c.Value, v));
            }));

        // Act
        cut.Find("label").Click();

        // Assert
        capturedValue.Should().Be(false);
        cut.Find("bui-component").GetAttribute("data-bui-active").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_ValueChanged_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool fired = false;
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, _ => fired = true));

        // Act
        cut.Find("label").Click();

        // Assert
        fired.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Work_With_String_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? capturedValue = null;
        IRenderedComponent<BUISwitch<string>> cut = ctx.Render<BUISwitch<string>>(p => p
            .Add(c => c.OptionInactive, "off")
            .Add(c => c.OptionActive, "on")
            .Add(c => c.Value, "off")
            .Add(c => c.ValueChanged, v => capturedValue = v));

        // Act
        cut.Find("label").Click();

        // Assert
        capturedValue.Should().Be("on");
    }
}
