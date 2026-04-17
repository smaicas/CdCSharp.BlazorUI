using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Badge;

[Trait("Component Rendering", "BUINotificationBadge")]
public class BUINotificationBadgeRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("notification-badge");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Indicator_Div(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>();

        // Assert
        cut.Find(".bui-notification-badge__indicator").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Inner_BUIBadge(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.BadgeContent, b => b.AddContent(0, "5")));

        // Assert — inner BUIBadge renders a span.bui-badge
        cut.Find("span.bui-badge").TextContent.Should().Contain("5");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Position_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.Position, BadgePosition.BottomLeft));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-position").Should().Be("bottomleft");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ChildContent_As_Host_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenElement(0, "button");
                b.AddContent(1, "Notifications");
                b.CloseElement();
            })
            .Add(c => c.BadgeContent, b => b.AddContent(0, "3")));

        // Assert — host content renders
        cut.Find("button").TextContent.Should().Contain("Notifications");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Default_Dot_Mode_In_Indicator_When_No_Badge_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — no BadgeContent = dot mode in inner badge
        IRenderedComponent<BUINotificationBadge> cut = ctx.Render<BUINotificationBadge>();

        // Assert — inner badge has dot attribute
        IElement innerBadge = cut.Find(".bui-notification-badge__indicator bui-component");
        innerBadge.GetAttribute("data-bui-dot").Should().Be("true");
    }
}
