using CdCSharp.BlazorUI.Core.Theming.Abstractions;
using CdCSharp.BlazorUI.Core.Theming.Css;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.Theming.Abstractions;

[Trait("Theming", "UIThemePaletteBase")]
public class UIThemePaletteBaseTests
{
    [Fact(DisplayName = "Constructor_SetsDefaultValues")]
    public void UIThemePaletteBase_Constructor_SetsDefaultValues()
    {
        // Act
        TestThemePalette palette = new();

        // Assert
        palette.Id.Should().Be("default");
        palette.Name.Should().Be("Default");

        // Main colors
        palette.Primary.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#3B82F6");
        palette.PrimaryContrast.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#FFFFFF");
        palette.Secondary.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#8B5CF6");
        palette.SecondaryContrast.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#FFFFFF");

        // Surface colors
        palette.Background.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#FFFFFF");
        palette.Surface.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#F8FAFC");
        palette.Foreground.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#1E293B");

        // Status colors
        palette.Error.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#EF4444");
        palette.Success.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#10B981");
        palette.Warning.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#F59E0B");
        palette.Info.ToString(ColorOutputFormats.Hex).Should().BeEquivalentTo("#3B82F6");
    }

    [Fact(DisplayName = "Properties_CanBeSetAndRetrieved")]
    public void UIThemePaletteBase_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        TestThemePalette palette = new();
        CssColor testColor = new("#123456");

        // Act & Assert - Test all properties
        palette.Id = "custom-id";
        palette.Id.Should().Be("custom-id");

        palette.Name = "Custom Name";
        palette.Name.Should().Be("Custom Name");

        palette.Primary = testColor;
        palette.Primary.Should().Be(testColor);

        palette.PrimaryContrast = testColor;
        palette.PrimaryContrast.Should().Be(testColor);

        palette.Secondary = testColor;
        palette.Secondary.Should().Be(testColor);

        palette.SecondaryContrast = testColor;
        palette.SecondaryContrast.Should().Be(testColor);

        palette.Background = testColor;
        palette.Background.Should().Be(testColor);

        palette.Surface = testColor;
        palette.Surface.Should().Be(testColor);

        palette.Foreground = testColor;
        palette.Foreground.Should().Be(testColor);

        palette.Error = testColor;
        palette.Error.Should().Be(testColor);

        palette.Success = testColor;
        palette.Success.Should().Be(testColor);

        palette.Warning = testColor;
        palette.Warning.Should().Be(testColor);

        palette.Info = testColor;
        palette.Info.Should().Be(testColor);
    }

    // Concrete implementation for testing
    private class TestThemePalette : UIThemePaletteBase
    {
    }
}