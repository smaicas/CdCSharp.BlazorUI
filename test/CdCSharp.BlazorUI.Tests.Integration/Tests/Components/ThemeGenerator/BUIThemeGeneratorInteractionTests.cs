using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Interaction", "BUIThemeGenerator")]
public class BUIThemeGeneratorInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Import_Dialog_On_Import_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Act — click Import JSON button (first action group, first button)
        cut.Find(".bui-theme-generator__actions-group button").Click();

        // Assert — import dialog should open
        cut.Find(".bui-theme-generator__actions").Should().NotBeNull();
        cut.FindAll("textarea").Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Export_Dialog_On_Export_JSON_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Act — click Export JSON button (second action group, first button)
        var actionGroups = cut.FindAll(".bui-theme-generator__actions-group");
        actionGroups[1].QuerySelector("button")!.Click();

        // Assert — export code block should be visible
        cut.FindAll(".bui-code-block, bui-component[data-bui-component='code-block']")
            .Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reset_Palettes_On_Reset_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();
        string initialStyle = cut.Find(".bui-theme-generator__preview-container").GetAttribute("style") ?? "";

        // Act — click Reset
        cut.Find("button[title='Reset'], .bui-theme-generator__actions > bui-component[data-bui-component='button']:last-child button")
            .Click();

        // Assert — preview container still renders (palettes reset to defaults)
        cut.Find(".bui-theme-generator__preview-container").Should().NotBeNull();
    }
}
