using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Tabs;

[Trait("Component Rendering", "BUITabs")]
public class BUITabsRenderingTests
{
    private static RenderFragment BuildTwoTabs(string? activeTabId = null) => b =>
    {
        b.OpenComponent<BUITab>(0);
        b.AddAttribute(1, "Id", "tab1");
        b.AddAttribute(2, "Label", "Tab One");
        b.AddAttribute(3, nameof(BUITab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Content One")));
        b.CloseComponent();
        b.OpenComponent<BUITab>(4);
        b.AddAttribute(5, "Id", "tab2");
        b.AddAttribute(6, "Label", "Tab Two");
        b.AddAttribute(7, nameof(BUITab.ChildContent), (RenderFragment)(b2 => b2.AddContent(0, "Content Two")));
        b.CloseComponent();
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, BuildTwoTabs()));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("tabs");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Tablist_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, BuildTwoTabs()));

        // Assert
        cut.Find("[role='tablist']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Tab_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, BuildTwoTabs()));

        // Assert
        cut.FindAll("[role='tab']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_First_Tab_Active_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, BuildTwoTabs()));

        // Assert — first tab has data-bui-active="true"
        cut.Find("[role='tab']").GetAttribute("data-bui-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Active_Tab_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, BuildTwoTabs())
            .Add(c => c.ActiveTab, "tab1"));

        // Assert — active tab content is visible
        cut.Find("[role='tabpanel']").TextContent.Should().Contain("Content One");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Tab_Labels(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, BuildTwoTabs()));

        // Assert
        cut.Find(".bui-tabs__tab-label").TextContent.Should().Be("Tab One");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant_Underline(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, BuildTwoTabs()));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("underline");
    }
}
