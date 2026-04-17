using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Svg;

[Trait("Component State", "BUISvgIcon")]
public class BUISvgIconStateTests
{
    private const string IconA = "<path d=\"M1 1h22v22H1z\"/>";
    private const string IconB = "<circle cx=\"12\" cy=\"12\" r=\"10\"/>";

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Icon_Content_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUISvgIcon> cut = ctx.Render<BUISvgIcon>(p => p
            .Add(c => c.Icon, IconA));

        cut.Find("svg").InnerHtml.Should().Contain("M1 1h22v22H1z");

        // Act
        cut.Render(p => p.Add(c => c.Icon, IconB));

        // Assert
        cut.Find("svg").InnerHtml.Should().Contain("cx=\"12\"");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Size_Attribute_On_Re_Render(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUISvgIcon> cut = ctx.Render<BUISvgIcon>(p => p
            .Add(c => c.Icon, IconA)
            .Add(c => c.Size, SizeEnum.Small));

        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("small");

        // Act
        cut.Render(p => p
            .Add(c => c.Icon, IconA)
            .Add(c => c.Size, SizeEnum.Large));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-size").Should().Be("large");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Apply_Color_Variable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUISvgIcon> cut = ctx.Render<BUISvgIcon>(p => p
            .Add(c => c.Icon, IconA)
            .Add(c => c.Color, "#ff0000"));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-color");
    }
}
