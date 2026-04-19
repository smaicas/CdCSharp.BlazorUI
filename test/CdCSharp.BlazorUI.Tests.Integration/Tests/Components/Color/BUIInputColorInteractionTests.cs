using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Color;

[Trait("Component Interaction", "BUIInputColor")]
public class BUIInputColorInteractionTests
{
    private class Model { public CssColor? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Valid_Hex_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        CssColor? captured = null;
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find("input.bui-input__field").Change("#ff0000");

        captured.Should().NotBeNull();
        captured!.ToString(ColorOutputFormats.Hex).Should().Be("#ff0000");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Value_On_Empty_Input(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = new CssColor("#ff0000") };
        CssColor? captured = new("#ff0000");
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v));

        cut.Find("input.bui-input__field").Change(string.Empty);

        captured.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Picker_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.DisplayMode, ColorPickerDisplayMode.Dropdown));

        cut.FindAll(".bui-input-color__dropdown").Should().BeEmpty();

        // Act — click the palette button (last _BUIBtn inside wrapper)
        cut.Find("[aria-label='Open color picker']").Click();

        cut.Find(".bui-input-color__dropdown").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Picker_On_Overlay_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.DisplayMode, ColorPickerDisplayMode.Dropdown));

        cut.Find("[aria-label='Open color picker']").Click();
        cut.Find(".bui-input-color__dropdown").Should().NotBeNull();

        cut.Find(".bui-input-color__dropdown-overlay").Click();

        cut.FindAll(".bui-input-color__dropdown").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_On_Focus(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");

        cut.Find("input.bui-input__field").Focus();

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Open_Picker_When_ReadOnly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, true));

        cut.Find("[aria-label='Open color picker']").Click();

        cut.FindAll(".bui-input-color__dropdown").Should().BeEmpty();
    }
}
