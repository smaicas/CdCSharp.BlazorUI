using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Text;

[Trait("Component Rendering", "BUIInputText")]
public class BUIInputTextRenderingTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.ValueExpression, () => model.Value));

        // Assert
        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-component").Should().Be("input-text");
        root.GetAttribute("data-bui-input-base").Should().NotBeNull();
        root.GetAttribute("data-bui-variant").Should().Be("outlined");
        root.GetAttribute("data-bui-size").Should().Be("medium");
        root.GetAttribute("data-bui-density").Should().Be("standard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Input_Field_With_Label_And_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = "hello" };
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Value, "hello")
            .Add(c => c.Label, "Full name")
            .Add(c => c.HelperText, "As it appears on your ID.")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bui-input__field").GetAttribute("value").Should().Be("hello");
        cut.Find("label.bui-input__label").TextContent.Should().Contain("Full name");
        cut.Find("._bui-field-helper").TextContent.Should().Contain("As it appears on your ID.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_False_When_Empty_And_Unfocused(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "Empty")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataBuiFloated_True_When_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new() { Value = "prefilled" };
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Value, "prefilled")
            .Add(c => c.Label, "With value")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Prefix_And_Suffix_Addons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "Website")
            .Add(c => c.PrefixText, "https://")
            .Add(c => c.SuffixText, ".com")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find(".bui-input__addon--prefix").TextContent.Should().Contain("https://");
        cut.Find(".bui-input__addon--suffix").TextContent.Should().Contain(".com");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Design_DataAttributes_And_InlineVars(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "Styled")
            .Add(c => c.Size, BUISize.Large)
            .Add(c => c.Density, BUIDensity.Compact)
            .Add(c => c.Color, "rgba(10,20,30,1)")
            .Add(c => c.BackgroundColor, "rgba(40,50,60,1)")
            .Add(c => c.Shadow, BUIShadowPresets.Elevation(2))
            .Add(c => c.ValueExpression, () => model.Value));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-size").Should().Be("large");
        root.GetAttribute("data-bui-density").Should().Be("compact");
        root.GetAttribute("data-bui-shadow").Should().Be("true");

        string style = root.GetAttribute("style") ?? string.Empty;
        style.Should().Contain("--bui-inline-color: rgba(10,20,30,1)");
        style.Should().Contain("--bui-inline-background: rgba(40,50,60,1)");
        style.Should().Contain("--bui-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Placeholder, "Write here")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.FindAll("label.bui-input__label").Should().BeEmpty();
        cut.Find("input.bui-input__field").GetAttribute("aria-label").Should().Be("Write here");
    }
}
