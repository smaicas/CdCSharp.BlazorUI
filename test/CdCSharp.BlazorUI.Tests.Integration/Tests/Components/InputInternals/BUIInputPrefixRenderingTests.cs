using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "BUIInputPrefix")]
public class BUIInputPrefixRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_Neither_Text_Nor_Icon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputPrefix> cut = ctx.Render<BUIInputPrefix>();

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Text_Inside_Prefix_Addon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputPrefix> cut = ctx.Render<BUIInputPrefix>(p => p
            .Add(c => c.PrefixText, "https://"));

        // Assert
        IElement addon = cut.Find(".bui-input__addon--prefix");
        addon.ClassList.Should().Contain("_bui-addon");
        addon.QuerySelector("span")!.TextContent.Should().Be("https://");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Icon_When_Only_Icon_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputPrefix> cut = ctx.Render<BUIInputPrefix>(p => p
            .Add(c => c.PrefixIcon, "search"));

        // Assert
        IElement addon = cut.Find(".bui-input__addon--prefix");
        addon.Should().NotBeNull();
        addon.QuerySelectorAll("span").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Ignore_Whitespace_Only_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputPrefix> cut = ctx.Render<BUIInputPrefix>(p => p
            .Add(c => c.PrefixText, "   ")
            .Add(c => c.PrefixIcon, "  "));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_When_Prefix_Parameters_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIInputPrefix> cut = ctx.Render<BUIInputPrefix>(p => p
            .Add(c => c.PrefixText, "before"));
        cut.Find(".bui-input__addon--prefix span").TextContent.Should().Be("before");

        // Act
        cut.Render(p => p.Add(c => c.PrefixText, "after"));

        // Assert
        cut.Find(".bui-input__addon--prefix span").TextContent.Should().Be("after");

        // Act
        cut.Render(p => p.Add(c => c.PrefixText, (string?)null));

        // Assert
        cut.Markup.Trim().Should().BeEmpty();
    }
}
