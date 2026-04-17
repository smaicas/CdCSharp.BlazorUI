using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Card;

[Trait("Component Variants", "BUICard")]
public class BUICardVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Custom_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BUICardVariant custom = BUICardVariant.Custom("Outlined");
        ctx.Services.AddBlazorUIVariants(b => b
            .ForComponent<BUICard>()
            .AddVariant(custom, _ => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-card-outlined");
                builder.CloseElement();
            }));

        // Act
        IRenderedComponent<BUICard> cut = ctx.Render<BUICard>(p => p
            .Add(c => c.Variant, custom));

        // Assert
        cut.Find(".custom-card-outlined").Should().NotBeNull();
    }
}
