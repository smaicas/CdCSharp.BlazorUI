using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.Consumers;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Radio;

[Trait("Component Rendering", "BUIInputRadio")]
public class BUIInputRadioRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.Label, "Pick one"));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-component").Should().Be("input-radio");
        root.GetAttribute("data-bui-variant").Should().Be("default");
        root.GetAttribute("data-bui-size").Should().Be("medium");
        root.GetAttribute("data-bui-disabled").Should().Be("false");
        root.GetAttribute("data-bui-error").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Orientation_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.Orientation, RadioOrientation.Horizontal));

        cut.Find("bui-component").GetAttribute("data-bui-orientation").Should().Be("horizontal");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Three_Options(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>();

        cut.FindAll(".bui-radio__option").Should().HaveCount(3);
        cut.FindAll(".bui-radio__option").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.Label, "Choose")
            .Add(c => c.HelperText, "Select one option."));

        cut.Find(".bui-radio__label").TextContent.Should().Contain("Choose");
        cut.Find("._bui-field-helper").TextContent.Should().Contain("Select one option.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.Label, "Choice")
            .Add(c => c.Required, true));

        cut.Find("bui-component").GetAttribute("data-bui-required").Should().Be("true");
        cut.Find(".bui-field__required").TextContent.Should().Contain("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Radiogroup_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>();

        cut.Find("[role='radiogroup']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Selected_Option(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputRadioConsumer> cut = ctx.Render<TestBUIInputRadioConsumer>(p => p
            .Add(c => c.SelectedValue, "opt2"));

        var options = cut.FindAll(".bui-radio__option");
        options[0].GetAttribute("aria-checked").Should().Be("false");
        options[1].GetAttribute("aria-checked").Should().Be("true");
        options[2].GetAttribute("aria-checked").Should().Be("false");
    }
}
