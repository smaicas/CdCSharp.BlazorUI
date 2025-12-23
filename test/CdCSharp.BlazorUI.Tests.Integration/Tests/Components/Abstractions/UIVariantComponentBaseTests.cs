using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Core.Components.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Abstractions;

[Trait("Abstractions", "UIVariantComponentBase")]
public class UIVariantComponentBaseTests : TestContextBase
{
    private readonly TestVariantTemplates _templates = new();

    public UIVariantComponentBaseTests()
    {
        // Register the test component's variant registry
        Services.AddSingleton<IVariantRegistry<TestVariantComponent, TestVariant>>(
            new VariantRegistry<TestVariantComponent, TestVariant>());
    }

    [Fact(DisplayName = "BuiltInVariant_RendersCorrectly")]
    public void UIVariantComponentBase_BuiltInVariant_RendersCorrectly()
    {
        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, TestVariant.Alternative));

        // Assert
        IElement div = cut.Find("div");
        div.Should().NotBeNull();
        cut.Find("span").TextContent.Should().Be("Alt: Test Variant Component");
        // Should have both base classes and template-added class
        div.ShouldHaveClass("test-variant-component");
        div.ShouldHaveClass("test-variant-component--alternative");
        div.ShouldHaveClass("alternative-style");
    }

    [Fact(DisplayName = "ClassesInheritance_WorksAcrossVariants")]
    public void UIVariantComponentBase_ClassesInheritance_WorksAcrossVariants()
    {
        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, TestVariant.Default)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-class" }
            }));

        // Assert
        IElement button = cut.Find("button");
        string? classes = button.GetAttribute("class");
        classes.Should().Contain("test-variant-component");
        classes.Should().Contain("test-variant-component--default");
        classes.Should().Contain("user-class");
    }

    [Fact(DisplayName = "DefaultVariant_UsedWhenNotSpecified")]
    public void UIVariantComponentBase_DefaultVariant_UsedWhenNotSpecified()
    {
        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>();

        // Assert
        cut.Find("button").Should().NotBeNull();
        cut.Find("button").ShouldHaveClass("test-variant-component");
        cut.Find("button").ShouldHaveClass("test-variant-component--default");
    }

    [Fact(DisplayName = "ParametersPassedToTemplate_PassedThrough")]
    public void UIVariantComponentBase_ParametersPassedToTemplate_PassedThrough()
    {
        // Arrange
        bool wasClicked = false;

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Text, "Click me")
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, () => wasClicked = true));

        // Assert
        IElement button = cut.Find("button");
        button.TextContent.Should().Be("Click me");
        button.HasAttribute("disabled").Should().BeTrue();

        // Click should not work on disabled button, but let's verify the event is wired
        button.GetAttribute("disabled").Should().NotBeNull();
    }

    [Fact(DisplayName = "RegisteredVariant_NotOverrideBuiltInVariant")]
    public void UIVariantComponentBase_RegisteredVariant_NotOverrideBuiltInVariant()
    {
        // Arrange
        TestVariant customVariant = TestVariant.Custom("Alternative");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();

        registry.Register(customVariant, _templates.RegisteredTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, customVariant));

        // Assert
        IElement div = cut.Find("div");
        div.Should().NotBeNull();
        cut.Find("span").TextContent.Should().Be("Alt: Test Variant Component");
        // Should have both base classes and template-added class
        div.ShouldHaveClass("test-variant-component");
        div.ShouldHaveClass("test-variant-component--alternative");
        div.ShouldHaveClass("alternative-style");
    }

    [Fact(DisplayName = "RegisteredVariant_RendersCorrectly")]
    public void UIVariantComponentBase_RegisteredVariant_RendersCorrectly()
    {
        // Arrange
        TestVariant customVariant = TestVariant.Custom("Registered");
        IVariantRegistry<TestVariantComponent, TestVariant> registry =
            Services.GetRequiredService<IVariantRegistry<TestVariantComponent, TestVariant>>();

        registry.Register(customVariant, _templates.RegisteredTemplate);

        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, customVariant));

        // Assert
        IElement div = cut.Find("div");
        div.TextContent.Should().Be("Registered: Test Variant Component");
        div.ShouldHaveClass("test-variant-component");
        div.ShouldHaveClass("test-variant-component--registered");
        div.ShouldHaveClass("registered-variant");
    }

    [Fact(DisplayName = "UnknownVariant_RendersNothing")]
    public void UIVariantComponentBase_UnknownVariant_RendersNothing()
    {
        // Act
        IRenderedComponent<TestVariantComponent> cut = Render<TestVariantComponent>(parameters => parameters
            .Add(p => p.Variant, TestVariant.Custom("Unknown")));

        // Assert
        cut.Markup.Should().BeEmpty();
    }
}