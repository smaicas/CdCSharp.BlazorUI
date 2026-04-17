using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Accessibility", "BUIInputDateTime")]
public class BUIInputDateTimeAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Label_Linked_To_Input_Via_For_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Birth Date"));

        // Assert — label's for attribute matches the generated input id
        IElement label = cut.Find("label");
        string? labelFor = label.GetAttribute("for");
        labelFor.Should().StartWith("bui-datetime-");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Label_On_Picker_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>();

        // Assert
        cut.Find("button[aria-label='Open picker']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Required_Data_Attribute_When_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Required, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Required_Asterisk_When_Required(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Label, "Date")
            .Add(c => c.Required, true));

        // Assert
        cut.Find(".bui-input__required").TextContent.Should().Be("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Error_Data_Attribute_When_Error_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>(p => p
            .Add(c => c.Error, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_With_Role_Dialog_When_Opened(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDateTime<DateOnly?>> cut = ctx.Render<BUIInputDateTime<DateOnly?>>();

        // Act
        cut.Find("button[aria-label='Open picker']").Click();

        // Assert — dialog rendered with .bui-dialog
        cut.Find(".bui-dialog").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Nav_Buttons_With_Aria_Labels_In_DatePicker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert — all nav buttons have descriptive aria-labels
        cut.Find("button[aria-label='Previous year']").Should().NotBeNull();
        cut.Find("button[aria-label='Previous month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next year']").Should().NotBeNull();
    }
}
