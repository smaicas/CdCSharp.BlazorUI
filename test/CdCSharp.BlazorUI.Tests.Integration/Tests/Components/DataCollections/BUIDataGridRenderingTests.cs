using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Rendering", "BUIDataGrid")]
public class BUIDataGridRenderingTests
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
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("data-grid");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Data_Collection_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find("bui-component").HasAttribute("data-bui-data-collection").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Grid_Table_With_Correct_Roles(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find("[role='grid']").Should().NotBeNull();
        cut.Find("[role='row']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Header_Cells_For_Each_Column(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.FindAll("[role='columnheader']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Row_Per_Item(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert — 2 data rows + 1 header row
        cut.FindAll("[role='row']").Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Empty_Content_When_No_Items(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, [])
            .Add(c => c.Columns, Columns)
            .Add(c => c.EmptyContent, b => b.AddContent(0, "No data")));

        // Assert
        cut.Find(".bui-datagrid__empty-cell").TextContent.Should().Contain("No data");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Item_Data_In_Cells(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30)])
            .Add(c => c.Columns, Columns));

        // Assert
        cut.FindAll("[role='gridcell']")[0].TextContent.Should().Be("Alice");
        cut.FindAll("[role='gridcell']")[1].TextContent.Should().Be("30");
    }
}
