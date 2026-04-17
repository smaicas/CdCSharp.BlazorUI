using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Integration", "BUIDataCollections")]
public class BUIDataCollectionIntegrationTests
{
    private sealed record Person(string Name, int Age);

    private static IEnumerable<Person> Items => [new Person("Alice", 30), new Person("Bob", 25)];

    private static RenderFragment Columns => b =>
    {
        b.OpenComponent<BUIDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
        b.OpenComponent<BUIDataColumn<Person>>(3);
        b.AddAttribute(4, "Header", "Age");
        b.AddAttribute(5, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Age.ToString())));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Grid_And_Cards_Show_Same_Column_Count(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> grid = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        IRenderedComponent<BUIDataCards<Person>> cards = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert — same 2 columns in both components
        grid.FindAll("[role='columnheader']").Should().HaveCount(2);
        // Cards show labels per card: 2 items × 2 columns = 4 labels
        cards.FindAll(".bui-datacards__field-label").Should().HaveCount(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Grid_Show_Correct_Cell_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30)])
            .Add(c => c.Columns, Columns));

        // Assert
        var cells = cut.FindAll("[role='gridcell']");
        cells[0].TextContent.Should().Be("Alice");
        cells[1].TextContent.Should().Be("30");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Cards_Show_Correct_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30)])
            .Add(c => c.Columns, Columns));

        // Assert — card has Name=Alice, Age=30
        var values = cut.FindAll(".bui-datacards__field-value");
        values[0].TextContent.Should().Be("Alice");
        values[1].TextContent.Should().Be("30");
    }
}
