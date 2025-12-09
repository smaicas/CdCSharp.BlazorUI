using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Variants;

public class CustomVariantTests : TestContextBase
{
    [Fact]
    public void CustomVariant_RegisteredInRegistry_RendersCorrectly()
    {
        // Arrange
        UIButtonVariant glassVariant = UIButtonVariant.Custom("Glass");
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();

        registry.Register(glassVariant, component => new TestTemplates().GlassButtonTemplate(component));

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.ChildContent, "Glass Button"));

        // Assert
        cut.Find("button").ShouldHaveClass("btn-glass");
        cut.Find("button").TextContent.Should().Be("Glass Button");
    }

    [Fact]
    public void CustomVariant_RegisteredInRegistryWithAdditionalAttributes_RendersCorrectly()
    {
        // Arrange
        UIButtonVariant glassVariant = UIButtonVariant.Custom("Glass");
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();

        registry.Register(glassVariant, component => new TestTemplates().GlassButtonTemplateWAdditionalAttributes(component));

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.ChildContent, "Glass Button")
            .Add(p => p.AdditionalAttributes, new() { { "custom-param-1", "custom-param-1-value" } })
);

        // Assert
        cut.Find("button").ShouldHaveClass("btn-glass");
        cut.Find("button").ShouldHaveClass("custom-param-1-value");
        cut.Find("button").TextContent.Should().Be("Glass Button");
    }
}
