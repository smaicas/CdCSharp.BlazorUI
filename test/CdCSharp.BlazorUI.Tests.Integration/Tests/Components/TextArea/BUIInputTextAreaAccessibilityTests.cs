using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.TextArea;

[Trait("Component Accessibility", "BUIInputTextArea")]
public class BUIInputTextAreaAccessibilityTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Associate_Label_With_Textarea_Via_For_Id(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputTextArea> cut = ctx.Render<BUIInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement textarea = cut.Find("textarea.bui-input__field");
        IElement label = cut.Find("label.bui-input__label");

        string? id = textarea.GetAttribute("id");
        id.Should().NotBeNullOrWhiteSpace();
        label.GetAttribute("for").Should().Be(id);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_AriaLabel_From_Placeholder_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputTextArea> cut = ctx.Render<BUIInputTextArea>(p => p
            .Add(c => c.Placeholder, "Type here")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("textarea.bui-input__field").GetAttribute("aria-label").Should().Be("Type here");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_AriaLabel_When_Label_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputTextArea> cut = ctx.Render<BUIInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.Placeholder, "Type")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("textarea.bui-input__field").GetAttribute("aria-label").Should().BeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaRequired_Matching_Required_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputTextArea> cut = ctx.Render<BUIInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.Required, true)
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("textarea.bui-input__field").GetAttribute("aria-required").Should().Be("true");

        cut.Render(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.Required, false)
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("textarea.bui-input__field").GetAttribute("aria-required").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaInvalid_Matching_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputTextArea> cut = ctx.Render<BUIInputTextArea>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, false));

        cut.Find("textarea.bui-input__field").GetAttribute("aria-invalid").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, true));

        cut.Find("textarea.bui-input__field").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper_Id(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputTextArea> cut = ctx.Render<BUIInputTextArea>(p => p
            .Add(c => c.HelperText, "Details here")
            .Add(c => c.ValueExpression, () => model.Value));

        string? describedBy = cut.Find("textarea.bui-input__field").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find("._bui-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Visual_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputTextArea> cut = ctx.Render<BUIInputTextArea>(p => p
            .Add(c => c.Label, "Notes")
            .Add(c => c.Required, true)
            .Add(c => c.ValueExpression, () => model.Value));

        IElement marker = cut.Find(".bui-input__required");
        marker.TextContent.Should().Contain("*");
    }
}
