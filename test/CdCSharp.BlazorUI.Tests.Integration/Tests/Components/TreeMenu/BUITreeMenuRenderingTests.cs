using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.TreeMenu;

[Trait("Component Rendering", "BUITreeMenu")]
public class BUITreeMenuRenderingTests
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
            new MenuItem("child1", "Child 1"),
            new MenuItem("child2", "Child 2"),
        ]),
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("tree-menu");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nav_With_Menubar_Role(BlazorScenario scenario)
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
    public async Task Should_Render_Items_With_Menuitem_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key));

        // Assert
        cut.FindAll("[role='menuitem']").Should().HaveCount(2);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Disabled_Item_With_DataBuiDisabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — declarative mode required; imperative mode has no DisabledSelector
        IRenderedComponent<BUITreeMenu<object>> cut = ctx.Render<BUITreeMenu<object>>(p => p
            .Add(c => c.ChildContent, b =>
            {
                b.OpenComponent<BUITreeMenuItem>(0);
                b.AddAttribute(1, "Key", "x");
                b.AddAttribute(2, "Text", "X");
                b.AddAttribute(3, "Disabled", true);
                b.CloseComponent();
            }));

        // Assert
        cut.Find(".bui-tree-menu__item").GetAttribute("data-bui-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Items_With_Expanded_False_Initially(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, NestedItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.ChildrenSelector, m => m.Children));

        // Assert
        cut.Find(".bui-tree-menu__item").GetAttribute("data-bui-expanded").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Orientation_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUITreeMenu<MenuItem>> cut = ctx.Render<BUITreeMenu<MenuItem>>(p => p
            .Add(c => c.Items, FlatItems)
            .Add(c => c.KeySelector, m => m.Key)
            .Add(c => c.Orientation, TreeMenuOrientation.Horizontal));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-orientation").Should().Be("horizontal");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Parent_Node_With_HasPopup(BlazorScenario scenario)
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
}
