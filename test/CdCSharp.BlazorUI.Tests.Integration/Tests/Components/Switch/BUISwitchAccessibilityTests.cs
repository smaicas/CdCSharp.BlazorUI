using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Accessibility", "BUISwitch")]
public class BUISwitchAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Role_Switch_On_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true));

        // Assert
        cut.Find("input").GetAttribute("role").Should().Be("switch");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Checked_False_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true));

        // Assert
        cut.Find("input").GetAttribute("aria-checked").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Default_Aria_Label_Toggle_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true));

        // Assert — fallback aria-label is "Toggle"
        cut.Find("input").GetAttribute("aria-label").Should().Be("Toggle");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Custom_Aria_Label_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.AriaLabel, "Dark mode"));

        // Assert
        cut.Find("input").GetAttribute("aria-label").Should().Be("Dark mode");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Label_Be_Associated_With_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.InputId, "my-switch"));

        // Assert — label for= matches input id=
        string? inputId = cut.Find("input").GetAttribute("id");
        string? labelFor = cut.Find("label").GetAttribute("for");
        inputId.Should().Be("my-switch");
        labelFor.Should().Be("my-switch");
    }
}
