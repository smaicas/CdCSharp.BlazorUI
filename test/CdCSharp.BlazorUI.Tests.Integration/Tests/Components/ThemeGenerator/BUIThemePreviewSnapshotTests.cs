using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeGenerator;

[Trait("Component Snapshots", "BUIThemePreview")]
public class BUIThemePreviewSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Section_Structure_Snapshot(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIThemePreview> cut = ctx.Render<BUIThemePreview>();

        // Snapshot the section headings + preview structure only — the full markup is
        // too large and brittle. This protects the demo surface from silent breakage
        // while leaving room for internal component evolution.
        string[] headings = cut.FindAll(".bui-theme-preview__section > h5")
                               .Select(h => h.TextContent.Trim())
                               .ToArray();

        int sections = cut.FindAll(".bui-theme-preview__section").Count;
        int rows = cut.FindAll(".bui-theme-preview__row").Count;

        await Verify(new { headings, sections, rows }).UseParameters(scenario.Name);
    }
}
