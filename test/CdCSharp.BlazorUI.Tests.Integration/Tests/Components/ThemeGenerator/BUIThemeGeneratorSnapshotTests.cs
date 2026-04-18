using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using VerifyXunit;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Snapshots", "BUIThemeGenerator")]
public class BUIThemeGeneratorSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_ThemeEditor_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIThemeEditor> cut = ctx.Render<BUIThemeEditor>(p => p
            .Add(c => c.Palette, new Dictionary<string, CssColor>
            {
                ["Primary"] = new("#1A73E8"),
                ["PrimaryContrast"] = new("#FFFFFF"),
                ["Background"] = new("#121212"),
                ["BackgroundContrast"] = new("#FFFFFF"),
            }));

        await Verifier.Verify(cut.GetNormalizedMarkup()).UseParameters(scenario.Name);
    }
}
