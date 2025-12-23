using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Components.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Services;

[Trait("Core", "VariantRegistry")]
public class VariantRegistryTests : TestContextBase
{
    [Fact(DisplayName = "Register_StoresTemplate")]
    public void VariantRegistry_Register_StoresTemplate()
    {
        // Arrange
        VariantRegistry<UIButton, UIButtonVariant> registry = new();
        UIButtonVariant variant = UIButtonVariant.Custom("Test");
        Func<UIButton, RenderFragment> template = _ => __builder => { };

        // Act
        registry.Register(variant, template);
        RenderFragment? retrieved = registry.GetTemplate(variant, null!);

        // Assert
        retrieved.Should().NotBeNull();
    }

    [Fact(DisplayName = "GetTemplate_ReturnsNullForUnregistered")]
    public void VariantRegistry_GetTemplate_ReturnsNullForUnregistered()
    {
        // Arrange
        VariantRegistry<UIButton, UIButtonVariant> registry = new();
        UIButtonVariant variant = UIButtonVariant.Custom("Unregistered");

        // Act
        RenderFragment? template = registry.GetTemplate(variant, null!);

        // Assert
        template.Should().BeNull();
    }

    [Fact(DisplayName = "Register_OverwritesExisting")]
    public void VariantRegistry_Register_OverwritesExisting()
    {
        // Arrange
        VariantRegistry<UIButton, UIButtonVariant> registry = new();
        UIButtonVariant variant = UIButtonVariant.Custom("Test");
        bool firstCalled = false;
        bool secondCalled = false;

        // Act
        registry.Register(variant, _ => __builder => { firstCalled = true; });
        registry.Register(variant, _ => __builder => { secondCalled = true; });

        RenderFragment? template = registry.GetTemplate(variant, null!);
        template?.Invoke(null!);

        // Assert
        firstCalled.Should().BeFalse();
        secondCalled.Should().BeTrue();
    }
}
