using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Snapshots", "BUIDialog")]
public class BUIDialogSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Dialog_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BUIDialog>> Builder)[] testCases =
        [
            ("Closed", p => p
                .Add(c => c.Open, false)),
            ("Open_NoContent", p => p
                .Add(c => c.Open, true)),
            ("Open_WithTitle", p => p
                .Add(c => c.Open, true)
                .Add(c => c.Title, "Dialog Title")
                .Add(c => c.Content, b => b.AddContent(0, "Body text"))),
            ("Open_WithFooter", p => p
                .Add(c => c.Open, true)
                .Add(c => c.Content, b => b.AddContent(0, "Body"))
                .Add(c => c.Footer, b => b.AddContent(0, "OK | Cancel"))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BUIDialog> cut = ctx.Render<BUIDialog>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Drawer_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BUIDrawer>> Builder)[] testCases =
        [
            ("Closed", p => p
                .Add(c => c.Open, false)),
            ("Open_Right", p => p
                .Add(c => c.Open, true)
                .Add(c => c.Position, DrawerPosition.Right)
                .Add(c => c.ChildContent, b => b.AddContent(0, "Drawer content"))),
            ("Open_Left", p => p
                .Add(c => c.Open, true)
                .Add(c => c.Position, DrawerPosition.Left)
                .Add(c => c.ChildContent, b => b.AddContent(0, "Left drawer"))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BUIDrawer> cut = ctx.Render<BUIDrawer>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
