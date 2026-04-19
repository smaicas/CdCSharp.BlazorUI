using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.SidebarLayout;

[Trait("Component Interaction", "BUISidebarLayout")]
public class BUISidebarLayoutInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Open_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUISidebarLayout> cut = ctx.Render<BUISidebarLayout>(p => p
            .Add(c => c.SidebarOpen, false)
            .Add(c => c.ShowToggle, true));

        cut.Find("bui-component").GetAttribute("data-bui-sidebar-open").Should().Be("false");

        // Act
        cut.Find(".bui-sidebar-layout__toggle").Click();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-sidebar-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_SidebarOpenChanged_On_Toggle(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? captured = null;
        IRenderedComponent<BUISidebarLayout> cut = ctx.Render<BUISidebarLayout>(p => p
            .Add(c => c.ShowToggle, true)
            .Add(c => c.SidebarOpenChanged, v => captured = v));

        // Act
        cut.Find(".bui-sidebar-layout__toggle").Click();

        // Assert
        captured.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_On_Scrim_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? captured = null;
        IRenderedComponent<BUISidebarLayout> cut = ctx.Render<BUISidebarLayout>(p => p
            .Add(c => c.SidebarOpen, true)
            .Add(c => c.SidebarOpenChanged, v => captured = v));

        // Act
        cut.Find(".bui-sidebar-layout__scrim").Click();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-sidebar-open").Should().Be("false");
        captured.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Button_Have_Aria_Expanded(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUISidebarLayout> cut = ctx.Render<BUISidebarLayout>(p => p
            .Add(c => c.SidebarOpen, true)
            .Add(c => c.ShowToggle, true));

        // Assert
        cut.Find(".bui-sidebar-layout__toggle").GetAttribute("aria-expanded").Should().Be("true");
    }
}
