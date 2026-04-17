using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component Accessibility", "BUITreeMenu")]
public class BUITreeMenuAccessibilityTests
{
    private sealed record MenuItem(string Key, string Label, IEnumerable<MenuItem>? Children = null, bool Disabled = false);

    private static IEnumerable<MenuItem> FlatItems =>
    [
        new MenuItem("a", "Alpha"),
        new MenuItem("b", "Beta"),
    ];

    private static IEnumerable<MenuItem> NestedItems =>
    [
        new MenuItem("parent", "Parent", [
            new MenuItem("child", "Child"),
        ]),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Nav_Have_Role_Menubar(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("nav").GetAttribute("role").Should().Be("menubar");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Nav_Have_Aria_Orientation_Vertical(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("nav").GetAttribute("aria-orientation").Should().Be("vertical");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Parent_Button_Have_Aria_Haspopup_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Assert
        cut.Find("[role='menuitem']").GetAttribute("aria-haspopup").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expanded_Parent_Have_Aria_Expanded_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Act
        cut.Find("[role='menuitem']").Click();

        // Assert
        cut.Find("[role='menuitem']").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disabled_Item_Have_Aria_Disabled_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — declarative mode required; imperative mode has no DisabledSelector
        IRenderedComponent<BUITreeMenu<object>> cut = ctx.Render<BUITreeMenu<object>>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BUITreeMenuItem>(0);
                b.AddAttribute(1, "Key", "d");
                b.AddAttribute(2, "Text", "D");
                b.AddAttribute(3, "Disabled", true);
                b.CloseComponent();
            }));

        // Assert
        cut.Find("[role='menuitem']").GetAttribute("aria-disabled").Should().Be("true");
    }
}
