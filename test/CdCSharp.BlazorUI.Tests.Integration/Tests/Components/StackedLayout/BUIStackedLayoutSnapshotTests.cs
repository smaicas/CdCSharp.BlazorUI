using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using VerifyXunit;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.StackedLayout;

[Trait("Component Snapshots", "BUIStackedLayout")]
public class BUIStackedLayoutSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_StackedLayout_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, Action<ComponentParameterCollectionBuilder<BUIStackedLayout>> Builder)[] testCases =
        [
            ("NoNav", p => p
                .Add(c => c.Header, b => b.AddContent(0, "Header"))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Content"))),
            ("WithNav_Closed", p => p
                .Add(c => c.Header, b => b.AddContent(0, "Header"))
                .Add(c => c.Nav, b => b.AddContent(0, "Nav"))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Content"))
                .Add(c => c.NavOpen, false)),
            ("WithNav_Open", p => p
                .Add(c => c.Header, b => b.AddContent(0, "Header"))
                .Add(c => c.Nav, b => b.AddContent(0, "Nav"))
                .Add(c => c.ChildContent, b => b.AddContent(0, "Content"))
                .Add(c => c.NavOpen, true)),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BUIStackedLayout> cut = ctx.Render<BUIStackedLayout>(tc.Builder);
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
