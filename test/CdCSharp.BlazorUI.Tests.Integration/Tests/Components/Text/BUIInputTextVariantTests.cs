using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Text;

[Trait("Component Variants", "BUIInputText")]
public class BUIInputTextVariantTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Default_To_Outlined_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("outlined");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Filled_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Variant, BUIInputVariant.Filled));

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("filled");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Standard_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Variant, BUIInputVariant.Standard));

        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("standard");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Custom_Variant_Template(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        BUIInputVariant customVariant = BUIInputVariant.Custom("GlassInput");

        ctx.Services.AddBlazorUIVariants(builder =>
            builder.ForComponent<BUIInputText>()
                   .AddVariant(
                       customVariant,
                       input => builder =>
                       {
                           builder.OpenElement(0, "bui-component");
                           builder.AddAttribute(1, "class", "glass-input");
                           builder.AddContent(2, input.Label);
                           builder.CloseElement();
                       }));

        Model model = new();

        // Act
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "My Label")
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Variant, customVariant));

        // Assert
        cut.Find(".glass-input").Should().NotBeNull();
        cut.Markup.Should().Contain("My Label");
    }
}
