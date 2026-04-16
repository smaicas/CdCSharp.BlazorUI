using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.Consumers;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Validation", "BUIInputCheckbox")]
public class BUIInputCheckboxValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputCheckboxConsumer> cut = ctx.Render<TestBUIInputCheckboxConsumer>();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("false");
        cut.FindAll("._bui-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_When_Required_Bool_Is_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputCheckboxConsumer> cut = ctx.Render<TestBUIInputCheckboxConsumer>();

        // Submit without checking
        cut.Find("button.submit-btn").Click();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("true");
        cut.Find("._bui-field-helper--error").TextContent.Should().Contain("You must accept the terms");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Error_After_Checking(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputCheckboxConsumer> cut = ctx.Render<TestBUIInputCheckboxConsumer>();

        // Provoke error
        cut.Find("button.submit-btn").Click();
        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("true");

        // Check the box
        cut.Find(".bui-checkbox").Click();

        // Re-submit
        cut.Find("button.submit-btn").Click();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("false");
        cut.FindAll("._bui-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Checked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputCheckboxConsumer> cut = ctx.Render<TestBUIInputCheckboxConsumer>();

        // Check and submit
        cut.Find(".bui-checkbox").Click();
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Propagate_Value_To_Bound_Model(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputCheckboxConsumer> cut = ctx.Render<TestBUIInputCheckboxConsumer>();

        cut.Instance.BoundModel.Accepted.Should().BeFalse();

        cut.Find(".bui-checkbox").Click();

        cut.Instance.BoundModel.Accepted.Should().BeTrue();
    }
}
