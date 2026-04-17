using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Badge;

[Trait("Component State", "BUINotificationBadge")]
public class BUINotificationBadgeStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Badge_Content_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "1")));

        cut.Find("span.bui-badge").TextContent.Should().Contain("1");

        // Act
        cut.Render(p => p.Add(c => c.BadgeContent, b => b.AddContent(0, "99+")));

        // Assert
        cut.Find("span.bui-badge").TextContent.Should().Contain("99+");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.Size, SizeEnum.Small));

        cut.Find(".bui-notification-badge__indicator bui-component")
            .GetAttribute("data-bui-size").Should().Be("small");

        // Act
        cut.Render(p => p.Add(c => c.Size, SizeEnum.Large));

        // Assert
        cut.Find(".bui-notification-badge__indicator bui-component")
            .GetAttribute("data-bui-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Position_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.Position, BadgePosition.TopRight));

        cut.Find("bui-component").GetAttribute("data-bui-position").Should().Be("topright");

        // Act
        cut.Render(p => p.Add(c => c.Position, BadgePosition.BottomLeft));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-position").Should().Be("bottomleft");
    }
}
