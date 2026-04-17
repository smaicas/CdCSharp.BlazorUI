using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.Consumers;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.TextArea;

[Trait("Component Validation", "BUIInputTextArea")]
public class BUIInputTextAreaValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputTextAreaConsumer> cut = ctx.Render<TestBUIInputTextAreaConsumer>();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("false");
        cut.FindAll("._bui-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Surface_Error_State_When_Validation_Fails(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputTextAreaConsumer> cut = ctx.Render<TestBUIInputTextAreaConsumer>();

        cut.Find("button.submit-btn").Click();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("true");
        cut.Find("textarea.bui-input__field").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("._bui-field-helper--error").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_ValidationMessage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputTextAreaConsumer> cut = ctx.Render<TestBUIInputTextAreaConsumer>();

        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find("._bui-field-helper--error");
        errorHelper.TextContent.Should().Contain("Bio is required");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_StringLength_ValidationMessage_For_Too_Long(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputTextAreaConsumer> cut = ctx.Render<TestBUIInputTextAreaConsumer>();

        cut.Find("textarea.bui-input__field").Change(new string('x', 25));
        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find("._bui-field-helper--error");
        errorHelper.TextContent.Should().Contain("Too long");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Error_State_After_Valid_Value_Entered(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputTextAreaConsumer> cut = ctx.Render<TestBUIInputTextAreaConsumer>();

        cut.Find("button.submit-btn").Click();
        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("true");

        cut.Find("textarea.bui-input__field").Change("short bio");
        cut.Find("button.submit-btn").Click();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("false");
        cut.Find("textarea.bui-input__field").GetAttribute("aria-invalid").Should().Be("false");
        cut.FindAll("._bui-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_To_Bound_Model(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputTextAreaConsumer> cut = ctx.Render<TestBUIInputTextAreaConsumer>();

        cut.Find("textarea.bui-input__field").Change("hello");

        cut.Instance.BoundModel.Bio.Should().Be("hello");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Value_Passes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputTextAreaConsumer> cut = ctx.Render<TestBUIInputTextAreaConsumer>();

        cut.Find("textarea.bui-input__field").Change("valid bio");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
    }
}
