using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Color;

[Trait("Component Interaction", "BUIColorPicker")]
public class BUIColorPickerInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Hue_Slider_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        CssColor? captured = null;
        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find(".bui-colorpicker__slider--hue input").Input("120");

        captured.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Alpha_Slider_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        CssColor? captured = null;
        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find(".bui-colorpicker__slider--alpha input").Input("128");

        captured.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Valid_Hex_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        CssColor? captured = null;
        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Hex)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find(".bui-picker__input").Change("#00ff00");

        captured.Should().NotBeNull();
        captured!.ToString(ColorOutputFormats.Hex).Should().Be("#00ff00");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnRevert_Callback(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        bool reverted = false;
        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.ShowActions, true)
            .Add(c => c.RevertText, "Revert")
            .Add(c => c.OnRevert, EventCallback.Factory.Create(this, () => reverted = true)));

        cut.Find(".bui-picker__row:last-child button").Click();

        reverted.Should().BeTrue();
    }
}
