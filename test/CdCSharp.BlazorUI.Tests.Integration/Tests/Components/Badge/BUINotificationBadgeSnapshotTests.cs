using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Badge;

[Trait("Component Snapshots", "BUINotificationBadge")]
public class BUINotificationBadgeSnapshotTests
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
                Name = "Default_Dot_TopRight",
                Html = ctx.Render<BUINotificationBadge>().GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Count_3_TopRight",
                Html = ctx.Render<BUINotificationBadge>(p => p
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Count_99_Plus",
                Html = ctx.Render<BUINotificationBadge>(p => p
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "99+"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "BottomLeft_Position",
                Html = ctx.Render<BUINotificationBadge>(p => p
                    .Add(c => c.Position, BadgePosition.BottomLeft)
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "1"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Large_Size",
                Html = ctx.Render<BUINotificationBadge>(p => p
                    .Add(c => c.Size, BUISize.Large)
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "5"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "Non_Circular",
                Html = ctx.Render<BUINotificationBadge>(p => p
                    .Add(c => c.Circular, false)
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "New"))).GetNormalizedMarkup()
            },
            new
            {
                Name = "With_Host_Button",
                Html = ctx.Render<BUINotificationBadge>(p => p
                    .Add(c => c.ChildContent, b =>
                    {
                        b.OpenElement(0, "button");
                        b.AddAttribute(1, "type", "button");
                        b.AddContent(2, "Inbox");
                        b.CloseElement();
                    })
                    .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))).GetNormalizedMarkup()
            },
        };

        await Verify(testCases).UseParameters(scenario.Name);
    }
}
