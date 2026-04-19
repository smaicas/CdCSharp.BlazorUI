using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component State", "BUIThemeEditor")]
public class BUIThemeEditorStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_New_Palette_Values_After_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Primary"] = new("#111111"),
            }));

        cut.Find("bui-component[data-bui-component='input-color'] input.bui-input__field")
           .GetAttribute("value").Should().Be("#111111");

        // Act — replace palette with a different value
        cut.Render(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Primary"] = new("#222222"),
            }));

        // Assert
        cut.Find("bui-component[data-bui-component='input-color'] input.bui-input__field")
           .GetAttribute("value").Should().Be("#222222");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Only_Inputs_For_Palette_Keys_Matching_Categories(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — palette has 2 recognised keys ("Primary", "Error") + one unknown ("Foo")
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Primary"] = new("#111111"),
                ["Error"] = new("#ff0000"),
                ["Foo"] = new("#00ff00"),
            }));

        // Assert — editor only renders inputs for keys that appear in one of its categories
        cut.FindAll("bui-component[data-bui-component='input-color']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_All_Category_Sections_Regardless_Of_Palette_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — empty palette
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>()));

        // Assert — category skeleton is always rendered (Surface, Main, Status, Utility)
        cut.FindAll(".bui-theme-editor__category").Should().HaveCount(4);
        cut.FindAll("bui-component[data-bui-component='input-color']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Normalize_Null_Palette_To_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — setter guards null with `value ?? new()`
        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, null!));

        // Assert — no inputs, but skeleton still rendered
        cut.Find(".bui-theme-editor").Should().NotBeNull();
        cut.FindAll("bui-component[data-bui-component='input-color']").Should().BeEmpty();
    }
}
