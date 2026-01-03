using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Library;

[Trait("Library", "VariantRegistry")]
public class VariantRegistryTests
{
    private readonly TestVariantComponent_CustomVariants _templates = new();

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_Should_Register_And_Retrieve_Custom_Variants(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        TestVariant customVariant = TestVariant.Custom("Custom");
        ctx.Services.AddBlazorUIVariants(builder => builder.ForComponent<TestVariantComponent>()
           .AddVariant(customVariant, _templates.BasicCustomTemplate));

        IRenderedComponent<TestVariantComponent> cut = ctx.Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, customVariant)
            .Add(p => p.Text, "Custom Component"));

        // Assert
        cut.Find("button").TextContent.Should().Be("Custom Component");
    }
}