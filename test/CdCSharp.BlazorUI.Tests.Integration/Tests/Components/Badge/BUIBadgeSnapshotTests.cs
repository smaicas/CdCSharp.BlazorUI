using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Badge;

[Trait("Component Snapshots", "BUIBadge")]
public class BUIBadgeSnapshotTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Match_Snapshots_For_All_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        var testCases = new[]
        {
            new
            {
                Name = "Dot_Mode",
                Html = ctx.Render<BUIBadge>().GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Content",
                Html = ctx.Render<BUIBadge>(p => p
                    .Add(c => c.ChildContent, b => b.AddContent(0, "5"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Circular_With_Content",
                Html = ctx.Render<BUIBadge>(p => p
                    .Add(c => c.Circular, true)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "99"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Large_Size",
                Html = ctx.Render<BUIBadge>(p => p
                    .Add(c => c.Size, BUISize.Large)
                    .Add(c => c.ChildContent, b => b.AddContent(0, "New"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Notification_Badge_TopRight",
                Html = ctx.Render<BUINotificationBadge>(p => p
                    .Add(c => c.Position, BadgePosition.TopRight)
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
