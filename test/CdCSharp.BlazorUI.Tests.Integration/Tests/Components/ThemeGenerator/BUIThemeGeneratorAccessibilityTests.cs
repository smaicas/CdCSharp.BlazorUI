using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Accessibility", "BUIThemeGenerator")]
public class BUIThemeGeneratorAccessibilityTests
{
    private static void OpenImportDialog(IRenderedComponent<BUIThemeGenerator> cut) =>
        cut.Find(".bui-theme-generator__actions-group button").Click();

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Label_Every_Color_Input_In_Editor(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Assert — each color input field has a <label> with a non-empty name inside its outline-notch
        IReadOnlyList<IElement> labels =
            cut.FindAll("bui-component[data-bui-component='input-color'] .bui-input__label");
        labels.Should().NotBeEmpty();
        foreach (IElement label in labels)
        {
            label.TextContent.Trim().Should().NotBeNullOrEmpty();
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Import_Dialog_With_Dialog_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Act
        OpenImportDialog(cut);

        // Assert
        cut.FindAll("[role='dialog']").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Action_Buttons_Expose_Accessible_Text(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();

        // Assert — every actions button has either a visible text node or aria-label
        IReadOnlyList<IElement> buttons =
            cut.FindAll(".bui-theme-generator__actions button");
        buttons.Should().NotBeEmpty();
        foreach (IElement btn in buttons)
        {
            bool hasText = !string.IsNullOrWhiteSpace(btn.TextContent);
            bool hasAria = btn.HasAttribute("aria-label");
            (hasText || hasAria).Should().BeTrue(
                "every actionable button must expose a name to assistive tech");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Surface_Import_Error_In_Discoverable_Container(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeGenerator> cut = ctx.Render<BUIThemeGenerator>();
        OpenImportDialog(cut);

        // Act — trigger error (empty content)
        cut.FindAll("[role='dialog'] button")
           .First(b => b.TextContent.Trim() == "Import")
           .Click();

        // Assert — error rendered inside the dialog so it stays in the reading order
        IElement error = cut.Find(".bui-theme-generator__import-error");
        error.Should().NotBeNull();
        error.TextContent.Should().NotBeNullOrWhiteSpace();
        cut.Find("[role='dialog']").Contains(error).Should().BeTrue();
    }
}
