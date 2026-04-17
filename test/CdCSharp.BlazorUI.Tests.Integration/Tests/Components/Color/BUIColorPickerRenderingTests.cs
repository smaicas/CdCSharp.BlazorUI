using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Color;

[Trait("Component Rendering", "BUIColorPicker")]
public class BUIColorPickerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Picker_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>();

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-component").Should().Be("color-picker");
        root.GetAttribute("data-bui-picker-base").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Hue_Slider(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>();

        cut.Find(".bui-colorpicker__slider--hue input[type='range']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Alpha_Slider(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>();

        cut.Find(".bui-colorpicker__slider--alpha input[type='range']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Preview(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>();

        cut.Find(".bui-picker__preview").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Rgb_Inputs_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Rgba));

        // RGB mode renders 4 inline number inputs (R, G, B, A)
        cut.FindAll("input[aria-label]").Count.Should().BeGreaterThanOrEqualTo(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Hex_Input_When_Hex_Format(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Hex));

        cut.Find(".bui-picker__input").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Actions_When_ShowActions_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.ShowActions, true)
            .Add(c => c.RevertText, "Undo"));

        cut.Markup.Should().Contain("Undo");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Actions_When_ShowActions_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.ShowActions, false));

        cut.FindAll(".bui-picker__row").Should().HaveCount(1);
    }
}
