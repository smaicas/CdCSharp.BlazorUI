using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Common;

[Trait("Features", "BorderStyle")]
public class BorderStyleTests
{
    [Fact(DisplayName = "Constructor_SetsAllProperties")]
    public void BorderStyle_Constructor_SetsAllProperties()
    {
        // Arrange & Act
        BorderStyle style = new("2px", BorderStyleType.Solid, UIColor.Blue.Default, 8);

        // Assert
        style.Width.Should().Be("2px");
        style.Style.Should().Be(BorderStyleType.Solid);
        style.Color.Should().Be(UIColor.Blue.Default);
        style.Radius.Should().Be(8);
    }

    [Fact(DisplayName = "ToCssValue_ReturnsCorrectFormat")]
    public void BorderStyle_ToCssValue_ReturnsCorrectFormat()
    {
        // Arrange
        BorderStyle style = new("1px", BorderStyleType.Dashed, new CssColor("#FF0000"));

        // Act
        string cssValue = style.ToCssValue();

        // Assert
        cssValue.Should().Be("1px dashed rgba(255,0,0,1)");
    }

    [Fact(DisplayName = "WithRadius_CreatesNewInstanceWithRadius")]
    public void BorderStyle_WithRadius_CreatesNewInstanceWithRadius()
    {
        // Arrange
        BorderStyle original = new("2px", BorderStyleType.Solid, UIColor.Gray.Default);

        // Act
        BorderStyle withRadius = original.WithRadius(16);

        // Assert
        withRadius.Should().NotBeSameAs(original);
        withRadius.Radius.Should().Be(16);
        withRadius.Width.Should().Be(original.Width);
        withRadius.Style.Should().Be(original.Style);
        withRadius.Color.Should().Be(original.Color);
        original.Radius.Should().BeNull();
    }

    [Theory(DisplayName = "GetRadiusCssValue_ReturnsCorrectValue")]
    [InlineData(null, "")]
    [InlineData(0, "0px")]
    [InlineData(8, "8px")]
    [InlineData(999, "999px")]
    public void BorderStyle_GetRadiusCssValue_ReturnsCorrectValue(int? radius, string expected)
    {
        // Arrange
        BorderStyle style = new("1px", BorderStyleType.Solid, UIColor.Black.Default, radius);

        // Act
        string radiusCss = style.GetRadiusCssValue();

        // Assert
        radiusCss.Should().Be(expected);
    }
}
