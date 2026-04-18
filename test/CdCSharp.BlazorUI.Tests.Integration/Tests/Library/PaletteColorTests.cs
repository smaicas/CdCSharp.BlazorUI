using CdCSharp.BlazorUI.Components;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Library;

/// <summary>
/// LIB-04: Each PaletteColor static property resolves to var(--palette-*).
/// Verifies the implicit string conversion and ToString() emit valid CSS variable references.
/// </summary>
[Trait("Library", "PaletteColor")]
public class PaletteColorTests
{
    [Theory]
    [InlineData("Background",         "--palette-background")]
    [InlineData("BackgroundContrast", "--palette-backgroundcontrast")]
    [InlineData("Error",              "--palette-error")]
    [InlineData("ErrorContrast",      "--palette-errorcontrast")]
    [InlineData("Info",               "--palette-info")]
    [InlineData("InfoContrast",       "--palette-infocontrast")]
    [InlineData("Primary",            "--palette-primary")]
    [InlineData("PrimaryContrast",    "--palette-primarycontrast")]
    [InlineData("Secondary",          "--palette-secondary")]
    [InlineData("SecondaryContrast",  "--palette-secondarycontrast")]
    [InlineData("Success",            "--palette-success")]
    [InlineData("SuccessContrast",    "--palette-successcontrast")]
    [InlineData("Surface",            "--palette-surface")]
    [InlineData("SurfaceContrast",    "--palette-surfacecontrast")]
    [InlineData("Warning",            "--palette-warning")]
    [InlineData("WarningContrast",    "--palette-warningcontrast")]
    [InlineData("Black",              "--palette-black")]
    [InlineData("Border",             "--palette-border")]
    [InlineData("Highlight",          "--palette-highlight")]
    [InlineData("Shadow",             "--palette-shadow")]
    [InlineData("White",              "--palette-white")]
    public void PaletteColor_ImplicitString_Should_Produce_Var_Reference(
        string propertyName, string expectedVariable)
    {
        // Arrange
        PaletteColor color = (PaletteColor)typeof(PaletteColor)
            .GetProperty(propertyName)!
            .GetValue(null)!;

        // Act
        string css = color;

        // Assert
        css.Should().Be($"var({expectedVariable})");
    }

    [Fact]
    public void PaletteColor_ToString_Should_Match_ImplicitConversion()
    {
        // Arrange
        PaletteColor color = PaletteColor.Primary;

        // Assert
        color.ToString().Should().Be((string)color);
    }

    [Fact]
    public void PaletteColor_All_Properties_Should_Start_With_VarPalette()
    {
        // Arrange — collect all static PaletteColor properties
        IEnumerable<string> values = typeof(PaletteColor)
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(PaletteColor))
            .Select(p => (string)(PaletteColor)p.GetValue(null)!);

        // Assert
        foreach (string css in values)
            css.Should().StartWith("var(--palette-",
                because: $"PaletteColor '{css}' must reference a --palette-* CSS variable");
    }
}
