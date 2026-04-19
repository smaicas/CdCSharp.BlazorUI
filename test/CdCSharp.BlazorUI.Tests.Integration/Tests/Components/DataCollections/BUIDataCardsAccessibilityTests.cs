using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Accessibility", "BUIDataCards")]
public class BUIDataCardsAccessibilityTests
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
    public async Task Should_Grid_Container_Have_Role_List(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find(".bui-datacards__grid").GetAttribute("role").Should().Be("list");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Cards_As_Articles(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert — cards use semantic <article> element
        cut.FindAll("article.bui-datacards__card").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Assign_Role_Option_And_Aria_Selected_In_Selection_Mode(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns)
            .Add(c => c.SelectionMode, SelectionMode.Single));

        // Assert
        foreach (AngleSharp.Dom.IElement card in cut.FindAll(".bui-datacards__card"))
        {
            card.GetAttribute("role").Should().Be("option");
            card.GetAttribute("aria-selected").Should().Be("false");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Assign_Role_Option_Without_Selection(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find(".bui-datacards__card").HasAttribute("role").Should().BeFalse();
        cut.Find(".bui-datacards__card").HasAttribute("aria-selected").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Make_Cards_Keyboard_Focusable_When_Interactive(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns)
            .Add(c => c.SelectionMode, SelectionMode.Single));

        // Assert
        cut.Find(".bui-datacards__card").GetAttribute("tabindex").Should().Be("0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_From_Tab_Order_When_Not_Interactive(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find(".bui-datacards__card").GetAttribute("tabindex").Should().Be("-1");
    }
}
