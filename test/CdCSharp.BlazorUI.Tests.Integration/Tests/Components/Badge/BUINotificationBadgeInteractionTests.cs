using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Badge;

[Trait("Component Interaction", "BUINotificationBadge")]
public class BUINotificationBadgeInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_Click_On_Host_ChildContent(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int clicks = 0;
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenElement(0, "button");
                b.AddAttribute(1, "type", "button");
                b.AddAttribute(2, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => clicks++));
                b.AddContent(3, "Host");
                b.CloseElement();
            })
            .Add(c => c.BadgeContent, b => b.AddContent(0, "3")));

        // Act
        cut.Find("button").Click();

        // Assert — click bubbles through the badge wrapper to the host handler.
        clicks.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_Own_Click_Handler(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "3")));

        // Assert — the badge is a passive indicator; no onclick on root/indicator.
        cut.Find("bui-component").GetAttribute("blazor:onclick").Should().BeNull();
        cut.Find(".bui-notification-badge__indicator").GetAttribute("blazor:onclick").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_Unmatched_Onclick_Via_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — consumer may attach @onclick on the component; it reaches the host element.
        int clicks = 0;
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "3"))
            .AddUnmatched("onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => clicks++)));

        // Act
        cut.Find("bui-component").Click();

        // Assert
        clicks.Should().Be(1);
    }
}
