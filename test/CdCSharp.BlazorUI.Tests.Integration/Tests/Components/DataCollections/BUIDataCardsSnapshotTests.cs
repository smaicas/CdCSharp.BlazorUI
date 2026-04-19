using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Snapshots", "BUIDataCards")]
public class BUIDataCardsSnapshotTests
{
    private sealed record Person(string Name, int Age);

    private static IEnumerable<Person> Items => [new Person("Alice", 30), new Person("Bob", 25)];

    private static RenderFragment Columns => b =>
    {
        b.OpenComponent<BUIDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_State_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Empty",
                Html = ctx.Render<BUIDataCards<Person>>(p => p
                    .Add(c => c.Items, [])
                    .Add(c => c.Columns, Columns)).GetNormalizedMarkup()
            },
            new
            {
                Name = "Loading",
                Html = ctx.Render<BUIDataCards<Person>>(p => p
                    .Add(c => c.Items, Items)
                    .Add(c => c.Columns, Columns)
                    .Add(c => c.Loading, true)
                    .Add(c => c.LoadingContent, b => b.AddContent(0, "Loading..."))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Selectable",
                Html = ctx.Render<BUIDataCards<Person>>(p => p
                    .Add(c => c.Items, Items)
                    .Add(c => c.Columns, Columns)
                    .Add(c => c.SelectionMode, SelectionMode.Multiple)).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
