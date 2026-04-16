using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Accessibility", "BUIInputCheckbox")]
public class BUIInputCheckboxAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Checkbox_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Accept"));

        cut.Find(".bui-checkbox").GetAttribute("role").Should().Be("checkbox");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_False_When_Unchecked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, false));

        cut.Find(".bui-checkbox").GetAttribute("aria-checked").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_True_When_Checked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, true));

        cut.Find(".bui-checkbox").GetAttribute("aria-checked").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_Mixed_When_Indeterminate(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool?>> cut = ctx.Render<BUIInputCheckbox<bool?>>(p => p
            .Add(c => c.Value, (bool?)null));

        cut.Find(".bui-checkbox").GetAttribute("aria-checked").Should().Be("mixed");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaDisabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Disabled, true));

        cut.Find(".bui-checkbox").GetAttribute("aria-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaLabelledBy_To_Label_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Terms"));

        IElement checkbox = cut.Find(".bui-checkbox");
        string? labelledBy = checkbox.GetAttribute("aria-labelledby");
        labelledBy.Should().NotBeNullOrWhiteSpace();

        IElement label = cut.Find(".bui-checkbox__label");
        label.GetAttribute("id").Should().Be(labelledBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_AriaLabelledBy_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>();

        cut.Find(".bui-checkbox").GetAttribute("aria-labelledby").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_TabIndex_0_When_Enabled_And_Minus1_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Disabled, false));

        cut.Find(".bui-checkbox").GetAttribute("tabindex").Should().Be("0");

        cut.Render(p => p.Add(c => c.Disabled, true));

        cut.Find(".bui-checkbox").GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.HelperText, "Select to continue"));

        string? describedBy = cut.Find(".bui-checkbox").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find("._bui-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }
}
