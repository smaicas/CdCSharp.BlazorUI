using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Library;

[Trait("Library", "VariantRegistry")]
public class VariantRegistryTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task VariantRegistry_Should_Register_And_Retrieve_Custom_Variants(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        VariantRegistry? registry = ctx.Services.GetRequiredService<IVariantRegistry>() as VariantRegistry;
        registry.Should().NotBeNull();

        BUIButtonVariant customVariant = BUIButtonVariant.Custom("TestVariant");
        RenderFragment customTemplate(BUIButton button) => builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Custom Variant Template");
            builder.CloseElement();
        };

        // Act
        registry!.Register<BUIButton, BUIButtonVariant>(customVariant, customTemplate);
        RenderFragment? retrieved = registry.GetTemplate(
            typeof(BUIButton),
            customVariant,
            new BUIButton());

        // Assert
        retrieved.Should().NotBeNull();
    }
}