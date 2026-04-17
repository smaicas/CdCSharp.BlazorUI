using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.Consumers;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Color;

[Trait("Component Validation", "BUIInputColor")]
public class BUIInputColorValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Error_On_Initial_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputColorConsumer> cut = ctx.Render<TestBUIInputColorConsumer>();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("false");
        cut.FindAll("._bui-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Surface_Error_State_When_Required_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputColorConsumer> cut = ctx.Render<TestBUIInputColorConsumer>();

        cut.Find("button.submit-btn").Click();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("true");
        cut.Find("input.bui-input__field").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("._bui-field-helper--error").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_ValidationMessage(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputColorConsumer> cut = ctx.Render<TestBUIInputColorConsumer>();

        cut.Find("button.submit-btn").Click();

        IElement errorHelper = cut.Find("._bui-field-helper--error");
        errorHelper.TextContent.Should().Contain("Color is required");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Clear_Error_After_Valid_Color_Entered(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputColorConsumer> cut = ctx.Render<TestBUIInputColorConsumer>();

        cut.Find("button.submit-btn").Click();
        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("true");

        cut.Find("input.bui-input__field").Change("#00ff00");
        cut.Find("button.submit-btn").Click();

        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("false");
        cut.FindAll("._bui-field-helper--error").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnValidSubmit_When_Color_Present(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<TestBUIInputColorConsumer> cut = ctx.Render<TestBUIInputColorConsumer>();

        cut.Find("input.bui-input__field").Change("#ff0000");
        cut.Find("button.submit-btn").Click();

        cut.Find(".submit-result").TextContent.Should().Be("valid");
    }
}
