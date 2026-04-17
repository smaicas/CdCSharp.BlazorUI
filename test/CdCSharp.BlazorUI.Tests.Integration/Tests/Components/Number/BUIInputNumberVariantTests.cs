using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Number;

[Trait("Component Variants", "BUIInputNumber")]
public class BUIInputNumberVariantTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Outlined_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>();

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("outlined");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Filled_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Variant, BUIInputVariant.Filled));

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("filled");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Standard_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Variant, BUIInputVariant.Standard));

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("standard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BUIInputVariant customVariant = BUIInputVariant.Custom("NeonNumber");

        ctx.Services.AddBlazorUIVariants(builder =>
            builder.ForComponent<BUIInputNumber<int>>()
                   .AddVariant(
                       customVariant,
                       input => builder =>
                       {
                           builder.OpenElement(0, "bui-component");
                           builder.AddAttribute(1, "class", "neon-number");
                           builder.AddContent(2, input.Label);
                           builder.CloseElement();
                       }));

        // Act
        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".neon-number").Should().NotBeNull();
        cut.Markup.Should().Contain("Qty");
    }
}
