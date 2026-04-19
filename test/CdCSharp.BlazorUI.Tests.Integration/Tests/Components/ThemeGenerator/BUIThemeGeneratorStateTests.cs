using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component State", "BUIThemeGenerator")]
public class BUIThemeGeneratorStateTests
{
    private static void OpenImportDialog(IRenderedComponent<BUIThemeGenerator> cut) =>
        cut.Find(".bui-theme-generator__actions-group button").Click();

    private static void ClickImportButton(IRenderedComponent<BUIThemeGenerator> cut) =>
        cut.FindAll("[role='dialog'] button")
           .First(b => b.TextContent.Trim() == "Import")
           .Click();

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Palette_Vars_In_Preview_Style(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Assert — preview container carries palette CSS vars from active (dark) theme
        string style = cut.Find(".bui-theme-generator__preview-container").GetAttribute("style") ?? "";
        style.Should().Contain("--palette-primary:");
        style.Should().Contain("--palette-background:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Roundtrip_Import_Then_Export_To_Same_Color(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();
        OpenImportDialog(cut);
        cut.Find("textarea").Change("{\"dark\":{\"primary\":\"#aabbcc\"}}");
        ClickImportButton(cut);

        // Act — open Export JSON (second action group → first button)
        IReadOnlyList<IElement> actionGroups = cut.FindAll(".bui-theme-generator__actions-group");
        actionGroups[1].QuerySelector("button")!.Click();

        // Assert — imported value appears in exported JSON
        string exported = cut.Find("[role='dialog'] .bui-code-block__content").TextContent;
        exported.ToLowerInvariant().Should().Contain("#aabbcc");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Report_Partial_Parse_Failures_Inline(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();
        OpenImportDialog(cut);
        cut.Find("textarea").Change("{\"dark\":{\"primary\":\"not-a-color\"}}");

        // Act
        ClickImportButton(cut);

        // Assert — dialog stays open, error container surfaces failure per key
        string error = cut.Find(".bui-theme-generator__import-error").TextContent;
        error.Should().Contain("dark.primary");
        error.Should().Contain("not-a-color");
        cut.FindAll("[role='dialog']").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Restore_Default_Palette_On_Reset(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — mutate via import
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();
        string beforeStyle = cut.Find(".bui-theme-generator__preview-container").GetAttribute("style") ?? "";

        OpenImportDialog(cut);
        cut.Find("textarea").Change("{\"dark\":{\"primary\":\"#aabbcc\"}}");
        ClickImportButton(cut);

        string mutatedStyle = cut.Find(".bui-theme-generator__preview-container").GetAttribute("style") ?? "";
        mutatedStyle.Should().Contain("#aabbcc");

        // Act — click Reset (last button in actions row)
        cut.FindAll(".bui-theme-generator__actions > bui-component[data-bui-component='button'] button")
           .Last()
           .Click();

        // Assert — preview style returns to initial defaults (no longer contains mutated color)
        string afterStyle = cut.Find(".bui-theme-generator__preview-container").GetAttribute("style") ?? "";
        afterStyle.Should().NotContain("#aabbcc");
        afterStyle.Should().Be(beforeStyle);
    }
}
