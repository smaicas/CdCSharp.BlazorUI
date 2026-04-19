using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component State", "BUISwitch")]
public class BUISwitchStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Active_True_When_Value_Equals_OptionActive(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_Attribute_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Disabled, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-disabled").Should().Be("true");
        cut.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Active_On_Value_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, false));

        cut.Find("bui-component").GetAttribute("data-bui-active").Should().Be("false");

        // Act
        cut.Render(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Aria_Checked_When_Active(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Value, true));

        // Assert
        cut.Find("input").GetAttribute("aria-checked").Should().Be("true");
    }
}
