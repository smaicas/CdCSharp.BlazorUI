using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Button;

[Trait("Component Rendering", "BUIButton")]
public class BUIButtonRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Test Button")
            .Add(c => c.Size, SizeEnum.Large)
            .Add(c => c.BackgroundColor, PaletteColor.Background)
            .Add(c => c.Color, PaletteColor.BackgroundContrast)

            .Add(c => c.Shadow, BUIShadowPresets.Elevation(4)));

        // Assert
        cut.Find("bui-component").Should().NotBeNull();
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("button");
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
        cut.Find("bui-component").GetAttribute("data-bui-shadow").Should().Be("true");
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-shadow:");
    }
}