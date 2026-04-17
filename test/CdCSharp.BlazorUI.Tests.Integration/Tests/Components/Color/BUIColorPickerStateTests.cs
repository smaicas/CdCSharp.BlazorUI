using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Color;

[Trait("Component State", "BUIColorPicker")]
public class BUIColorPickerStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Initialize_With_Provided_Color(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.OutputFormat, ColorOutputFormats.Hex));

        string inputValue = cut.Find(".bui-picker__input").GetAttribute("value") ?? string.Empty;
        inputValue.Should().StartWith("#ff0000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Cycle_Format_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Start in Hex with no actions row so only one .bui-picker__row exists
        IRenderedComponent<BUIColorPicker> cut = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.OutputFormat, ColorOutputFormats.Hex)
            .Add(c => c.ShowActions, false));

        cut.Find(".bui-picker__input").Should().NotBeNull();

        // Click the sync/cycle button — last button inside the single inputs row
        cut.FindAll(".bui-picker__row button").Last().Click();

        // After cycling from Hex, format switches to Rgb — RGB num inputs replace the hex input
        cut.FindAll(".bui-picker__input").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Different_Preview_For_Different_Initial_Colors(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Two instances initialized with different colors show different preview styles
        IRenderedComponent<BUIColorPicker> cut1 = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.Value, new CssColor("#000000")));

        IRenderedComponent<BUIColorPicker> cut2 = ctx.Render<BUIColorPicker>(p => p
            .Add(c => c.Value, new CssColor("#ffffff")));

        string? style1 = cut1.Find(".bui-picker__preview div").GetAttribute("style");
        string? style2 = cut2.Find(".bui-picker__preview div").GetAttribute("style");
        style2.Should().NotBe(style1);
    }
}
