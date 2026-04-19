using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "BUIInputSuffix")]
public class BUIInputSuffixRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Neither_Text_Nor_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputSuffix> cut = ctx.Render<BUIInputSuffix>();

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Text_Inside_Suffix_Addon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputSuffix> cut = ctx.Render<BUIInputSuffix>(p => p
            .Add(c => c.SuffixText, ".com"));

        // Assert
        IElement addon = cut.Find(".bui-input__addon--suffix");
        addon.ClassList.Should().Contain("_bui-addon");
        addon.QuerySelector("span")!.TextContent.Should().Be(".com");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Icon_When_Only_Icon_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputSuffix> cut = ctx.Render<BUIInputSuffix>(p => p
            .Add(c => c.SuffixIcon, "close"));

        // Assert
        IElement addon = cut.Find(".bui-input__addon--suffix");
        addon.Should().NotBeNull();
        addon.QuerySelectorAll("span").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Ignore_Whitespace_Only_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputSuffix> cut = ctx.Render<BUIInputSuffix>(p => p
            .Add(c => c.SuffixText, "   ")
            .Add(c => c.SuffixIcon, "  "));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_When_Suffix_Parameters_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputSuffix> cut = ctx.Render<BUIInputSuffix>(p => p
            .Add(c => c.SuffixText, "before"));
        cut.Find(".bui-input__addon--suffix span").TextContent.Should().Be("before");

        // Act
        cut.Render(p => p.Add(c => c.SuffixText, "after"));

        // Assert
        cut.Find(".bui-input__addon--suffix span").TextContent.Should().Be("after");

        // Act
        cut.Render(p => p.Add(c => c.SuffixText, (string?)null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }
}
