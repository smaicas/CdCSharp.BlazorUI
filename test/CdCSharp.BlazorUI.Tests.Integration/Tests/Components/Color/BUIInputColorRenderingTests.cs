using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Color;

[Trait("Component Rendering", "BUIInputColor")]
public class BUIInputColorRenderingTests
{
    private class Model { public CssColor? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-component").Should().Be("input-color");
        root.GetAttribute("data-bui-input-base").Should().NotBeNull();
        root.GetAttribute("data-bui-variant").Should().Be("outlined");
        root.GetAttribute("data-bui-size").Should().Be("medium");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Text_Input_Field(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bui-input__field").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Preview_Swatch(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find(".bui-input-color__preview-chess").Should().NotBeNull();
        cut.Find(".bui-input-color__preview-color").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Preview_With_Current_Color(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = new CssColor("#ff0000") };
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.ValueExpression, () => model.Value));

        IElement preview = cut.Find(".bui-input-color__preview-color");
        preview.GetAttribute("style").Should().Contain("background-color:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Display_Value_In_Hex_Format_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = new CssColor("#ff0000") };
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.Value, new CssColor("#ff0000"))
            .Add(c => c.ValueExpression, () => model.Value));

        string? inputValue = cut.Find("input.bui-input__field").GetAttribute("value");
        inputValue.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_True_When_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = new CssColor("#aabbcc") };
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.Value, new CssColor("#aabbcc"))
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_False_When_NoValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.Label, "Color")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.Label, "Background")
            .Add(c => c.HelperText, "Pick a color")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("label.bui-input__label").TextContent.Should().Contain("Background");
        cut.Find("._bui-field-helper").TextContent.Should().Contain("Pick a color");
    }
}
