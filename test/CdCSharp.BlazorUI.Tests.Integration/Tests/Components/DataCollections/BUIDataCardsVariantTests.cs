using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.DataCollections;

[Trait("Component Variants", "BUIDataCards")]
public class BUIDataCardsVariantTests
{
    private sealed record Person(string Name, int Age);

    private static IEnumerable<Person> Items => [new Person("Alice", 30)];

    private static RenderFragment Columns => b =>
    {
        b.OpenComponent<BUIDataColumn<Person>>(0);
        b.AddAttribute(1, "Header", "Name");
        b.AddAttribute(2, "Template", (RenderFragment<Person>)(item => b2 => b2.AddContent(0, item.Name)));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        DataCardsVariant custom = DataCardsVariant.Custom("Compact");
        ctx.Services.AddBlazorUIVariants(b => b
            .ForComponent<BUIDataCards<Person>>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-datacards-compact");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BUIDataCards<Person>> cut = ctx.Render<BUIDataCards<Person>>(p => p
            .Add(c => c.Items, Items)
            .Add(c => c.Columns, Columns)
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-datacards-compact").Should().NotBeNull();
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("compact");
    }
}
