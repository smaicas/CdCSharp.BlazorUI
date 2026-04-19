using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Rendering", "BUIDataCards")]
public class BUIDataCardsRenderingTests
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
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("data-cards");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Data_Collection_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find("bui-component").HasAttribute("data-bui-data-collection").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_One_Card_Per_Item(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.FindAll(".bui-datacards__card").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Card_With_Column_Labels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, [new Person("Alice", 30)])
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find(".bui-datacards__field-label").TextContent.Should().Be("Name");
        cut.Find(".bui-datacards__field-value").TextContent.Should().Be("Alice");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Empty_Content_When_No_Items(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, [])
            .Add(c => c.Columns, Columns)
            .Add(c => c.EmptyContent, b => b.AddContent(0, "No cards")));

        // Assert
        cut.Find("bui-component").TextContent.Should().Contain("No cards");
    }
}
