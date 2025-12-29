using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Generic.Button;

[Trait("Components", "UIButton")]
public class ButtonVariantTests : TestContextBase
{
    [Fact(DisplayName = "DefaultVariant_RendersCorrectly")]
    public void Button_DefaultVariant_RendersCorrectly()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Default Button"));

        // Assert
        IElement uiComponent = cut.FindByDataComponent("button");
        uiComponent.Should().NotBeNull();
        uiComponent.ShouldHaveDataVariant("default");
    }

    [Fact(DisplayName = "DefaultVariantWithIcons_RendersCorrectStructure")]
    public void Button_DefaultVariantWithIcons_RendersCorrectStructure()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Button")
            .Add(p => p.LeadingIcon, "<path />")
            .Add(p => p.TrailingIcon, "<path />"));

        // Assert
        IElement uiComponent = cut.FindByDataComponent("button");
        IElement? button = uiComponent.QuerySelector("button");

        button.Should().NotBeNull();
        button.Children.Should().HaveCount(3);
        button.Children[0].ShouldHaveTagName("svg"); // Leading icon
        button.Children[1].ShouldHaveTagName("span"); // Text
        button.Children[1].ShouldHaveClass("ui-button__text"); // This class still exists
        button.Children[2].ShouldHaveTagName("svg"); // Trailing icon
    }
}