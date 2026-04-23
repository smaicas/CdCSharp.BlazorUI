using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.FlexStack;

[Trait("Component State", "BUIFlexStack")]
public class BUIFlexStackStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Direction_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Direction, FlexStackDirection.Column));

        cut.Find("bui-component").GetAttribute("data-dir").Should().Be("column");

        // Act
        cut.Render(p => p.Add(c => c.Direction, FlexStackDirection.RowReverse));

        // Assert
        cut.Find("bui-component").GetAttribute("data-dir").Should().Be("row-reverse");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Direction_DataAttribute_When_Reset_To_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Direction, FlexStackDirection.Column));

        cut.Find("bui-component").HasAttribute("data-dir").Should().BeTrue();

        // Act
        cut.Render(p => p.Add(c => c.Direction, FlexStackDirection.Row));

        // Assert
        cut.Find("bui-component").HasAttribute("data-dir").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Gap_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Gap, "1rem"));

        cut.Find("bui-component").GetAttribute("style").Should().Contain("--gap: 1rem");

        // Act
        cut.Render(p => p.Add(c => c.Gap, "2rem"));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--gap: 2rem");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_UserAttributes_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .AddUnmatched("data-testid", "my-flex")
            .Add(c => c.Gap, "1rem"));

        // Act
        cut.Render(p => p
            .AddUnmatched("data-testid", "my-flex")
            .Add(c => c.Gap, "2rem"));

        // Assert
        cut.Find("bui-component").GetAttribute("data-testid").Should().Be("my-flex");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_FullWidth_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.FullWidth, false));

        cut.Find("bui-component").GetAttribute("data-bui-fullwidth").Should().Be("false");

        // Act
        cut.Render(p => p.Add(c => c.FullWidth, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-fullwidth").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Color_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string initialColor = "rgba(255,0,0,1)";
        string updatedColor = "rgba(0,255,0,1)";

        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Color, initialColor));

        cut.Find("bui-component").GetAttribute("style").Should().Contain($"--bui-inline-color: {initialColor}");

        // Act
        cut.Render(p => p.Add(c => c.Color, updatedColor));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain($"--bui-inline-color: {updatedColor}");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Shadow_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Shadow, ShadowStyle.Create(2, 4)));

        cut.Find("bui-component").GetAttribute("data-bui-shadow").Should().Be("true");

        // Act
        cut.Render(p => p.Add(c => c.Shadow, ShadowStyle.Create(4, 8)));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-shadow").Should().Be("true");
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-shadow:");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Border_On_Rerender(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIFlexStack> cut = ctx.Render<BUIFlexStack>(p => p
            .Add(c => c.Border, BorderStyle.Create().All("1px", BorderStyleType.Solid, "red")));

        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-border");

        // Act
        cut.Render(p => p.Add(c => c.Border, BorderStyle.Create().All("2px", BorderStyleType.Dashed, "blue")));

        // Assert
        string style = cut.Find("bui-component").GetAttribute("style")!;
        style.Should().Contain("--bui-inline-border");
        style.Should().Contain("2px dashed blue");
    }
}
