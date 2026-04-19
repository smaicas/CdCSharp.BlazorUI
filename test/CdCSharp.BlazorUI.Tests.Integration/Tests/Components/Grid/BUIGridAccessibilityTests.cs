using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Grid;

[Trait("Component Accessibility", "BUIGrid")]
public class BUIGridAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_Semantic_Role_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — BUIGrid is a pure layout primitive.
        IRenderedComponent<BUIGrid> cut = ctx.Render<BUIGrid>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert — no role/aria automatically applied
        IElement root = cut.Find("bui-component");
        root.HasAttribute("role").Should().BeFalse();
        root.HasAttribute("aria-label").Should().BeFalse();
        root.HasAttribute("tabindex").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_Role_From_AdditionalAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — consumer opts in to role="list" / role="grid" via attributes.
        IRenderedComponent<BUIGrid> cut = ctx.Render<BUIGrid>(p => p
            .AddUnmatched("role", "list")
            .AddUnmatched("aria-label", "Products")
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        IElement root = cut.Find("bui-component");
        root.GetAttribute("role").Should().Be("list");
        root.GetAttribute("aria-label").Should().Be("Products");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Forward_Role_From_AdditionalAttributes_On_GridItem(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIGridItem> cut = ctx.Render<BUIGridItem>(p => p
            .AddUnmatched("role", "listitem")
            .Add(c => c.ChildContent, b => b.AddContent(0, "Item")));

        // Assert
        cut.Find("bui-component").GetAttribute("role").Should().Be("listitem");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Block_Semantic_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — heading inside grid cell preserves document outline.
        IRenderedComponent<BUIGrid> cut = ctx.Render<BUIGrid>(p => p
            .Add(c => c.ChildContent, b => b.AddMarkupContent(0, "<h2 id='section'>Heading</h2>")));

        // Assert
        cut.Find("h2#section").TextContent.Should().Be("Heading");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Leak_Breakpoint_Hide_Flags_As_AriaHidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — HideXs is visual-only (CSS media query driven).
        // SR should still reach the content; aria-hidden must not be emitted.
        IRenderedComponent<BUIGridItem> cut = ctx.Render<BUIGridItem>(p => p
            .Add(c => c.HideXs, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "Hidden on mobile")));

        // Assert
        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-hide-xs").Should().Be("true");
        root.HasAttribute("aria-hidden").Should().BeFalse();
    }
}
