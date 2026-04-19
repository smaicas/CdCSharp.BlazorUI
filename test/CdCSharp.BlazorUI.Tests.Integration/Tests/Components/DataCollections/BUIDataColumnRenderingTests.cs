using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Rendering", "BUIDataColumn")]
public class BUIDataColumnRenderingTests
{
    private sealed record Person(string Name, int Age);

    private static IEnumerable<Person> Items => [new Person("Alice", 30), new Person("Bob", 25)];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Register_Column_Header_In_Grid(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BUIDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Full Name");
                b.CloseComponent();
            }));

        // Assert — header cell rendered with column name
        cut.Find("[role='columnheader']").TextContent.Trim().Should().Be("Full Name");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Register_Multiple_Columns(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BUIDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.CloseComponent();
                b.OpenComponent<BUIDataColumn<Person>>(2);
                b.AddAttribute(3, "Header", "Age");
                b.CloseComponent();
            }));

        // Assert
        cut.FindAll("[role='columnheader']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Column_Template_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataGrid<Person>> cut = ctx.Render<BUIDataGrid<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30)])
            .Add(c => c.Columns, b =>
            {
                b.OpenComponent<BUIDataColumn<Person>>(0);
                b.AddAttribute(1, "Header", "Name");
                b.AddAttribute(2, "Template", (RenderFragment<Person>)(item =>
                    b2 => b2.AddContent(0, $"[{item.Name}]")));
                b.CloseComponent();
            }));

        // Assert — template rendered in cell
        cut.Find("[role='gridcell']").TextContent.Should().Be("[Alice]");
    }
}
