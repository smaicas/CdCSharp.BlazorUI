using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Rendering", "BUIDrawer")]
public class BUIDrawerRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_When_Closed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDrawer> cut = ctx.Render<BUIDrawer>(p => p
            .Add(c => c.Open, false));

        // Assert
        cut.FindAll("[role='dialog']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDrawer> cut = ctx.Render<BUIDrawer>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find("[role='dialog']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Right_Position_Class_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDrawer> cut = ctx.Render<BUIDrawer>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find(".bui-drawer--right").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Left_Position_Class(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDrawer> cut = ctx.Render<BUIDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.Position, DrawerPosition.Left));

        // Assert
        cut.Find(".bui-drawer--left").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDrawer> cut = ctx.Render<BUIDrawer>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.ChildContent, b => b.AddContent(0, "Drawer content")));

        // Assert
        cut.Find(".bui-drawer__content").TextContent.Should().Contain("Drawer content");
    }
}
