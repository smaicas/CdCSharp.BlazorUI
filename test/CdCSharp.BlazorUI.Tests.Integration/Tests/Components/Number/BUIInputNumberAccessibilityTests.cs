using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Number;

[Trait("Component Accessibility", "BUIInputNumber")]
public class BUIInputNumberAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Associate_Label_With_Input_Via_For(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty"));

        IElement input = cut.Find("input.bui-input__field");
        IElement label = cut.Find("label.bui-input__label");
        string? inputId = input.GetAttribute("id");
        inputId.Should().NotBeNullOrWhiteSpace();
        label.GetAttribute("for").Should().Be(inputId);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaLabel_From_Placeholder_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Placeholder, "Enter qty"));

        cut.Find("input.bui-input__field").GetAttribute("aria-label").Should().Be("Enter qty");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaInvalid_Matching_Error(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Error, true));

        cut.Find("input.bui-input__field").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaRequired(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Required, true));

        cut.Find("input.bui-input__field").GetAttribute("aria-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.HelperText, "Enter a number."));

        string? describedBy = cut.Find("input.bui-input__field").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find("._bui-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_InputMode_Decimal(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<decimal>> cut = ctx.Render<BUIInputNumber<decimal>>();

        cut.Find("input.bui-input__field").GetAttribute("inputmode").Should().Be("decimal");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Increment_Decrement_Aria_Labels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.ShowStepButtons, true));

        cut.Markup.Should().Contain("Increment");
        cut.Markup.Should().Contain("Decrement");
    }
}
