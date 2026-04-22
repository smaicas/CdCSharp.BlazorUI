using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Rendering", "BUISwitch")]
public class BUISwitchRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("switch");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Active_False_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — Value not set, defaults to false which equals OptionInactive
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-active").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Input_With_Role_Switch(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true));

        // Assert
        cut.Find("input[type='checkbox']").GetAttribute("role").Should().Be("switch");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_Text_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Label, "Enable notifications"));

        // Assert
        cut.Find(".bui-switch__label").TextContent.Should().Be("Enable notifications");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Track_And_Thumb_Elements(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true));

        // Assert
        cut.Find(".bui-switch__track").Should().NotBeNull();
        cut.Find(".bui-switch__thumb").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.Size, BUISize.Large));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Track_Color_Variables_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISwitch<bool>> cut = ctx.Render<BUISwitch<bool>>(p => p
            .Add(c => c.OptionInactive, false)
            .Add(c => c.OptionActive, true)
            .Add(c => c.TrackColorActive, "#00ff00")
            .Add(c => c.TrackColorInactive, "#ff0000"));

        // Assert — inline CSS vars for track colors
        string style = cut.Find("bui-component").GetAttribute("style") ?? string.Empty;
        style.Should().Contain("--bui-inline-track");
    }
}
