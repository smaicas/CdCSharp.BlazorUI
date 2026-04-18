using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using VerifyXunit;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Toast;

[Trait("Component Snapshots", "BUIToast")]
public class BUIToastSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Toast_Snapshots(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        (string Name, ToastState State)[] testCases =
        [
            ("Default_Closable", new ToastState
            {
                Content = b => b.AddContent(0, "Hello World"),
                Options = new ToastOptions { Closable = true, AutoDismiss = false, Position = ToastPosition.TopRight }
            }),
            ("TopLeft_NoClose", new ToastState
            {
                Content = b => b.AddContent(0, "Top Left"),
                Options = new ToastOptions { Closable = false, AutoDismiss = false, Position = ToastPosition.TopLeft }
            }),
            ("BottomRight_NoClose", new ToastState
            {
                Content = b => b.AddContent(0, "Bottom Right"),
                Options = new ToastOptions { Closable = false, AutoDismiss = false, Position = ToastPosition.BottomRight }
            }),
        ];

        var results = testCases.Select(tc =>
        {
            IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
                .Add(c => c.State, tc.State));
            return new { tc.Name, Html = cut.GetNormalizedMarkup() };
        }).ToArray();

        await Verifier.Verify(results).UseParameters(scenario.Name);
    }
}
