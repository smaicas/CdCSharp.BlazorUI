using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Core.Css;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Common;

[Trait("Features", "BorderPresets")]
public class BorderPresetsTests
{
    [Fact(DisplayName = "AllPresets_HaveValidValues")]
    public void BorderPresets_AllPresets_HaveValidValues()
    {
        // Assert all presets are not null and have valid values
        BorderPresets.Default.Should().NotBeNull();
        BorderPresets.Subtle.Should().NotBeNull();
        BorderPresets.Strong.Should().NotBeNull();
        BorderPresets.Primary.Should().NotBeNull();
        BorderPresets.Secondary.Should().NotBeNull();
        BorderPresets.Error.Should().NotBeNull();
        BorderPresets.Warning.Should().NotBeNull();
        BorderPresets.Success.Should().NotBeNull();
        BorderPresets.Info.Should().NotBeNull();
        BorderPresets.Dashed.Should().NotBeNull();
        BorderPresets.Dotted.Should().NotBeNull();
        BorderPresets.Double.Should().NotBeNull();
        BorderPresets.Rounded.Should().NotBeNull();
        BorderPresets.RoundedLarge.Should().NotBeNull();
        BorderPresets.Pill.Should().NotBeNull();
    }

    [Fact(DisplayName = "ThemePresets_UseThemeColors")]
    public void BorderPresets_ThemePresets_UseThemeColors()
    {
        // Primary should use theme color
        BorderPresets.Primary.Color.ToString(ColorOutputFormats.Rgba)
            .Should().Be("var(--palette-primary)");

        // Secondary should use theme color
        BorderPresets.Secondary.Color.ToString(ColorOutputFormats.Rgba)
            .Should().Be("var(--palette-secondary)");
    }

    [Theory(DisplayName = "RoundedPresets_HaveCorrectRadius")]
    [InlineData("Rounded", 4)]
    [InlineData("RoundedLarge", 8)]
    [InlineData("Pill", 9999)]
    public void BorderPresets_RoundedPresets_HaveCorrectRadius(string presetName, int expectedRadius)
    {
        // Arrange
        BorderStyle preset = presetName switch
        {
            "Rounded" => BorderPresets.Rounded,
            "RoundedLarge" => BorderPresets.RoundedLarge,
            "Pill" => BorderPresets.Pill,
            _ => throw new ArgumentException()
        };

        // Assert
        preset.Radius.Should().Be(expectedRadius);
    }
}
