using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Rendering", "BUIThemePreview")]
public class BUIThemePreviewRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_Preview_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();

        // Assert
        cut.Find(".bui-theme-preview").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Preview_Sections(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();

        // Assert — multiple sections rendered (Buttons, Inputs, Selection, Card, etc.)
        cut.FindAll(".bui-theme-preview__section").Should().HaveCountGreaterThan(4);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Buttons_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();

        // Assert — buttons exist in preview
        cut.FindAll("bui-component[data-bui-component='button']").Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Card_Section(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();

        // Assert
        cut.FindAll("bui-component[data-bui-component='card']").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Preview_Rows(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();

        // Assert
        cut.FindAll(".bui-theme-preview__row").Should().HaveCountGreaterThan(0);
    }
}
