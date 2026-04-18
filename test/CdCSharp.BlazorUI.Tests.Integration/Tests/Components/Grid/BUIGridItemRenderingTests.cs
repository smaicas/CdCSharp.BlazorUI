using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Grid;

[Trait("Component Rendering", "BUIGridItem")]
public class BUIGridItemRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIGridItem> cut = ctx.Render<BUIGridItem>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("grid-item");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Span_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIGridItem> cut = ctx.Render<BUIGridItem>(p => p
            .Add(c => c.Span, 2));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--span: 2");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataSized_When_Span_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIGridItem> cut = ctx.Render<BUIGridItem>(p => p
            .Add(c => c.Span, 4));

        // Assert
        cut.Find("bui-component").GetAttribute("data-sized").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_DataAuto_When_Auto(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIGridItem> cut = ctx.Render<BUIGridItem>(p => p
            .Add(c => c.Auto, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-auto").Should().Be("true");
    }
}
