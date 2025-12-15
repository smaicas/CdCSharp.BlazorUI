using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Variants;

public class VariantDiscoveryTests : TestContextBase
{
    [Fact]
    public void Variant_Is_Discovered_Automatically()
    {
        // Arrange
        UIButtonVariant custom = UIButtonVariant.Custom("Custom");

        // Act
        Bunit.IRenderedComponent<UIButton> cut = Render<UIButton>(p => p
            .Add(x => x.Variant, custom)
            .Add(x => x.Text, "Hello"));

        // Assert
        cut.Find("button").TextContent.Should().Be("Hello");
        cut.Find("button").ShouldHaveClass("ui-button");
        cut.Find("button").ShouldHaveClass("ui-button--custom");
    }
}
