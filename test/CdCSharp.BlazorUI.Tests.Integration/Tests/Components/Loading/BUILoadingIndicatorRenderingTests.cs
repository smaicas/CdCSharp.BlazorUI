using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Loading;

[Trait("Component Rendering", "BUILoadingIndicator")]
public class BUILoadingIndicatorRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("loading-indicator");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Spinner_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("spinner");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Status_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        // Assert — spinner has role="status"
        cut.Find("bui-component").GetAttribute("role").Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        // Assert
        cut.Find("bui-component").GetAttribute("aria-label").Should().Be("Loading");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Svg_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        // Assert
        cut.Find("svg").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Size, BUISize.Large));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Linear_Variant_With_Progressbar_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Variant, BUILoadingIndicatorVariant.LinearIndeterminate));

        // Assert — linear has role="progressbar" instead of "status"
        cut.Find("bui-component").GetAttribute("role").Should().Be("progressbar");
    }
}
