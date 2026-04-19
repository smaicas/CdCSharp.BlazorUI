using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Grid;

[Trait("Component State", "BUIGrid")]
public class BUIGridStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Columns_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIGrid> cut = ctx.Render<BUIGrid>(p => p
            .Add(c => c.Columns, 2));

        cut.Find("bui-component").GetAttribute("style").Should().Contain("--columns: 2");

        // Act
        cut.Render(p => p.Add(c => c.Columns, 4));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--columns: 4");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Direction_DataAttribute_When_Reset_To_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIGrid> cut = ctx.Render<BUIGrid>(p => p
            .Add(c => c.Direction, GridDirection.Column));

        cut.Find("bui-component").HasAttribute("data-dir").Should().BeTrue();

        // Act
        cut.Render(p => p.Add(c => c.Direction, GridDirection.Row));

        // Assert
        cut.Find("bui-component").HasAttribute("data-dir").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_UserAttributes_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIGrid> cut = ctx.Render<BUIGrid>(p => p
            .AddUnmatched("data-testid", "my-grid")
            .Add(c => c.Columns, 2));

        // Act
        cut.Render(p => p
            .AddUnmatched("data-testid", "my-grid")
            .Add(c => c.Columns, 3));

        // Assert
        cut.Find("bui-component").GetAttribute("data-testid").Should().Be("my-grid");
    }
}
