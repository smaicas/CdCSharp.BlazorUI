using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Linq.Expressions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dropdown;

[Trait("Component Interaction", "BUIInputDropdownTree")]
public class BUIInputDropdownTreeInteractionTests
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
    public async Task Should_Open_Menu_On_Trigger_Click(BlazorScenario scenario)
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

        // Assert
        cut.Find(".bui-dropdown__menu").Should().NotBeNull();
        cut.Find(".bui-dropdown__tree-content").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Menu_On_Second_Trigger_Click(BlazorScenario scenario)
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
        cut.Find("button.bui-dropdown__trigger").Click();

        // Assert
        cut.FindAll(".bui-dropdown__menu").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Tree_Nodes_When_Open(BlazorScenario scenario)
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

        // Assert — tree node text should be in markup
        cut.Markup.Should().Contain("Node 1");
        cut.Markup.Should().Contain("Node 2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Node_Content_When_Open(BlazorScenario scenario)
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

        // Assert — tree nodes are rendered with correct content structure
        IReadOnlyList<IElement> nodeContents = cut.FindAll(".bui-tree-selector__node-content");
        nodeContents.Should().NotBeEmpty();
        cut.Find(".bui-dropdown__tree-content").Should().NotBeNull();
    }
}
