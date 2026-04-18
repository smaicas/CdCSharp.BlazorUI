using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Rendering", "BUIThemeEditor")]
public class BUIThemeEditorRenderingTests
{
    private static Dictionary<string, CssColor> CreatePalette() => new()
    {
        ["Primary"] = new CssColor("#1A73E8"),
        ["PrimaryContrast"] = new CssColor("#FFFFFF"),
        ["Background"] = new CssColor("#121212"),
        ["BackgroundContrast"] = new CssColor("#FFFFFF"),
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_Editor_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert
        cut.Find(".bui-theme-editor").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Category_Sections(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — at least one category rendered
        cut.FindAll(".bui-theme-editor__category").Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Category_Titles(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — category title elements rendered
        cut.FindAll(".bui-theme-editor__category-title").Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Input_For_Each_Palette_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — color inputs present for palette keys that match categories
        cut.FindAll("bui-component[data-bui-component='input-color'], input[type='color']")
            .Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Color_Components(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, CreatePalette()));

        // Assert — BUIInputColor components rendered for matching palette keys
        cut.FindAll("bui-component[data-bui-component='input-color']")
            .Should().HaveCountGreaterThan(0);
    }
}
