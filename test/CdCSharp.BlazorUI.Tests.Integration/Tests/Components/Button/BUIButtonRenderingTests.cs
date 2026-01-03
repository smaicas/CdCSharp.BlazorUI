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
            .Add(c => c.Elevation, 4));

        // Assert
        cut.Find("bui-component").Should().NotBeNull();
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("button");
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
        cut.Find("bui-component").GetAttribute("data-bui-elevation").Should().Be("4");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Loading_State_Correctly(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Loading Button")
            .Add(c => c.IsLoading, true)
            .Add(c => c.LoadingIndicatorVariant, BUILoadingIndicatorVariant.Spinner));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-loading").Should().Be("true");
        cut.Find("button").GetAttribute("disabled").Should().NotBeNull();
        cut.FindComponent<BUILoadingIndicator>().Should().NotBeNull();
    }
}