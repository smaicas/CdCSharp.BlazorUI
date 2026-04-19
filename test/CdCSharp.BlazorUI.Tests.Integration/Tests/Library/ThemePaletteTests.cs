using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Themes;
using CdCSharp.BlazorUI.Core.Theming.Abstractions;
using FluentAssertions;
using System.Reflection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Library;

/// <summary>
/// Palette invariants for shipped themes.
/// Pins that every CssColor property is populated and parseable, and that the primary
/// contrast pairs meet the minimum WCAG AA contrast ratio for large text (≥ 3:1).
/// </summary>
[Trait("Core", "ThemePalettes")]
public class ThemePaletteTests
{
    public static IEnumerable<object[]> AllThemes => new[]
    {
        new object[] { new LightTheme() },
        new object[] { new DarkTheme() }
    };

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void All_CssColor_Properties_Should_Be_Non_Null(BUIThemePaletteBase theme)
    {
        PropertyInfo[] colorProps = theme.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .ToArray();

        colorProps.Should().NotBeEmpty();

        foreach (PropertyInfo prop in colorProps)
        {
            CssColor? color = (CssColor?)prop.GetValue(theme);
            color.Should().NotBeNull($"{prop.Name} must be populated");
        }
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void All_CssColor_Properties_Should_Emit_Valid_Rgba(BUIThemePaletteBase theme)
    {
        PropertyInfo[] colorProps = theme.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(CssColor))
            .ToArray();

        foreach (PropertyInfo prop in colorProps)
        {
            CssColor color = (CssColor)prop.GetValue(theme)!;
            string rendered = color.ToString(ColorOutputFormats.Rgba);
            rendered.Should().StartWith("rgba(");
            rendered.Should().EndWith(")");
        }
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Id_And_Name_Should_Be_Populated(BUIThemePaletteBase theme)
    {
        theme.Id.Should().NotBeNullOrWhiteSpace();
        theme.Name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void LightTheme_Should_Declare_Light_Id()
    {
        LightTheme theme = new();
        theme.Id.Should().Be("light");
        theme.Name.Should().Be("Light");
    }

    [Fact]
    public void DarkTheme_Should_Declare_Dark_Id()
    {
        DarkTheme theme = new();
        theme.Id.Should().Be("dark");
        theme.Name.Should().Be("Dark");
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void GetPaletteMapping_Should_Emit_Palette_Vars_For_All_Colors(BUIThemePaletteBase theme)
    {
        Dictionary<string, string> mapping = theme.GetPaletteMapping();

        mapping.Should().NotBeEmpty();
        mapping.Keys.Should().OnlyContain(k => k.StartsWith("--palette-"));
        mapping.Values.Should().OnlyContain(v => v.StartsWith($"var(--{theme.Id}-"));
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void GetThemeVariables_Should_Emit_Resolved_Color_Values(BUIThemePaletteBase theme)
    {
        Dictionary<string, string> vars = theme.GetThemeVariables();

        vars.Should().NotBeEmpty();
        vars.Keys.Should().OnlyContain(k => k.StartsWith($"--{theme.Id}-"));
        vars.Values.Should().OnlyContain(v => !string.IsNullOrWhiteSpace(v));
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Palette_Mapping_And_Theme_Variables_Should_Cover_Same_Properties(BUIThemePaletteBase theme)
    {
        Dictionary<string, string> mapping = theme.GetPaletteMapping();
        Dictionary<string, string> vars = theme.GetThemeVariables();

        mapping.Count.Should().Be(vars.Count);
    }

    // ─────────── Contrast pairs (WCAG) ───────────

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Background_Pair_Should_Meet_WCAG_Large_Text_Contrast(BUIThemePaletteBase theme)
    {
        double ratio = Contrast(theme.Background, theme.BackgroundContrast);

        ratio.Should().BeGreaterThanOrEqualTo(3.0, "Background ↔ BackgroundContrast must be legible");
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Surface_Pair_Should_Meet_WCAG_Large_Text_Contrast(BUIThemePaletteBase theme)
    {
        double ratio = Contrast(theme.Surface, theme.SurfaceContrast);

        ratio.Should().BeGreaterThanOrEqualTo(3.0, "Surface ↔ SurfaceContrast must be legible");
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Primary_Pair_Should_Meet_WCAG_Large_Text_Contrast(BUIThemePaletteBase theme)
    {
        double ratio = Contrast(theme.Primary, theme.PrimaryContrast);

        ratio.Should().BeGreaterThanOrEqualTo(3.0, "Primary ↔ PrimaryContrast must be legible");
    }

    [Theory]
    [MemberData(nameof(AllThemes))]
    public void Semantic_Pairs_Should_Meet_WCAG_Large_Text_Contrast(BUIThemePaletteBase theme)
    {
        Contrast(theme.Error, theme.ErrorContrast).Should().BeGreaterThanOrEqualTo(3.0);
        Contrast(theme.Success, theme.SuccessContrast).Should().BeGreaterThanOrEqualTo(3.0);
        Contrast(theme.Warning, theme.WarningContrast).Should().BeGreaterThanOrEqualTo(3.0);
        Contrast(theme.Info, theme.InfoContrast).Should().BeGreaterThanOrEqualTo(3.0);
        Contrast(theme.Secondary, theme.SecondaryContrast).Should().BeGreaterThanOrEqualTo(3.0);
    }

    private static double Contrast(CssColor a, CssColor b)
    {
        double la = a.GetRelativeLuminance();
        double lb = b.GetRelativeLuminance();
        double light = Math.Max(la, lb);
        double dark = Math.Min(la, lb);
        return (light + 0.05) / (dark + 0.05);
    }
}
