using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.AspNetCore.Components;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Tabs;

[Trait("Component State", "BUITabs")]
public class BUITabsStateTests
{
    private static RenderFragment TwoTabs => b =>
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
    public async Task Should_Switch_Active_Tab_When_ActiveTab_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab1"));

        cut.Find("[role='tabpanel']").TextContent.Should().Contain("Content One");

        // Act
        cut.Render(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab2"));

        // Assert
        cut.Find("[role='tabpanel']").TextContent.Should().Contain("Content Two");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Second_Tab_Active_When_Switched(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.ActiveTab, "tab2"));

        // Assert — second tab button has active
        IReadOnlyList<IElement> tabs = cut.FindAll("[role='tab']");
        tabs[0].GetAttribute("data-bui-active").Should().Be("false");
        tabs[1].GetAttribute("data-bui-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUITabs> cut = ctx.Render<BUITabs>(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.Size, BUISize.Small));

        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("small");

        // Act
        cut.Render(p => p
            .Add(c => c.ChildContent, TwoTabs)
            .Add(c => c.Size, BUISize.Large));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }
}
