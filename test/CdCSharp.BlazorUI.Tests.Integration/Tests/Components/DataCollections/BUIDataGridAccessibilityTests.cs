using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Accessibility", "BUIDataGrid")]
public class BUIDataGridAccessibilityTests
{
    private sealed record Person(string Name, int Age);
    private static readonly Expression<Func<Person, object?>> NameExpr = p => (object?)p.Name;

    private static IEnumerable<Person> Items => [new Person("Alice", 30), new Person("Bob", 25)];

    private static RenderFragment SimpleColumns => b =>
    {
        b.OpenComponent<BUIDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Table_Have_Role_Grid(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns));

        // Assert
        cut.Find("[role='grid']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Header_Cells_Have_Role_Columnheader(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns));

        // Assert
        cut.Find("[role='columnheader']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Data_Cells_Have_Role_Gridcell(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns));

        // Assert
        cut.Find("[role='gridcell']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Sorted_Column_Have_Aria_Sort(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Sortable, true)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BUIDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Sortable", true);
                b.AddAttribute(3, "Property", NameExpr);
                b.AddAttribute(4, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
                b.CloseComponent();
            }));

        // Act
        cut.Find(".bui-datagrid__sort-btn").Click();

        // Assert
        cut.Find("[role='columnheader']").GetAttribute("aria-sort").Should().Be("ascending");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Grid_Have_Aria_Rowcount(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, SimpleColumns));

        // Assert
        cut.Find("[role='grid']").GetAttribute("aria-rowcount").Should().Be("2");
    }
}
