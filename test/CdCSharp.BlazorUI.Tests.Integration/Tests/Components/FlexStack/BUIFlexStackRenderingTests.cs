using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.FlexStack;

[Trait("Component Rendering", "BUIFlexStack")]
public class BUIFlexStackRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "x")));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("flex-stack");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Direction_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Direction, FlexStackDirection.Column));

        // Assert
        cut.Find("bui-component").GetAttribute("data-dir").Should().Be("column");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Wrap_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Wrap, FlexStackWrap.NoWrap));

        // Assert
        cut.Find("bui-component").GetAttribute("data-wrap").Should().Be("no-wrap");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_JustifyContent_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.JustifyContent, FlexStackJustifyContent.Center));

        // Assert
        cut.Find("bui-component").GetAttribute("data-justify").Should().Be("center");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AlignItems_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.AlignItems, FlexStackAlignItems.Center));

        // Assert
        cut.Find("bui-component").GetAttribute("data-align").Should().Be("center");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AlignContent_DataAttribute_When_Not_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.AlignContent, FlexStackAlignContent.SpaceBetween));

        // Assert
        cut.Find("bui-component").GetAttribute("data-align-content").Should().Be("space-between");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Gap_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Gap, "1.5rem"));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--gap: 1.5rem");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_FullWidth_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.FullWidth, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-fullwidth").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Children(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.ChildContent, b => b.AddContent(0, "Stack child")));

        // Assert
        cut.Find("bui-component").TextContent.Should().Contain("Stack child");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Spacing_CssVariables(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.P, "1rem")
            .Add(c => c.Mx, "auto"));

        // Assert
        string style = cut.Find("bui-component").GetAttribute("style")!;
        style.Should().Contain("--p: 1rem");
        style.Should().Contain("--mr: auto");
        style.Should().Contain("--ml: auto");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_BackgroundColor_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.BackgroundColor, "rgba(255,0,0,1)"));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-background: rgba(255,0,0,1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Color_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Color, "rgba(0,255,0,1)"));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-color: rgba(0,255,0,1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Shadow_DataAttribute_And_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Shadow, ShadowStyle.Create(4, 8)));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-shadow").Should().Be("true");
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Border_InlineVar(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Border, BorderStyle.Create().All("1px", BorderStyleType.Solid, "red")));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-border");
    }
}
