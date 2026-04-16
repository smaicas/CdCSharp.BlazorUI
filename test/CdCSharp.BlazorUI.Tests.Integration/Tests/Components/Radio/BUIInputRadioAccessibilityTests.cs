using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.Consumers;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Radio;

[Trait("Component Accessibility", "BUIInputRadio")]
public class BUIInputRadioAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Radiogroup_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>();

        cut.Find("[role='radiogroup']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Radio_Role_Per_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>();

        cut.FindAll(".bui-radio__option").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_Per_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.SelectedValue, "opt1"));

        var options = cut.FindAll(".bui-radio__option");
        options[0].GetAttribute("aria-checked").Should().Be("true");
        options[1].GetAttribute("aria-checked").Should().Be("false");
        options[2].GetAttribute("aria-checked").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaDisabled_Per_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.Option3Disabled, true));

        var options = cut.FindAll(".bui-radio__option");
        options[0].GetAttribute("aria-disabled").Should().Be("false");
        options[2].GetAttribute("aria-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Tabindex_On_Options(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.Option3Disabled, true));

        var options = cut.FindAll(".bui-radio__option");
        options[0].GetAttribute("tabindex").Should().Be("0");
        options[2].GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.HelperText, "Pick wisely."));

        IElement firstOption = cut.FindAll(".bui-radio__option")[0];
        string? describedBy = firstOption.GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find("._bui-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }
}
