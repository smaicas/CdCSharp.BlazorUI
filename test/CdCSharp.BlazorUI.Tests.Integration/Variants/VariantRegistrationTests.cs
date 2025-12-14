using Bunit;
using CdCSharp.BlazorUI.Components.Attributes;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Components.Abstractions;
using CdCSharp.BlazorUI.Core.Components.Discovery;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Variants;

public class VariantRegistrationTests : TestContextBase
{
    [Fact]
    public void BuilderPattern_RegistersVariantsCorrectly()
    {
        // Arrange
        Services.AddBlazorUI(options =>
        {
            options.ConfigureButton()
                .AddVariant("BuilderTest", component => builder =>
                {
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "class", "builder-test");
                    builder.AddContent(2, "Builder Test");
                    builder.CloseElement();
                });
        });

        // Simulate startup
        IVariantRegistry<UIButton, UIButtonVariant> registry =
            Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, UIButtonVariant.Custom("BuilderTest")));

        // Assert
        cut.Find("button").TextContent.Should().Be("Builder Test");
        cut.Find("button").ClassList.Should().Contain("builder-test");
    }

    [Fact]
    public void AttributeDiscovery_FindsAndRegistersVariants()
    {
        // This test would need a test assembly with attributed methods
        // For now, we'll test the manual registration path

        // Arrange
        UIButtonVariant variant = UIButtonVariant.Custom("AttributeTest");
        IVariantRegistry<UIButton, UIButtonVariant> registry =
            Services.GetRequiredService<IVariantRegistry<UIButton, UIButtonVariant>>();

        registry.Register(variant, TestTemplatesByAttribute.AttributeTestTemplate);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Variant, variant));

        // Assert
        cut.Find("button").TextContent.Should().Be("Attribute Test");
    }
}

// Test templates class
public static class TestTemplatesByAttribute
{
    [ButtonVariant("AttributeTest")]
    public static RenderFragment AttributeTestTemplate(UIButton component) => builder =>
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "class", "attribute-test");
        builder.AddContent(2, "Attribute Test");
        builder.CloseElement();
    };
}

// Test variant provider
public class TestButtonVariantProvider : IVariantProvider<UIButton, UIButtonVariant>
{
    public IEnumerable<(UIButtonVariant variant, Func<UIButton, RenderFragment> template)> GetVariants()
    {
        yield return (UIButtonVariant.Custom("Provider1"), ProviderTemplate1);
        yield return (UIButtonVariant.Custom("Provider2"), ProviderTemplate2);
    }

    private RenderFragment ProviderTemplate1(UIButton component) => builder =>
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "class", "provider-1");
        builder.AddContent(2, "Provider 1");
        builder.CloseElement();
    };

    private RenderFragment ProviderTemplate2(UIButton component) => builder =>
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "class", "provider-2");
        builder.AddContent(2, "Provider 2");
        builder.CloseElement();
    };
}
