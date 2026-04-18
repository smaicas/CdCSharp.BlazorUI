using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Validation", "BUIThemeGenerator")]
public class BUIThemeGeneratorValidationTests
{
    private static void OpenImportDialog(IRenderedComponent<BUIThemeGenerator> cut)
    {
        cut.Find(".bui-theme-generator__actions-group button").Click();
    }

    private static void ClickImportButton(IRenderedComponent<BUIThemeGenerator> cut)
    {
        var btn = cut.FindAll("[role='dialog'] button")
            .FirstOrDefault(b => b.TextContent.Trim() == "Import");
        btn?.Click();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_For_Empty_Import(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();
        OpenImportDialog(cut);

        // Act — click Import without pasting anything
        ClickImportButton(cut);

        // Assert
        cut.Find(".bui-theme-generator__import-error").TextContent
            .Should().Contain("Please paste JSON content");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Error_For_Invalid_JSON(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();
        OpenImportDialog(cut);

        // Act — change textarea value (onchange event triggers @bind-Value update) and click Import
        cut.Find("textarea").Change("not valid json {{");
        ClickImportButton(cut);

        // Assert
        cut.Find(".bui-theme-generator__import-error").TextContent
            .Should().Contain("Invalid JSON");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Import_Dialog_On_Valid_JSON(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();
        OpenImportDialog(cut);

        // Act
        string validJson = "{\"dark\": {\"primary\": \"#ff0000\"}, \"light\": {\"primary\": \"#0000ff\"}}";
        cut.Find("textarea").Change(validJson);
        ClickImportButton(cut);

        // Assert — no import error shown after successful import
        cut.FindAll(".bui-theme-generator__import-error").Should().BeEmpty();
    }
}
