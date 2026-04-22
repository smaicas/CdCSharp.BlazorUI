using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Badge;

[Trait("Component State", "BUIBadge")]
public class BUIBadgeStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Content_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "1")));

        cut.Find("span.bui-badge").TextContent.Should().Contain("1");

        // Act
        cut.Render(p => p.Add(c => c.ChildContent, b => b.AddContent(0, "99")));

        // Assert
        cut.Find("span.bui-badge").TextContent.Should().Contain("99");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Circular_Attribute_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "5")));

        cut.Find("bui-component").GetAttribute("data-bui-circular").Should().BeNull();

        // Act
        cut.Render(p => p
            .Add(c => c.Circular, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "5")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-circular").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Color_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.Color, "#fff")
            .Add(c => c.ChildContent, b => b.AddContent(0, "N")));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-color");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Size_Attribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIBadge> cut = ctx.Render<BUIBadge>(p => p
            .Add(c => c.Size, BUISize.Large)
            .Add(c => c.ChildContent, b => b.AddContent(0, "X")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }
}
