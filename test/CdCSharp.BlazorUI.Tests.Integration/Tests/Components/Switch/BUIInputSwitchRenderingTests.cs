using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Rendering", "BUIInputSwitch")]
public class BUIInputSwitchRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Label, "Toggle"));

        // Outer bui-component from BUIInputSwitch
        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-component").Should().Be("input-switch");
        root.GetAttribute("data-bui-variant").Should().Be("default");
        root.GetAttribute("data-bui-size").Should().Be("medium");
        root.GetAttribute("data-bui-density").Should().Be("standard");
        root.GetAttribute("data-bui-disabled").Should().Be("false");
        root.GetAttribute("data-bui-error").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Inner_Switch_With_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Label, "Feature"));

        IElement input = cut.Find("input.bui-switch__input");
        input.GetAttribute("role").Should().Be("switch");
        input.GetAttribute("aria-checked").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Track_And_Thumb(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>();

        cut.Find(".bui-switch__track").Should().NotBeNull();
        cut.Find(".bui-switch__thumb").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_When_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Label, "Dark mode"));

        cut.Find(".bui-switch__label").TextContent.Should().Contain("Dark mode");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>();

        cut.FindAll(".bui-switch__label").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.HelperText, "Enable to activate."));

        cut.Find("._bui-field-helper").TextContent.Should().Contain("Enable to activate.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Size_And_Density(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Size, SizeEnum.Large)
            .Add(c => c.Density, DensityEnum.Compact));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-size").Should().Be("large");
        root.GetAttribute("data-bui-density").Should().Be("compact");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Checked_When_Value_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Value, true));

        cut.Find("input.bui-switch__input").GetAttribute("aria-checked").Should().Be("true");
    }
}
