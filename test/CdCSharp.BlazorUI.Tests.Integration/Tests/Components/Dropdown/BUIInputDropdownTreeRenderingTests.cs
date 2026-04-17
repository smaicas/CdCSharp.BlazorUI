using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Rendering", "BUIInputDropdownTree")]
public class BUIInputDropdownTreeRenderingTests
{
    private record TreeNode(string Key, string Name, List<TreeNode>? Children = null);

    
    private class DummyModel { public string? Value { get; set; } }
    private static readonly DummyModel _dm = new();
    private static readonly Expression<Func<string?>> _expr = () => _dm.Value;

    private static List<TreeNode> SampleItems =>
    [
        new("1", "Node 1", [new("1.1", "Child 1.1"), new("1.2", "Child 1.2")]),
        new("2", "Node 2")
    ];

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_DropdownContainer_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdownTree<TreeNode, string>> cut =
            ctx.Render<BUIInputDropdownTree<TreeNode, string>>(p => p
                .Add(c => c.ValueExpression, _expr)
                .Add(c => c.Items, SampleItems)
                .Add(c => c.KeySelector, n => n.Key)
                .Add(c => c.DisplayTextSelector, n => n.Name)
                .Add(c => c.ChildrenSelector, n => n.Children));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("dropdown-container");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Input_Family_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdownTree<TreeNode, string>> cut =
            ctx.Render<BUIInputDropdownTree<TreeNode, string>>(p => p
                .Add(c => c.ValueExpression, _expr)
                .Add(c => c.Items, SampleItems)
                .Add(c => c.KeySelector, n => n.Key)
                .Add(c => c.DisplayTextSelector, n => n.Name)
                .Add(c => c.ChildrenSelector, n => n.Children));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-input-base").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Trigger_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdownTree<TreeNode, string>> cut =
            ctx.Render<BUIInputDropdownTree<TreeNode, string>>(p => p
                .Add(c => c.ValueExpression, _expr)
                .Add(c => c.Items, SampleItems)
                .Add(c => c.KeySelector, n => n.Key)
                .Add(c => c.DisplayTextSelector, n => n.Name)
                .Add(c => c.ChildrenSelector, n => n.Children));

        // Assert
        cut.Find("button.bui-dropdown__trigger").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Menu_When_Closed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputDropdownTree<TreeNode, string>> cut =
            ctx.Render<BUIInputDropdownTree<TreeNode, string>>(p => p
                .Add(c => c.ValueExpression, _expr)
                .Add(c => c.Items, SampleItems)
                .Add(c => c.KeySelector, n => n.Key)
                .Add(c => c.DisplayTextSelector, n => n.Name)
                .Add(c => c.ChildrenSelector, n => n.Children));

        // Assert
        cut.FindAll(".bui-dropdown__menu").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_No_Options_Message_When_Items_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — no items provided
        IRenderedComponent<BUIInputDropdownTree<TreeNode, string>> cut =
            ctx.Render<BUIInputDropdownTree<TreeNode, string>>(p => p
                .Add(c => c.ValueExpression, _expr)
                .Add(c => c.Items, [])
                .Add(c => c.KeySelector, n => n.Key)
                .Add(c => c.DisplayTextSelector, n => n.Name)
                .Add(c => c.ChildrenSelector, n => n.Children));

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert
        cut.Find(".bui-dropdown__no-options").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Tree_Aria_Role_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputDropdownTree<TreeNode, string>> cut =
            ctx.Render<BUIInputDropdownTree<TreeNode, string>>(p => p
                .Add(c => c.ValueExpression, _expr)
                .Add(c => c.Items, SampleItems)
                .Add(c => c.KeySelector, n => n.Key)
                .Add(c => c.DisplayTextSelector, n => n.Name)
                .Add(c => c.ChildrenSelector, n => n.Children));

        // Act
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert — tree dropdown uses role="tree" instead of "listbox"
        cut.Find(".bui-dropdown__menu").GetAttribute("role").Should().Be("tree");
    }
}
