using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Snapshots", "BUIDataCollections")]
public class BUIDataCollectionSnapshotTests
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
    public async Task Should_Match_DataGrid_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Grid_Empty",
                Html = ctx.Render<BUIDataGrid<Person>>(p => p
                    .Add(c => c.Items, [])
                    .Add(c => c.Columns, Columns)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Grid_WithData",
                Html = ctx.Render<BUIDataGrid<Person>>(p => p
                    .Add(c => c.Items, Items)
                    .Add(c => c.Columns, Columns)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Cards_WithData",
                Html = ctx.Render<BUIDataCards<Person>>(p => p
                    .Add(c => c.Items, Items)
                    .Add(c => c.Columns, Columns)).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
