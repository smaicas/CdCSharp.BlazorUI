using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Library;

[Trait("Library", "CssColorSystem")]
public class CssColorSystemTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task BUIColor_Should_Provide_Color_Variants(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Assert base colors exist
        BUIColor.Red.Default.Should().NotBeNull();
        BUIColor.Red.Darken1.Should().NotBeNull();
        BUIColor.Red.Lighten1.Should().NotBeNull();

        // Assert palette colors
        BUIColor.Palette.Primary.Should().NotBeNull();
        BUIColor.Palette.Background.Should().NotBeNull();

        // Assert color output formats
        CssColor color = BUIColor.Blue.Default;
        string rgba = color.ToString(ColorOutputFormats.Rgba);
        rgba.Should().StartWith("rgba(");
    }
}
