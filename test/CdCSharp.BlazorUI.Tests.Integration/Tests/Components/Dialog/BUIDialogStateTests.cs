using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dialog;

[Trait("Component State", "BUIDialog")]
public class BUIDialogStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Dialog_When_Open_Becomes_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDialog> cut = ctx.Render<BUIDialog>(p => p
            .Add(c => c.Open, false));

        cut.FindAll("[role='dialog']").Should().BeEmpty();

        // Act
        cut.Render(p => p.Add(c => c.Open, true));

        // Assert
        cut.Find("[role='dialog']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Dialog_When_Open_Becomes_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIDialog> cut = ctx.Render<BUIDialog>(p => p
            .Add(c => c.Open, true));

        cut.Find("[role='dialog']").Should().NotBeNull();

        // Act
        cut.Render(p => p.Add(c => c.Open, false));

        // Assert
        cut.FindAll("[role='dialog']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Overlay_When_Open(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIDialog> cut = ctx.Render<BUIDialog>(p => p
            .Add(c => c.Open, true));

        // Assert
        cut.Find(".bui-dialog-overlay").Should().NotBeNull();
    }
}
