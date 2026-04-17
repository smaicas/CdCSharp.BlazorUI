using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Accessibility", "BUITabs")]
public class BUITabsAccessibilityTests
{
    private static RenderFragment TwoTabs => b =>
    {
        b.OpenComponent<BUITab>(0);
        b.AddAttribute(1, "Id", "tab1");
        b.AddAttribute(2, "Label", "Tab One");
        b.AddAttribute(3, nameof(BUITab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "C1")));
        b.CloseComponent();
        b.OpenComponent<BUITab>(4);
        b.AddAttribute(5, "Id", "tab2");
        b.AddAttribute(6, "Label", "Tab Two");
        b.AddAttribute(7, nameof(BUITab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "C2")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Active_Tab_Have_Aria_Selected_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Assert
        cut.FindAll("[role='tab']")[0].GetAttribute("aria-selected").Should().Be("true");
        cut.FindAll("[role='tab']")[1].GetAttribute("aria-selected").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Tab_Have_Aria_Controls_Pointing_To_Panel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Assert — aria-controls matches panel id
        cut.FindAll("[role='tab']")[0].GetAttribute("aria-controls")
            .Should().Be("bui-tabpanel-tab1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Active_Tab_Have_Tabindex_Zero(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Assert — active tab tabindex=0, inactive=-1 (roving tabindex)
        cut.FindAll("[role='tab']")[0].GetAttribute("tabindex").Should().Be("0");
        cut.FindAll("[role='tab']")[1].GetAttribute("tabindex").Should().Be("-1");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Panel_Have_Role_Tabpanel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, TwoTabs));

        // Assert
        cut.Find("[role='tabpanel']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Panel_Have_Aria_LabelledBy_Pointing_To_Tab(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        // Assert
        cut.Find("[role='tabpanel']").GetAttribute("aria-labelledby")
            .Should().Be("bui-tab-tab1");
    }
}
