using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Components.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Variants;

public class CustomVariantTests : TestContextBase
{
    private readonly TestTemplates _templates = new();

    [Fact]
    public void CustomVariant_BasicTemplate_InheritsComponentClasses()
    {
        // Arrange
        UIButtonVariant customVariant = UIButtonVariant.Custom("Custom");

        Services.AddBlazorUIVariants(builder => builder.For<UIButton, UIButtonVariant>()
            .Register(customVariant, _templates.BasicCustomTemplate));

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, customVariant)
            .Add(p => p.Text, "Custom Button"));

        // Assert
        cut.Find("button").TextContent.Should().Be("Custom Button");
        // Basic template inherits component classes through @attributes
        cut.Find("button").ShouldHaveClass("ui-button");
        cut.Find("button").ShouldHaveClass("ui-button--custom");
    }

    [Fact]
    public void CustomVariant_OverrideTemplate_ReplacesAllClasses()
    {
        // Arrange - This demonstrates a template that completely overrides classes
        UIButtonVariant overrideVariant = UIButtonVariant.Custom("Override");
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        registry.Register(overrideVariant, _templates.OverrideClassTemplate);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, overrideVariant)
            .Add(p => p.Text, "Override Button"));

        // Assert - Only template classes are present
        AngleSharp.Dom.IElement button = cut.Find("button");
        string? classAttribute = button.GetAttribute("class");

        classAttribute.Should().NotBeNull();
        classAttribute.Should().Be("btn-override-only");
        // Component classes are NOT preserved
        button.ShouldNotHaveClass("ui-button");
    }

    [Fact]
    public void CustomVariant_MergeTemplate_PreservesAllClasses()
    {
        // Arrange - This demonstrates the RECOMMENDED approach
        UIButtonVariant glassVariant = UIButtonVariant.Custom("Glass");
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        registry.Register(glassVariant, _templates.AddsOneClassTemplate);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Button"));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        // Component classes are preserved
        button.ShouldHaveClass("ui-button");
        button.ShouldHaveClass("ui-button--glass");
        // Template class is added
        button.ShouldHaveClass("btn-glass");
    }

    [Fact]
    public void CustomVariant_UserClasses_PreservedInCorrectOrder()
    {
        // Arrange
        UIButtonVariant glassVariant = UIButtonVariant.Custom("Glass");
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        registry.Register(glassVariant, _templates.AddsOneClassTemplate);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Button")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-class-1 user-class-2" }
            }));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        string? classAttribute = button.GetAttribute("class");

        classAttribute.Should().NotBeNull();
        // Order: component classes, template classes, user classes
        classAttribute.Should().Be("ui-button ui-button--glass user-class-1 user-class-2 btn-glass");
    }

    [Fact]
    public void CustomVariant_ComplexTemplate_MergesMultipleClasses()
    {
        // Arrange
        UIButtonVariant complexVariant = UIButtonVariant.Custom("Complex");
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        registry.Register(complexVariant, _templates.AddsOneClassTemplate);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, complexVariant)
            .Add(p => p.Text, "Complex Button")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-extra" },
                { "id", "complex-btn" }
            }));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        string? classAttribute = button.GetAttribute("class");

        // All classes should be present in correct order
        // <original UI Button classes> <added from external classes> <template classes>
        classAttribute.Should().NotBeNull();
        classAttribute.Should().Be("ui-button ui-button--complex user-extra btn-glass");
        button.GetAttribute("id").Should().Be("complex-btn");
    }

    [Fact]
    public void CustomVariant_DataAttributes_UserOverridesTemplate()
    {
        // Arrange - Template where user attributes have priority
        UIButtonVariant dataVariant = UIButtonVariant.Custom("DataVariant");
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        registry.Register(dataVariant, _templates.DataAttributeTemplate);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, dataVariant)
            .Add(p => p.Text, "Test")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
            { "data-id", "user-id" },
            { "data-test", "user-value" }, // Should override template value
            { "class", "user-class" }
            }));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        button.GetAttribute("data-variant").Should().Be("custom"); // Template value preserved
        button.GetAttribute("data-id").Should().Be("user-id"); // User value added
        button.GetAttribute("data-test").Should().Be("user-value"); // User overrides template
    }

    [Fact]
    public void CustomVariant_DataAttributes_TemplateCanHavePriority()
    {
        // Arrange - Template where template attributes have priority
        UIButtonVariant dataVariant = UIButtonVariant.Custom("DataPriority");
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        registry.Register(dataVariant, _templates.DataAttributeTemplatePriority);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, dataVariant)
            .Add(p => p.Text, "Test")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
            { "data-test", "user-value" } // Will be overridden by template
            }));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        button.GetAttribute("data-test").Should().Be("template-value-priority"); // Template wins
    }

    [Fact]
    public void CustomVariant_UnregisteredVariant_RendersNothing()
    {
        // Arrange
        UIButtonVariant unregistered = UIButtonVariant.Custom("Unregistered");

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, unregistered)
            .Add(p => p.Text, "Should not appear"));

        // Assert
        cut.Markup.Should().BeEmpty();
    }

    [Fact]
    public void CustomVariant_TypedVariant_WorksCorrectly()
    {
        // Arrange
        IVariantRegistry<UIButton, UIButtonVariant> registry = Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();
        registry.Register(MyCustomButtonVariants.MyCustom1, _templates.AddsOneClassTemplate);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, MyCustomButtonVariants.MyCustom1)
            .Add(p => p.Text, "Typed Variant Button"));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-button");
        button.ShouldHaveClass("ui-button--mycustom1");
        button.ShouldHaveClass("btn-glass");
    }
}