using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Snapshots", "BUIThemeEditor")]
public class BUIThemeEditorSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Status_Palette_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Error"] = new("#B00020"),
                ["ErrorContrast"] = new("#FFFFFF"),
                ["Success"] = new("#2E7D32"),
                ["SuccessContrast"] = new("#FFFFFF"),
            }));

        await Verify(cut.GetNormalizedMarkup()).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Empty_Palette_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>()));

        await Verify(cut.GetNormalizedMarkup()).UseParameters(scenario.Name);
    }
}
