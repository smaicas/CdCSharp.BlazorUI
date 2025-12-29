using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Generic.Button;

[Trait("Components", "UIButton")]
public class ButtonRenderTests : TestContextBase
{
    [Fact(DisplayName = "DefaultVariant_RendersWithCorrectDataAttributes")]
    public void Button_DefaultVariant_RendersWithCorrectDataAttributes()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Click me"));

        // Assert
        IElement uiComponent = cut.FindByDataComponent("button");
        uiComponent.Should().NotBeNull();
        uiComponent.ShouldHaveDataVariant("default");

        IElement? button = uiComponent.QuerySelector("button");
        button.Should().NotBeNull();
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

    [Fact(DisplayName = "WithCustomColors_AppliesInlineStyles")]
    public void Button_WithCustomColors_AppliesInlineStyles()
    {
        // Arrange
        CssColor backgroundColor = new("#FF0000");
        CssColor textColor = new("#FFFFFF");

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Colored Button")
            .Add(p => p.BackgroundColor, backgroundColor)
            .Add(p => p.Color, textColor));

        // Assert
        IElement button = cut.Find("button");
        string? style = button.GetAttribute("style");
        style.Should().Contain("background-color: rgba(255,0,0,1)");
        style.Should().Contain("color: rgba(255,255,255,1)");
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

    [Fact(DisplayName = "WithNullText_DoesNotRenderTextSpan")]
    public void Button_WithNullText_DoesNotRenderTextSpan()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, null));

        // Assert
        cut.FindAll(".ui-button__text").Should().BeEmpty();
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

    [Fact(DisplayName = "WithBorder_AppliesInlineStyles")]
    public void Button_WithBorder_AppliesInlineStyles()
    {
        // Arrange
        BorderStyle borderStyle = new("2px", BorderStyleType.Solid, UIColor.Palette.Primary);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Bordered Button")
            .Add(p => p.Border, borderStyle));

        // Assert
        IElement button = cut.Find("button");
        string? style = button.GetAttribute("style");
        style.Should().Contain("border: 2px solid var(--palette-primary)");
    }

    [Fact(DisplayName = "WithBorderAndRadius_AppliesCorrectStyles")]
    public void Button_WithBorderAndRadius_AppliesCorrectStyles()
    {
        // Arrange
        BorderStyle borderStyle = new("1px", BorderStyleType.Solid, UIColor.Gray.Default, 8);

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Rounded Button")
            .Add(p => p.Border, borderStyle));

        // Assert
        IElement button = cut.Find("button");
        string? style = button.GetAttribute("style");
        style.Should().Contain("border: 1px solid");
        style.Should().Contain("border-radius: 8px");
    }
}