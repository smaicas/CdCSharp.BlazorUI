using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using VerifyXunit;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.SidebarLayout;

[Trait("Component Snapshots", "BUISidebarLayout")]
public class BUISidebarLayoutSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_SidebarLayout_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BUISidebarLayout>> Builder)[] testCases =
        [
            ("Closed_Start", p => p
                .Add(c => c.SidebarOpen, false)
                .Add(c => c.SidebarSide, SidebarSide.Start)
                .Add(c => c.Header, b => b.AddContent(0, "Header"))
                .Add(c => c.Sidebar, b => b.AddContent(0, "Nav"))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Content"))),
            ("Open_Start", p => p
                .Add(c => c.SidebarOpen, true)
                .Add(c => c.Header, b => b.AddContent(0, "Header"))
                .Add(c => c.Sidebar, b => b.AddContent(0, "Nav"))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Content"))),
            ("Closed_End", p => p
                .Add(c => c.SidebarOpen, false)
                .Add(c => c.SidebarSide, SidebarSide.End)
                .Add(c => c.ChildContent, b => b.AddContent(0, "Content"))),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BUISidebarLayout> cut = ctx.Render<BUISidebarLayout>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
