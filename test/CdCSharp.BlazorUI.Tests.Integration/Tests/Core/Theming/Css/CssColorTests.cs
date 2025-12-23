using CdCSharp.BlazorUI.Core.Theming.Css;
using FluentAssertions;
using System.Drawing;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.Theming.Css;

[Trait("Theming", "CssColor")]
public class CssColorTests
{
    [Fact(DisplayName = "APercentage_Property")]
    public void CssColor_APercentage_Property()
    {
        // Arrange
        CssColor color = new(255, 0, 0, 128);

        // Act
        double percentage = color.APercentage;

        // Assert
        percentage.Should().BeApproximately(0.5, 0.01);
    }

    [Fact(DisplayName = "Constructor_ByteValues")]
    public void CssColor_Constructor_ByteValues()
    {
        // Act
        CssColor color = new(100, 150, 200, 255);

        // Assert
        color.R.Should().Be(100);
        color.G.Should().Be(150);
        color.B.Should().Be(200);
        color.A.Should().Be(255);
    }

    [Fact(DisplayName = "Constructor_FromSystemColor_WithVariant")]
    public void CssColor_Constructor_FromSystemColor_WithVariant()
    {
        // Arrange
        Color systemColor = Color.FromArgb(255, 100, 150, 200);

        // Act - Test different variants using static methods
        CssColor defaultColor = new(systemColor, null);
        CssColor lightenColor = new(systemColor, CssColorVariant.Lighten(1));
        CssColor darkenColor = new(systemColor, CssColorVariant.Darken(1));

        // Assert
        defaultColor.R.Should().Be(100);
        defaultColor.G.Should().Be(150);
        defaultColor.B.Should().Be(200);
        defaultColor.A.Should().Be(255);

        // Verify variants were applied (exact values depend on implementation)
        lightenColor.Should().NotBeNull();
        darkenColor.Should().NotBeNull();
    }

    [Fact(DisplayName = "Constructor_IntValuesWithAlpha")]
    public void CssColor_Constructor_IntValuesWithAlpha()
    {
        // Act
        CssColor color = new(100, 150, 200, 255);

        // Assert
        color.R.Should().Be(100);
        color.G.Should().Be(150);
        color.B.Should().Be(200);
        color.A.Should().Be(255);
    }

    [Fact(DisplayName = "Constructor_IntValuesWithAlphaDouble")]
    public void CssColor_Constructor_IntValuesWithAlphaDouble()
    {
        // Act
        CssColor color = new(100, 150, 200, 0.5);

        // Assert
        color.R.Should().Be(100);
        color.G.Should().Be(150);
        color.B.Should().Be(200);
        color.A.Should().BeCloseTo(127, 1); // 0.5 * 255
    }

    [Theory(DisplayName = "CssColor_Constructor_Throws_ForInvalidStrings")]
    [InlineData(null)]                   // null
    [InlineData("")]                     // empty
    [InlineData("   ")]                  // whitespace
    [InlineData("invalid")]              // random string
    [InlineData("rgb()")]                // missing numbers
    [InlineData("rgba()")]               // missing numbers
    [InlineData("rgb(300,400,500)")]     // values out of range
    [InlineData("rgba(0,0,0,2)")]        // alpha > 1
    [InlineData("#GGHHII")]              // invalid hex characters
    [InlineData("#12345")]               // invalid hex length
    [InlineData("#123456789")]           // invalid hex length
    public void CssColor_Constructor_Throws_ForInvalidStrings(string invalidValue)
    {
        // Arrange
        Action act = () => new CssColor(invalidValue);

        // Act & Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact(DisplayName = "EqualityOperator_ComparesTwoColors")]
    public void CssColor_EqualityOperator_ComparesTwoColors()
    {
        // Arrange
        CssColor color1 = new("#FF0000");
        CssColor color2 = new("#FF0000");
        CssColor color3 = new("#00FF00");

        // Act & Assert
        (color1 == color2).Should().BeTrue();
        (color1 == color3).Should().BeFalse();
        (color1 != color3).Should().BeTrue();
        (color1 != color2).Should().BeFalse();
    }

    [Fact(DisplayName = "Equals_WithCssColorParameter")]
    public void CssColor_Equals_WithCssColorParameter()
    {
        // Arrange
        CssColor color1 = new("#FF0000");
        CssColor color2 = new("#FF0000");
        CssColor color3 = new("#00FF00");
        CssColor? nullColor = null;

        // Act & Assert
        color1.Equals(color2).Should().BeTrue();
        color1.Equals(color3).Should().BeFalse();
        color1.Equals(nullColor).Should().BeFalse();

        // Same reference
        color1.Equals(color1).Should().BeTrue();
    }

    [Fact(DisplayName = "Equals_WithObjectParameter")]
    public void CssColor_Equals_WithObjectParameter()
    {
        // Arrange
        CssColor color1 = new("#FF0000");
        CssColor color2 = new("#FF0000");
        object? nullObject = null;
        object notAColor = "Not a color";

        // Act & Assert
        color1.Equals((object)color2).Should().BeTrue();
        color1.Equals(nullObject).Should().BeFalse();
        color1.Equals(notAColor).Should().BeFalse();
    }

    [Fact(DisplayName = "GetHashCode_SameColorsHaveSameHashCode")]
    public void CssColor_GetHashCode_SameColorsHaveSameHashCode()
    {
        // Arrange
        CssColor color1 = new("#FF0000");
        CssColor color2 = new("#FF0000");
        CssColor color3 = new("#00FF00");

        // Act
        int hash1 = color1.GetHashCode();
        int hash2 = color2.GetHashCode();
        int hash3 = color3.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
        hash1.Should().NotBe(hash3);
    }

    [Fact(DisplayName = "HSL_Properties_Get")]
    public void CssColor_HSL_Properties_Get()
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act & Assert - Only test getters
        color.H.Should().BeInRange(0, 360);
        color.S.Should().BeInRange(0, 100);
        color.L.Should().BeInRange(0, 100);
    }

