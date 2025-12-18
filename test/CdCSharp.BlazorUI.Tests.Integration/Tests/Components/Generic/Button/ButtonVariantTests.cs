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
        AngleSharp.Dom.IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-button");
        button.ShouldHaveClass("ui-button--default");
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
        AngleSharp.Dom.IElement button = cut.Find("button");
        button.Children.Should().HaveCount(3);
        button.Children[0].ShouldHaveTagName("svg"); // Leading icon
        button.Children[1].ShouldHaveTagName("span"); // Text
        button.Children[2].ShouldHaveTagName("svg"); // Trailing icon
    }
}