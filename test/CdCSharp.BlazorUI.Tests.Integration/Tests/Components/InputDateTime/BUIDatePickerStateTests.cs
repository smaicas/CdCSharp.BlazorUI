using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Globalization;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputDateTime;

[Trait("Component State", "BUIDatePicker")]
public class BUIDatePickerStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Size_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .Add(c => c.Size, SizeEnum.Large));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Density_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .Add(c => c.Density, DensityEnum.Compact));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-density").Should().Be("compact");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sync_CurrentMonth_To_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .Add(c => c.Value, new DateOnly(2020, 1, 10)));

        // Assert
        string title = cut.Find(".bui-picker__title").TextContent;
        title.Should().Contain(new DateTime(2020, 1, 1).ToString("MMMM yyyy", CultureInfo.CurrentCulture));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Title_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .Add(c => c.Value, new DateOnly(2020, 1, 10)));

        // Act
        cut.Render(p => p.Add(c => c.Value, new DateOnly(2022, 7, 4)));

        // Assert
        string title = cut.Find(".bui-picker__title").TextContent;
        title.Should().Contain(new DateTime(2022, 7, 1).ToString("MMMM yyyy", CultureInfo.CurrentCulture));
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Shift_Active_Day_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .Add(c => c.Value, new DateOnly(2024, 6, 15)));
        cut.FindAll(".bui-picker__cell._bui-btn--active")
            .Single().TextContent.Trim().Should().Be("15");

        // Act
        cut.Render(p => p.Add(c => c.Value, new DateOnly(2024, 6, 22)));

        // Assert
        cut.FindAll(".bui-picker__cell._bui-btn--active")
            .Single().TextContent.Trim().Should().Be("22");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Class_And_Style(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDatePicker> cut = ctx.Render<BUIDatePicker>(p => p
            .AddUnmatched("class", "custom-picker")
            .AddUnmatched("style", "margin: 1rem;"));

        // Assert
        IElement root = cut.Find("bui-component");
        root.GetAttribute("class").Should().Contain("custom-picker");
        root.GetAttribute("style").Should().Contain("margin: 1rem;");
    }
}
