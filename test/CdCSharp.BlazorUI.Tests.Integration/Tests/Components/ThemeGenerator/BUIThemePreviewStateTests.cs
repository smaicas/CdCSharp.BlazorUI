using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component State", "BUIThemePreview")]
public class BUIThemePreviewStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dialog_When_Show_Dialog_Button_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();
        cut.FindAll("[role='dialog']").Should().BeEmpty();

        // Act
        cut.FindAll("button")
           .First(b => b.TextContent.Trim() == "Show Dialog")
           .Click();

        // Assert
        cut.FindAll("[role='dialog']").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Open_Drawer_When_Show_Drawer_Button_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();
        cut.FindAll(".bui-drawer-host, .bui-drawer-overlay, .bui-drawer").Should().BeEmpty();

        // Act
        cut.FindAll("button")
           .First(b => b.TextContent.Trim() == "Show Drawer")
           .Click();

        // Assert — drawer host materialized
        cut.FindAll(".bui-drawer-host, .bui-drawer-overlay, .bui-drawer").Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Text_Value_Across_Inputs(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — the two "Text Input" + "With Helper" fields share `_textValue`
        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();
        IReadOnlyList<IElement> textInputs =
            cut.FindAll("bui-component[data-bui-component='input-text'] input.bui-input__field");
        textInputs.Should().HaveCountGreaterThan(1);

        // Act
        textInputs[0].Change("shared");

        // Assert — second input (bound to the same backing field) now reflects the same value
        IReadOnlyList<IElement> after =
            cut.FindAll("bui-component[data-bui-component='input-text'] input.bui-input__field");
        after[1].GetAttribute("value").Should().Be("shared");
    }
}
