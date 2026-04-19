using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.StackedLayout;

[Trait("Component State", "BUIStackedLayout")]
public class BUIStackedLayoutStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_StickyHeader_In_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIStackedLayout> cut = ctx.Render<BUIStackedLayout>(p => p
            .Add(c => c.StickyHeader, true));
        cut.Find("bui-component").GetAttribute("data-bui-sticky-header").Should().Be("true");

        // Act
        cut.Render(p => p.Add(c => c.StickyHeader, false));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-sticky-header").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_StickyNav_In_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIStackedLayout> cut = ctx.Render<BUIStackedLayout>(p => p
            .Add(c => c.StickyNav, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-sticky-nav").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_NavOpen_In_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIStackedLayout> cut = ctx.Render<BUIStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n"))
            .Add(c => c.ShowToggle, true));
        cut.Find("bui-component").GetAttribute("data-bui-nav-open").Should().Be("false");

        // Act
        cut.Find(".bui-stacked-layout__toggle").Click();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-nav-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Accept_NavOpen_As_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — parameter two-way binding: pre-open the nav.
        IRenderedComponent<BUIStackedLayout> cut = ctx.Render<BUIStackedLayout>(p => p
            .Add(c => c.Nav, b => b.AddContent(0, "n"))
            .Add(c => c.NavOpen, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-nav-open").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_HeaderHeight_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIStackedLayout> cut = ctx.Render<BUIStackedLayout>(p => p
            .Add(c => c.HeaderHeight, "72px"));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-header-height: 72px");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_NavColumns_CssVariable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIStackedLayout> cut = ctx.Render<BUIStackedLayout>(p => p
            .Add(c => c.NavColumns, 3));

        // Assert
        cut.Find("bui-component").GetAttribute("style").Should().Contain("--bui-inline-nav-columns: 3");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_ContentMaxWidth_And_NavGap_CssVariables(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIStackedLayout> cut = ctx.Render<BUIStackedLayout>(p => p
            .Add(c => c.ContentMaxWidth, "1200px")
            .Add(c => c.NavGap, "1rem")
            .Add(c => c.NavMinColumnWidth, "180px"));

        // Assert
        string style = cut.Find("bui-component").GetAttribute("style") ?? "";
        style.Should().Contain("--bui-inline-content-max-width: 1200px");
        style.Should().Contain("--bui-inline-nav-gap: 1rem");
        style.Should().Contain("--bui-inline-nav-col-min: 180px");
    }
}
