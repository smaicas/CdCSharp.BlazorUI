using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Rendering", "BUIThemeGenerator")]
public class BUIThemeGeneratorRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Assert
        cut.Find(".bui-theme-generator").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Actions_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Assert
        cut.Find(".bui-theme-generator__actions").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Preview_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Assert
        cut.Find(".bui-theme-generator__preview-container").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Editor_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Assert
        cut.Find(".bui-theme-generator__editor").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Export_Action_Group(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Assert
        cut.FindAll(".bui-theme-generator__actions-group").Should().HaveCount(2);
    }
}
