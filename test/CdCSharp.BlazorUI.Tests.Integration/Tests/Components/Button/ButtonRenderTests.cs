using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Button;

[Trait("Components", "UIButton")]
public class ButtonRenderTests : TestContextBase
{
    [Fact(DisplayName = "DefaultVariant_RendersWithCorrectCssClasses")]
    public void Button_DefaultVariant_RendersWithCorrectCssClasses()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Test Button"));

        // Assert
        IElement button = cut.Find("button");
        button.ClassList
            .Should().Contain("ui-button")
            .And.Contain("ui-button--default");
    }

    [Fact(DisplayName = "WithText_RendersTextContent")]
    public void Button_WithText_RendersTextContent()
    {
        // Arrange
        const string expectedText = "Click me";

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, expectedText));

        // Assert
        cut.Find("button .ui-button__text").TextContent.Should().Be(expectedText);
    }

    [Fact(DisplayName = "WithLeadingIcon_RendersIconBeforeText")]
    public void Button_WithLeadingIcon_RendersIconBeforeText()
    {
        // Arrange
        const string iconPath = "<path d=\"M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z\"/>";

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Home")
            .Add(p => p.LeadingIcon, iconPath));

        // Assert
        IElement button = cut.Find("button");
        IElement firstChild = button.Children[0];
        firstChild.ShouldHaveTagName("svg");

        IElement? textSpan = button.QuerySelector(".ui-button__text");
        textSpan.Should().NotBeNull();
        textSpan.TextContent.Should().Be("Home");
    }

    [Fact(DisplayName = "WithTrailingIcon_RendersIconAfterText")]
    public void Button_WithTrailingIcon_RendersIconAfterText()
    {
        // Arrange
        const string iconPath = "<path d=\"M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z\"/>";

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Next")
            .Add(p => p.TrailingIcon, iconPath));

        // Assert
        IElement button = cut.Find("button");
        IElement lastChild = button.Children[button.Children.Length - 1];
        lastChild.ShouldHaveTagName("svg");
    }

    [Fact(DisplayName = "IconOnly_HasSpecificCssClass")]
    public void Button_IconOnly_HasSpecificCssClass()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.LeadingIcon, "<path d=\"M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z\"/>"));

        // Assert
        cut.Find("button").ClassList
            .Should().Contain("ui-button--icon-only");
    }

    [Theory(DisplayName = "DisabledState_RendersCorrectly")]
    [InlineData(true)]
    [InlineData(false)]
    public void Button_DisabledState_RendersCorrectly(bool disabled)
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Test")
            .Add(p => p.Disabled, disabled));

        // Assert
        IElement button = cut.Find("button");
        button.HasAttribute("disabled").Should().Be(disabled);
    }

    [Fact(DisplayName = "WithAdditionalAttributes_MergesCorrectly")]
    public void Button_WithAdditionalAttributes_MergesCorrectly()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Test")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "data-test-id", "my-button" },
                { "class", "custom-class" }
            }));

        // Assert
        IElement button = cut.Find("button");
        button.GetAttribute("data-test-id").Should().Be("my-button");
        button.ClassList
            .Should().Contain("ui-button")
            .And.Contain("ui-button--default")
            .And.Contain("custom-class");
    }
}