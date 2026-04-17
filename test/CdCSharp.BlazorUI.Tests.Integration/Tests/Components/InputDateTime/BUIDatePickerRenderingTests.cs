using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Globalization;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component Rendering", "BUIDatePicker")]
public class BUIDatePickerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_DatePicker_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("date-picker");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Picker_Family_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-picker-base").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Navigation_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert
        cut.Find("button[aria-label='Previous year']").Should().NotBeNull();
        cut.Find("button[aria-label='Previous month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next month']").Should().NotBeNull();
        cut.Find("button[aria-label='Next year']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_42_Day_Cell_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert — 42 day buttons plus 7 week header spans = 49 .bui-picker__cell elements
        IReadOnlyList<IElement> dayCells = cut.FindAll(".bui-picker__grid button.bui-picker__cell");
        dayCells.Should().HaveCount(42);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Week_Day_Headers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert — 7 abbreviated weekday header spans
        IReadOnlyList<IElement> headers = cut.FindAll(".bui-picker__grid span.bui-picker__cell");
        headers.Should().HaveCount(7);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Month_Year_Title(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>();

        // Assert — title shows current month/year
        string expected = DateTime.Today.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
        cut.Find(".bui-picker__title").TextContent.Should().Contain(expected);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Selected_Day_As_Active(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        DateOnly selectedDate = new(2024, 6, 15);
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .Add(c => c.Value, selectedDate));

        // Assert — selected day has _bui-btn--active class
        IReadOnlyList<IElement> activeCells = cut.FindAll(".bui-picker__cell._bui-btn--active");
        activeCells.Should().HaveCount(1);
        activeCells[0].TextContent.Trim().Should().Be("15");
    }
}
