using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.SidebarLayout;

[Trait("Component State", "BUISidebarLayout")]
public class BUISidebarLayoutStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Scrim_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISidebarLayout> cut = ctx.Render<BUISidebarLayout>(p => p
            .Add(c => c.SidebarOpen, true)
            .Add(c => c.ShowToggle, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-sidebar-open").Should().Be("true");
        cut.FindAll(".bui-sidebar-layout__scrim").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Show_Scrim_When_Closed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISidebarLayout> cut = ctx.Render<BUISidebarLayout>(p => p
            .Add(c => c.SidebarOpen, false));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-sidebar-open").Should().Be("false");
        cut.FindAll(".bui-sidebar-layout__scrim").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Open_State_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUISidebarLayout> cut = ctx.Render<BUISidebarLayout>(p => p
            .Add(c => c.SidebarOpen, false));

        // Act
        cut.Render(p => p.Add(c => c.SidebarOpen, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-sidebar-open").Should().Be("true");
    }
}
