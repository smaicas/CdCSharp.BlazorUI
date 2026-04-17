using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Globalization;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Interaction", "BUIDatePicker")]
public class BUIDatePickerInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Previous_Month(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();
        string initialTitle = cut.Find(".bui-picker__title").TextContent;

        // Act
        cut.Find("button[aria-label='Previous month']").Click();

        // Assert
        string updatedTitle = cut.Find(".bui-picker__title").TextContent;
        string expectedTitle = DateTime.Today.AddMonths(-1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        updatedTitle.Should().Contain(expectedTitle);
        updatedTitle.Should().NotBe(initialTitle);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Next_Month(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();
        string initialTitle = cut.Find(".bui-picker__title").TextContent;

        // Act
        cut.Find("button[aria-label='Next month']").Click();

        // Assert
        string updatedTitle = cut.Find(".bui-picker__title").TextContent;
        string expectedTitle = DateTime.Today.AddMonths(1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        updatedTitle.Should().Contain(expectedTitle);
        updatedTitle.Should().NotBe(initialTitle);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Previous_Year(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Act
        cut.Find("button[aria-label='Previous year']").Click();

        // Assert
        string updatedTitle = cut.Find(".bui-picker__title").TextContent;
        string expectedTitle = DateTime.Today.AddYears(-1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        updatedTitle.Should().Contain(expectedTitle);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Navigate_To_Next_Year(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Act
        cut.Find("button[aria-label='Next year']").Click();

        // Assert
        string updatedTitle = cut.Find(".bui-picker__title").TextContent;
        string expectedTitle = DateTime.Today.AddYears(1).ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        updatedTitle.Should().Contain(expectedTitle);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_ValueChanged_On_Day_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateOnly? captured = null;
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .Add(c => c.ValueChanged, v => captured = v));

        // Act — click first non-muted day cell
        IReadOnlyList<IElement> dayCells = cut.FindAll(".bui-picker__grid button.bui-picker__cell");
        dayCells.First().Click();

        // Assert
        captured.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Selected_Day_After_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DateOnly? selected = null;
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .Add(c => c.Value, new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1))
            .Add(c => c.ValueChanged, v => selected = v));

        // Act — click a day that contains "15"
        IElement day15 = cut.FindAll(".bui-picker__grid button.bui-picker__cell")
            .First(b => b.TextContent.Trim() == "15");
        day15.Click();

        // Assert
        selected.Should().NotBeNull();
        selected!.Value.Day.Should().Be(15);
    }
}