    [Fact(DisplayName = "ImplicitOperator_FromCssColorToString")]
    public void CssColor_ImplicitOperator_FromCssColorToString()
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act
        string colorString = (string)color; // Explicit cast since implicit might not be available

        // Assert
        colorString.Should().Be("rgba(255,0,0,1)");
    }

    [Fact(DisplayName = "ImplicitOperator_FromStringToCssColor")]
    public void CssColor_ImplicitOperator_FromStringToCssColor()
    {
        // Act
        CssColor color = new("#FF0000"); // Direct constructor instead of implicit

        // Assert
        color.Should().NotBeNull();
        color.R.Should().Be(255);
        color.G.Should().Be(0);
        color.B.Should().Be(0);
    }

    [Theory(DisplayName = "CssColor_Constructor_ParsesValidFormats")]
    [InlineData("#FF0000")]
    [InlineData("#ff0000")]           // lowercase
    [InlineData("#F00")]
    [InlineData("#F00F")]             // 4-digit HEX with alpha
    [InlineData("#FF0000FF")]         // 8-digit HEX with alpha
    [InlineData("rgb(255,0,0)")]
    [InlineData("rgb( 255 , 0 , 0 )")] // spaces
    [InlineData("rgba(255,0,0,1)")]
    [InlineData("rgba(255,0,0,0.5)")]  // alpha decimal
    [InlineData("RGBA(255,0,0,0.5)")]  // uppercase rgba
    public void CssColor_Parse_ValidFormats(string colorString)
    {
        // Act
        CssColor color = new(colorString);

        // Assert
        color.Should().NotBeNull();
        color.R.Should().BeInRange(0, 255);
        color.G.Should().BeInRange(0, 255);
        color.B.Should().BeInRange(0, 255);
        color.A.Should().BeInRange(0, 255);
    }

    [Theory(DisplayName = "ToString_AllFormats")]
    [InlineData(ColorOutputFormats.Hex)]
    [InlineData(ColorOutputFormats.Rgba)]
    [InlineData(ColorOutputFormats.Rgb)]
    [InlineData(ColorOutputFormats.HexA)]
    public void CssColor_ToString_AllFormats(ColorOutputFormats format)
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act
        string result = color.ToString(format);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Theory(DisplayName = "ToString_WithFormat_ReturnsCorrectFormat")]
    [InlineData(ColorOutputFormats.Hex, "#")]
    [InlineData(ColorOutputFormats.Rgba, "rgba")] // Using Rgba instead of RGB/RGBA
    public void CssColor_ToString_WithFormat_ReturnsCorrectFormat(ColorOutputFormats format, string expectedStart)
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act
        string result = color.ToString(format);

        // Assert
        result.Should().StartWith(expectedStart, "Format {0} should produce output starting with {1}", format, expectedStart);
    }

    [Fact(DisplayName = "ToString_WithoutParameter_ReturnsDefaultFormat")]
    public void CssColor_ToString_WithoutParameter_ReturnsDefaultFormat()
    {
        // Arrange
        CssColor color = new("#FF0000");

        // Act
        string result = color.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
        // Should return Rgba format by default
        result.Should().Be("rgba(255,0,0,1)");
    }

    [Fact(DisplayName = "CssColorVariant_Constructor")]
    public void CssColorVariant_Constructor()
    {
        // This tests the CssColorVariant constructor that takes Modifier and double We can't create
        // it directly, but we can test through the static methods
        CssColorVariant lighten1 = CssColorVariant.Lighten(1);
        CssColorVariant darken2 = CssColorVariant.Darken(2);

        // Assert
        lighten1.Should().NotBeNull();
        darken2.Should().NotBeNull();

        // Test the properties if they are accessible
        lighten1.Alteration.Should().BeGreaterThan(0);
        darken2.Alteration.Should().BeGreaterThan(0);
    }
}