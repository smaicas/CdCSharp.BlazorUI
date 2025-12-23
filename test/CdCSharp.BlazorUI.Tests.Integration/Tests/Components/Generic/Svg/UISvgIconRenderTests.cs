using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Core.Theming.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Generic.Svg;

[Trait("Components", "UISvgIcon")]
public class UISvgIconRenderTests : TestContextBase
{
    // Use '< path > </ path >' instead of '< path />' because AngleSharp normalizes '< path />' to
    // '< path > </ path >'
    private const string TestIcon = "<path d=\"M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z\"></path>";

    [Fact(DisplayName = "CustomViewBox_AppliesCorrectly")]
    public void SvgIcon_CustomViewBox_AppliesCorrectly()
    {
        // Arrange
        const string customViewBox = "0 0 48 48";

        // Act
        IRenderedComponent<UISvgIcon> cut = Render<UISvgIcon>(parameters => parameters
            .Add(p => p.Icon, TestIcon)
            .Add(p => p.ViewBox, customViewBox));

        // Assert
        cut.Find("svg").GetAttribute("viewBox").Should().Be(customViewBox);
    }

    [Fact(DisplayName = "RequiredIcon_RendersCorrectly")]
    public void SvgIcon_RequiredIcon_RendersCorrectly()
    {
        // Act
        IRenderedComponent<UISvgIcon> cut = Render<UISvgIcon>(parameters => parameters
            .Add(p => p.Icon, TestIcon));

        // Assert
        IElement svg = cut.Find("svg");
        svg.Should().NotBeNull();
        svg.InnerHtml.Should().Be(TestIcon);
        svg.GetAttribute("viewBox").Should().Be("0 0 24 24");
        svg.GetAttribute("aria-hidden").Should().Be("true");
        svg.GetAttribute("focusable").Should().Be("false");
    }

    [Theory(DisplayName = "Sizes_ApplyCorrectClasses")]
    [InlineData(UISvgIcon.UISvgIconSize.Small, "ui-svg-icon-s")]
    [InlineData(UISvgIcon.UISvgIconSize.Medium, "ui-svg-icon-m")]
    [InlineData(UISvgIcon.UISvgIconSize.Large, "ui-svg-icon-l")]
    [InlineData(UISvgIcon.UISvgIconSize.XLarge, "ui-svg-icon-xl")]
    [InlineData(UISvgIcon.UISvgIconSize.XXLarge, "ui-svg-icon-xxl")]
    public void SvgIcon_Sizes_ApplyCorrectClasses(UISvgIcon.UISvgIconSize size, string expectedClass)
    {
        // Act
        IRenderedComponent<UISvgIcon> cut = Render<UISvgIcon>(parameters => parameters
            .Add(p => p.Icon, TestIcon)
            .Add(p => p.Size, size));

        // Assert
        IElement svg = cut.Find("svg");
        svg.ShouldHaveClass("ui-svg-icon");
        svg.ShouldHaveClass(expectedClass);
    }

    [Fact(DisplayName = "WithColor_AppliesInlineStyle")]
    public void SvgIcon_WithColor_AppliesInlineStyle()
    {
        // Arrange
        CssColor color = new("#FF5733");

        // Act
        IRenderedComponent<UISvgIcon> cut = Render<UISvgIcon>(parameters => parameters
            .Add(p => p.Icon, TestIcon)
            .Add(p => p.Color, color));

        // Assert
        IElement svg = cut.Find("svg");
        string? style = svg.GetAttribute("style");
        style.Should().Contain("color: rgba(255,87,51,1)");
    }

    [Fact(DisplayName = "WithTitle_RendersTitleElement")]
    public void SvgIcon_WithTitle_RendersTitleElement()
    {
        // Arrange
        const string title = "Home Icon";

        // Act
        IRenderedComponent<UISvgIcon> cut = Render<UISvgIcon>(parameters => parameters
            .Add(p => p.Icon, TestIcon)
            .Add(p => p.Title, title));

        // Assert
        IElement titleElement = cut.Find("svg title");
        titleElement.TextContent.Should().Be(title);
    }
}