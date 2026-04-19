using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "BUIInputLoading")]
public class BUIInputLoadingRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Loading_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputLoading> cut = ctx.Render<BUIInputLoading>(p => p
            .Add(c => c.Loading, false));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Addon_With_Loading_Indicator_When_Loading_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputLoading> cut = ctx.Render<BUIInputLoading>(p => p
            .Add(c => c.Loading, true));

        // Assert
        IElement addon = cut.Find("._bui-addon");
        addon.Should().NotBeNull();
        cut.FindComponents<BUILoadingIndicator>().Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_LoadingIndicatorVariant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputLoading> cut = ctx.Render<BUIInputLoading>(p => p
            .Add(c => c.Loading, true)
            .Add(c => c.LoadingIndicatorVariant, BUILoadingIndicatorVariant.Dots));

        // Assert
        IRenderedComponent<BUILoadingIndicator> indicator = cut.FindComponent<BUILoadingIndicator>();
        indicator.Instance.Variant.Name.Should().Be("Dots");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_Size_To_Indicator(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputLoading> cut = ctx.Render<BUIInputLoading>(p => p
            .Add(c => c.Loading, true)
            .Add(c => c.Size, SizeEnum.Large));

        // Assert
        IRenderedComponent<BUILoadingIndicator> indicator = cut.FindComponent<BUILoadingIndicator>();
        indicator.Instance.Size.Should().Be(SizeEnum.Large);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Re_Render_When_Loading_Toggled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputLoading> cut = ctx.Render<BUIInputLoading>(p => p
            .Add(c => c.Loading, false));
        cut.Markup.Trim().Should().BeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.Loading, true));

        // Assert
        cut.FindAll("._bui-addon").Should().HaveCount(1);

        // Act
        cut.Render(p => p.Add(c => c.Loading, false));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }
}
