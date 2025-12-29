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

    [Fact(DisplayName = "ComplexTemplate_MergesMultipleDataAttributes")]
    public void CustomVariant_ComplexTemplate_MergesMultipleDataAttributes()
    {
        // Arrange
        TestVariant complexVariant = TestVariant.Custom("Complex");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(complexVariant, _templates.AddsDataAttributeTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, complexVariant)
            .Add(p => p.Text, "Complex Component")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
            { "data-user", "extra" },
            { "id", "complex-btn" }
            }));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");
        AngleSharp.Dom.IElement button = uiComponent.QuerySelector("button");

        // Verify ui-component wrapper attributes
        uiComponent.ShouldHaveDataAttribute("ui-component", "test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-variant", "complex");
        uiComponent.ShouldHaveDataAttribute("custom-modifier", "glass"); // From AddsDataAttributeTemplate
        uiComponent.ShouldHaveDataAttribute("user", "extra"); // From AdditionalAttributes

        // Verify button still has its attributes
        button.GetAttribute("id").Should().Be("complex-btn");
    }

    [Fact(DisplayName = "ComplexTemplateVariantBuilder_MergesMultipleDataAttributes")]
    public void CustomVariant_ComplexTemplateVariantBuilder_MergesMultipleDataAttributes()
    {
        // Arrange
        TestVariant complexVariant = TestVariant.Custom("Complex");
        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(complexVariant, _templates.AddsDataAttributeTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, complexVariant)
            .Add(p => p.Text, "Complex Component")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
            { "data-user", "extra" },
            { "id", "complex-btn" }
            }));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");

        // Verify all data attributes are merged correctly
        uiComponent.ShouldHaveDataAttribute("ui-component", "test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-variant", "complex");
        uiComponent.ShouldHaveDataAttribute("custom-modifier", "glass"); // From template
        uiComponent.ShouldHaveDataAttribute("user", "extra"); // From AdditionalAttributes
        uiComponent.GetAttribute("id").Should().Be("complex-btn");
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

    [Fact(DisplayName = "MergeTemplate_PreservesAllDataAttributes")]
    public void CustomVariant_MergeTemplate_PreservesAllDataAttributes()
    {
        // Arrange - This demonstrates the RECOMMENDED approach
        TestVariant glassVariant = TestVariant.Custom("Glass");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(glassVariant, _templates.AddsDataAttributeTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Component"));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");

        // Component data attributes are preserved
        uiComponent.ShouldHaveDataAttribute("ui-component", "test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-variant", "glass");
        // Template data attribute is added
        uiComponent.ShouldHaveDataAttribute("custom-modifier", "glass");
    }

    [Fact(DisplayName = "MergeTemplateVariantBuilder_PreservesAllDataAttributes")]
    public void CustomVariant_MergeTemplateVariantBuilder_PreservesAllDataAttributes()
    {
        // Arrange - This demonstrates the RECOMMENDED approach
        TestVariant glassVariant = TestVariant.Custom("Glass");
        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(glassVariant, _templates.AddsDataAttributeTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Component"));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");

        // Component data attributes are preserved
        uiComponent.ShouldHaveDataAttribute("ui-component", "test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-variant", "glass");
        // Template data attribute is added
        uiComponent.ShouldHaveDataAttribute("custom-modifier", "glass");
    }

    [Fact(DisplayName = "OverrideTemplate_ReplacesDataAttributes")]
    public void CustomVariant_OverrideTemplate_ReplacesDataAttributes()
    {
        // Arrange - This demonstrates a template that overrides specific attributes
        TestVariant overrideVariant = TestVariant.Custom("Override");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(overrideVariant, _templates.OverrideDataAttributeTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, overrideVariant)
            .Add(p => p.Text, "Override Component"));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");

        // Template overrides the variant
        uiComponent.ShouldHaveDataAttribute("ui-variant", "override"); // Not "override" from variant name
        uiComponent.ShouldHaveDataAttribute("ui-state", "custom");
    }

    [Fact(DisplayName = "OverrideTemplateVariantBuilder_ReplacesDataAttributes")]
    public void CustomVariant_OverrideTemplateVariantBuilder_ReplacesDataAttributes()
    {
        // Arrange - This demonstrates a template that overrides specific attributes
        TestVariant overrideVariant = TestVariant.Custom("Override");
        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(overrideVariant, _templates.OverrideDataAttributeTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, overrideVariant)
            .Add(p => p.Text, "Override Component"));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");

        // Template overrides the variant attribute
        uiComponent.ShouldHaveDataAttribute("ui-variant", "override"); // Not "Override" from variant name
        uiComponent.ShouldHaveDataAttribute("ui-state", "custom");
    }

    [Fact(DisplayName = "TypedVariant_WorksCorrectly")]
    public void CustomVariant_TypedVariant_WorksCorrectly()
    {
        // Arrange
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(MyCustomTestVariants.MyCustom1, _templates.AddsDataAttributeTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, MyCustomTestVariants.MyCustom1)
            .Add(p => p.Text, "Typed Variant Component"));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-component", "test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-variant", "mycustom1");
        uiComponent.ShouldHaveDataAttribute("custom-modifier", "glass");
    }

    [Fact(DisplayName = "TypedVariantVariantBuilder_WorksCorrectly")]
    public void CustomVariant_TypedVariantVariantBuilder_WorksCorrectly()
    {
        // Arrange
        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(MyCustomTestVariants.MyCustom1, _templates.AddsDataAttributeTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, MyCustomTestVariants.MyCustom1)
            .Add(p => p.Text, "Typed Variant Component"));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-component", "test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-variant", "mycustom1");
        uiComponent.ShouldHaveDataAttribute("custom-modifier", "glass");
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

    [Fact(DisplayName = "UserDataAttributes_PreservedWithCorrectPriority")]
    public void CustomVariant_UserDataAttributes_PreservedWithCorrectPriority()
    {
        // Arrange
        TestVariant glassVariant = TestVariant.Custom("Glass");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(glassVariant, _templates.AddsDataAttributeTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Component")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
            { "data-user", "value1" },
            { "data-test", "value2" }
            }));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");

        // All data attributes should be present
        uiComponent.ShouldHaveDataAttribute("ui-component", "test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-variant", "glass");
        uiComponent.ShouldHaveDataAttribute("custom-modifier", "glass"); // From template
        uiComponent.ShouldHaveDataAttribute("user", "value1"); // From user
        uiComponent.ShouldHaveDataAttribute("test", "value2"); // From user
    }

    [Fact(DisplayName = "UserDataAttributesVariantBuilder_PreservedWithCorrectPriority")]
    public void CustomVariant_UserDataAttributesVariantBuilder_PreservedWithCorrectPriority()
    {
        // Arrange
        TestVariant glassVariant = TestVariant.Custom("Glass");
        Services.AddBlazorUIVariants(builder => builder.For<TestVariantComponent, TestVariant>()
            .Register(glassVariant, _templates.AddsDataAttributeTemplate));

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, glassVariant)
            .Add(p => p.Text, "Glass Component")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
            { "data-user-1", "value1" },
            { "data-user-2", "value2" }
            }));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");

        // Verify all data attributes are present
        uiComponent.ShouldHaveDataAttribute("ui-component", "test-variant");
        uiComponent.ShouldHaveDataAttribute("ui-variant", "glass");
        uiComponent.ShouldHaveDataAttribute("custom-modifier", "glass"); // From template
        uiComponent.ShouldHaveDataAttribute("user-1", "value1"); // From user
        uiComponent.ShouldHaveDataAttribute("user-2", "value2"); // From user
    }

    [Fact(DisplayName = "DataAttributePriority_TemplateOverridesUser")]
    public void CustomVariant_DataAttributePriority_TemplateOverridesUser()
    {
        // Arrange
        TestVariant priorityVariant = TestVariant.Custom("Priority");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();
        registry.Register(priorityVariant, _templates.DataAttributeTemplatePriority);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, priorityVariant)
            .Add(p => p.Text, "Priority Test")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
            { "data-test", "user-value" } // This should be overridden
            }));

        // Assert
        AngleSharp.Dom.IElement uiComponent = cut.FindByDataComponent("test-variant");

        // Template value should win
        uiComponent.ShouldHaveDataAttribute("test", "template-value-priority");
    }
}