using Bunit;
using CdCSharp.BlazorUI.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Variants;

internal class MyCustomTestVariants : TestVariant
{
    public static readonly TestVariant MyCustom1 = Custom("MyCustom1");

    public static readonly TestVariant MyCustom2 = Custom("MyCustom2");

    public MyCustomTestVariants(string name) : base(name)
    {
    }
}

[Trait("Behavior", "Variants")]
public class CustomVariantTests : TestContextBase
{
    private readonly TestVariantTemplates _templates = new();

    public CustomVariantTests()
    {
        // Register the test component's variant registry
        Services.AddSingleton<IVariantRegistry<TestVariantComponent, TestVariant>>(
            new VariantRegistry<TestVariantComponent, TestVariant>());
    }

    [Fact(DisplayName = "BasicTemplate_InheritsComponentClasses")]
    public void CustomVariant_BasicTemplate_InheritsComponentClasses()
    {
        // Arrange
        TestVariant customVariant = TestVariant.Custom("Custom");

        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(customVariant, _templates.BasicCustomTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, customVariant)
            .Add(p => p.Text, "Custom Component"));

        // Assert
        cut.Find("button").TextContent.Should().Be("Custom Component");
        // Basic template inherits component classes through @attributes
        cut.Find("button").ShouldHaveClass("test-variant-component");
        cut.Find("button").ShouldHaveClass("test-variant-component--custom");
    }

    [Fact(DisplayName = "BasicTemplateVariantBuilder_InheritsComponentClasses")]
    public void CustomVariant_BasicTemplateVariantBuilder_InheritsComponentClasses()
    {
        // Arrange
        TestVariant customVariant = TestVariant.Custom("Custom");

        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(customVariant, _templates.BasicCustomTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, customVariant)
            .Add(p => p.Text, "Custom Component"));

        // Assert
        cut.Find("button").TextContent.Should().Be("Custom Component");
        // Basic template inherits component classes through @attributes
        cut.Find("button").ShouldHaveClass("test-variant-component");
        cut.Find("button").ShouldHaveClass("test-variant-component--custom");
    }

    [Fact(DisplayName = "ComplexTemplate_MergesMultipleClasses")]
    public void CustomVariant_ComplexTemplate_MergesMultipleClasses()
    {
        // Arrange
        TestVariant complexVariant = TestVariant.Custom("Complex");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(complexVariant, _templates.AddsOneClassTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, complexVariant)
            .Add(p => p.Text, "Complex Component")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-extra" },
                { "id", "complex-btn" }
            }));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        string? classAttribute = button.GetAttribute("class");

        // All classes should be present in correct order
        classAttribute.Should().NotBeNull();
        classAttribute.Should().Be("test-variant-component test-variant-component--complex user-extra btn-glass");
        button.GetAttribute("id").Should().Be("complex-btn");
    }

    [Fact(DisplayName = "ComplexTemplateVariantBuilder_MergesMultipleClasses")]
    public void CustomVariant_ComplexTemplateVariantBuilder_MergesMultipleClasses()
    {
        // Arrange
        TestVariant complexVariant = TestVariant.Custom("Complex");

        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(complexVariant, _templates.AddsOneClassTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, complexVariant)
            .Add(p => p.Text, "Complex Component")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-extra" },
                { "id", "complex-btn" }
            }));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        string? classAttribute = button.GetAttribute("class");

        // All classes should be present in correct order
        classAttribute.Should().NotBeNull();
        classAttribute.Should().Be("test-variant-component test-variant-component--complex user-extra btn-glass");
        button.GetAttribute("id").Should().Be("complex-btn");
    }

    [Fact(DisplayName = "DataAttributes_TemplateCanHavePriority")]
    public void CustomVariant_DataAttributes_TemplateCanHavePriority()
    {
        // Arrange - Template where template attributes have priority
        TestVariant dataVariant = TestVariant.Custom("DataPriority");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(dataVariant, _templates.DataAttributeTemplatePriority);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
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

    [Fact(DisplayName = "DataAttributes_UserOverridesTemplate")]
    public void CustomVariant_DataAttributes_UserOverridesTemplate()
    {
        // Arrange - Template where user attributes have priority
        TestVariant dataVariant = TestVariant.Custom("DataVariant");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(dataVariant, _templates.DataAttributeTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
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

    [Fact(DisplayName = "DataAttributesVariantBuilder_TemplateCanHavePriority")]
    public void CustomVariant_DataAttributesVariantBuilder_TemplateCanHavePriority()
    {
        // Arrange - Template where template attributes have priority
        TestVariant dataVariant = TestVariant.Custom("DataPriority");

        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(dataVariant, _templates.DataAttributeTemplatePriority));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
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

    [Fact(DisplayName = "DataAttributesVariantBuilder_UserOverridesTemplate")]
    public void CustomVariant_DataAttributesVariantBuilder_UserOverridesTemplate()
    {
        // Arrange - Template where user attributes have priority
        TestVariant dataVariant = TestVariant.Custom("DataVariant");

        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(dataVariant, _templates.DataAttributeTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
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

    [Fact(DisplayName = "MergeTemplate_PreservesAllClasses")]
    public void CustomVariant_MergeTemplate_PreservesAllClasses()
    {
        // Arrange - This demonstrates the RECOMMENDED approach
        TestVariant glassVariant = TestVariant.Custom("Glass");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(glassVariant, _templates.AddsOneClassTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Component"));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        // Component classes are preserved
        button.ShouldHaveClass("test-variant-component");
        button.ShouldHaveClass("test-variant-component--glass");
        // Template class is added
        button.ShouldHaveClass("btn-glass");
    }

    [Fact(DisplayName = "MergeTemplateVariantBuilder_PreservesAllClasses")]
    public void CustomVariant_MergeTemplateVariantBuilder_PreservesAllClasses()
    {
        // Arrange - This demonstrates the RECOMMENDED approach
        TestVariant glassVariant = TestVariant.Custom("Glass");

        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(glassVariant, _templates.AddsOneClassTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Component"));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        // Component classes are preserved
        button.ShouldHaveClass("test-variant-component");
        button.ShouldHaveClass("test-variant-component--glass");
        // Template class is added
        button.ShouldHaveClass("btn-glass");
    }

    [Fact(DisplayName = "OverrideTemplate_ReplacesAllClasses")]
    public void CustomVariant_OverrideTemplate_ReplacesAllClasses()
    {
        // Arrange - This demonstrates a template that completely overrides classes
        TestVariant overrideVariant = TestVariant.Custom("Override");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(overrideVariant, _templates.OverrideClassTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, overrideVariant)
            .Add(p => p.Text, "Override Component"));

        // Assert - Only template classes are present
        AngleSharp.Dom.IElement element = cut.Find("button");
        string? classAttribute = element.GetAttribute("class");

        classAttribute.Should().NotBeNull();
        classAttribute.Should().Be("btn-override-only");
        // Component classes are NOT preserved
        element.ShouldNotHaveClass("test-variant-component");
    }

    [Fact(DisplayName = "OverrideTemplateVariantBuilder_ReplacesAllClasses")]
    public void CustomVariant_OverrideTemplateVariantBuilder_ReplacesAllClasses()
    {
        // Arrange - This demonstrates a template that completely overrides classes
        TestVariant overrideVariant = TestVariant.Custom("Override");

        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(overrideVariant, _templates.OverrideClassTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, overrideVariant)
            .Add(p => p.Text, "Override Component"));

        // Assert - Only template classes are present
        AngleSharp.Dom.IElement element = cut.Find("button");
        string? classAttribute = element.GetAttribute("class");

        classAttribute.Should().NotBeNull();
        classAttribute.Should().Be("btn-override-only");
        // Component classes are NOT preserved
        element.ShouldNotHaveClass("test-variant-component");
    }

    [Fact(DisplayName = "TypedVariant_WorksCorrectly")]
    public void CustomVariant_TypedVariant_WorksCorrectly()
    {
        // Arrange
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(MyCustomTestVariants.MyCustom1, _templates.AddsOneClassTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, MyCustomTestVariants.MyCustom1)
            .Add(p => p.Text, "Typed Variant Component"));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        button.ShouldHaveClass("test-variant-component");
        button.ShouldHaveClass("test-variant-component--mycustom1");
        button.ShouldHaveClass("btn-glass");
    }

    [Fact(DisplayName = "TypedVariantVariantBuilder_WorksCorrectly")]
    public void CustomVariant_TypedVariantVariantBuilder_WorksCorrectly()
    {
        // Arrange
        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(MyCustomTestVariants.MyCustom1, _templates.AddsOneClassTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, MyCustomTestVariants.MyCustom1)
            .Add(p => p.Text, "Typed Variant Component"));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        button.ShouldHaveClass("test-variant-component");
        button.ShouldHaveClass("test-variant-component--mycustom1");
        button.ShouldHaveClass("btn-glass");
    }

    [Fact(DisplayName = "UnregisteredVariant_RendersNothing")]
    public void CustomVariant_UnregisteredVariant_RendersNothing()
    {
        // Arrange
        TestVariant unregistered = TestVariant.Custom("Unregistered");

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, unregistered)
            .Add(p => p.Text, "Should not appear"));

        // Assert
        cut.Markup.Should().BeEmpty();
    }

    [Fact(DisplayName = "UserClasses_PreservedInCorrectOrder")]
    public void CustomVariant_UserClasses_PreservedInCorrectOrder()
    {
        // Arrange
        TestVariant glassVariant = TestVariant.Custom("Glass");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(glassVariant, _templates.AddsOneClassTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Component")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-class-1 user-class-2" }
            }));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        string? classAttribute = button.GetAttribute("class");

        classAttribute.Should().NotBeNull();
        // Order: component classes, user classes, template classes
        classAttribute.Should().Be("test-variant-component test-variant-component--glass user-class-1 user-class-2 btn-glass");
    }

    [Fact(DisplayName = "UserClassesVariantBuilder_PreservedInCorrectOrder")]
    public void CustomVariant_UserClassesVariantBuilder_PreservedInCorrectOrder()
    {
        // Arrange
        TestVariant glassVariant = TestVariant.Custom("Glass");

        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(glassVariant, _templates.AddsOneClassTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Component")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-class-1 user-class-2" }
            }));

        // Assert
        AngleSharp.Dom.IElement button = cut.Find("button");
        string? classAttribute = button.GetAttribute("class");

        classAttribute.Should().NotBeNull();
        // Order: component classes, user classes, template classes
        classAttribute.Should().Be("test-variant-component test-variant-component--glass user-class-1 user-class-2 btn-glass");
    }
}