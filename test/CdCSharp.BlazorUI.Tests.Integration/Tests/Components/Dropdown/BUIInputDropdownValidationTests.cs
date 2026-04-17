using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.Consumers;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Validation", "BUIInputDropdown")]
public class BUIInputDropdownValidationTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_No_Error_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<TestBUIInputDropdownValidationConsumer> cut =
            ctx.Render<TestBUIInputDropdownValidationConsumer>();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-error").Should().NotBe("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_After_Submit_Without_Selected_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBUIInputDropdownValidationConsumer> cut =
            ctx.Render<TestBUIInputDropdownValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Validation_Error_Message(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBUIInputDropdownValidationConsumer> cut =
            ctx.Render<TestBUIInputDropdownValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Find("._bui-field-helper--error").TextContent.Should().Contain("Please select an option");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Submit_When_No_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBUIInputDropdownValidationConsumer> cut =
            ctx.Render<TestBUIInputDropdownValidationConsumer>();

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Instance.WasSubmitted.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Validation_When_Value_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<TestBUIInputDropdownValidationConsumer> cut =
            ctx.Render<TestBUIInputDropdownValidationConsumer>();

        // Set model value directly (simulates selection)
        cut.Instance.BoundModel.Selected = "opt1";

        // Act
        cut.Find("button.submit-btn").Click();

        // Assert
        cut.Instance.WasSubmitted.Should().BeTrue();
    }
}
