using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Core.Theming.Abstractions;
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

        // Surface colors
        palette.Background.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#0F172A");

        palette.BackgroundContrast.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#F1F5F9");

        palette.Surface.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#1E293B");

        palette.SurfaceContrast.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#F1F5F9");

        // Main colors
        palette.Primary.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#60A5FA");

        palette.PrimaryContrast.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#0F172A");

        palette.Secondary.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#A78BFA");

        palette.SecondaryContrast.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#0F172A");

        // Status colors
        palette.Success.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#10B981");

        palette.SuccessContrast.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#0F172A");

        palette.Warning.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#F59E0B");

        palette.WarningContrast.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#0F172A");

        palette.Error.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#EF4444");

        palette.ErrorContrast.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#0F172A");

        palette.Info.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#3B82F6");

        palette.InfoContrast.ToString(ColorOutputFormats.Hex)
            .Should().BeEquivalentTo("#0F172A");
    }

    [Fact(DisplayName = "Properties_CanBeSetAndRetrieved")]
    public void UIThemePaletteBase_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        TestThemePalette palette = new();
        CssColor testColor = new("#123456");

        // Act & Assert
        palette.Id = "custom-id";
        palette.Id.Should().Be("custom-id");

        palette.Name = "Custom Name";
        palette.Name.Should().Be("Custom Name");

        palette.Background = testColor;
        palette.Background.Should().Be(testColor);

        palette.BackgroundContrast = testColor;
        palette.BackgroundContrast.Should().Be(testColor);

        palette.Surface = testColor;
        palette.Surface.Should().Be(testColor);

        palette.SurfaceContrast = testColor;
        palette.SurfaceContrast.Should().Be(testColor);

        palette.Primary = testColor;
        palette.Primary.Should().Be(testColor);

        palette.PrimaryContrast = testColor;
        palette.PrimaryContrast.Should().Be(testColor);

        palette.Secondary = testColor;
        palette.Secondary.Should().Be(testColor);

        palette.SecondaryContrast = testColor;
        palette.SecondaryContrast.Should().Be(testColor);

        palette.Success = testColor;
        palette.Success.Should().Be(testColor);

        palette.SuccessContrast = testColor;
        palette.SuccessContrast.Should().Be(testColor);

        palette.Warning = testColor;
        palette.Warning.Should().Be(testColor);

        palette.WarningContrast = testColor;
        palette.WarningContrast.Should().Be(testColor);

        palette.Error = testColor;
        palette.Error.Should().Be(testColor);

        palette.ErrorContrast = testColor;
        palette.ErrorContrast.Should().Be(testColor);

        palette.Info = testColor;
        palette.Info.Should().Be(testColor);

        palette.InfoContrast = testColor;
        palette.InfoContrast.Should().Be(testColor);
    }

    // Concrete implementation for testing
    private class TestThemePalette : UIThemePaletteBase
    {
    }
}