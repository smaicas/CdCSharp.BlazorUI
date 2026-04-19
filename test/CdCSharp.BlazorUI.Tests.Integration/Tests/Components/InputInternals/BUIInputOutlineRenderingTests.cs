using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.InputInternals;

[Trait("Component Rendering", "BUIInputOutline")]
public class BUIInputOutlineRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Outline_Structure(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputOutline> cut = ctx.Render<BUIInputOutline>();

        // Assert
        IElement outline = cut.Find("div.bui-input__outline");
        outline.GetAttribute("aria-hidden").Should().Be("true");
        outline.QuerySelector("span.bui-input__outline-leading").Should().NotBeNull();
        outline.QuerySelector("span.bui-input__outline-notch").Should().NotBeNull();
        outline.QuerySelector("span.bui-input__outline-trailing").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputOutline> cut = ctx.Render<BUIInputOutline>();

        // Assert
        cut.FindAll("label.bui-input__label").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_Inside_Notch(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputOutline> cut = ctx.Render<BUIInputOutline>(p => p
            .Add(c => c.Label, "Full name")
            .Add(c => c.For, "inp-42"));

        // Assert
        IElement notch = cut.Find("span.bui-input__outline-notch");
        IElement label = notch.QuerySelector("label.bui-input__label")!;
        label.Should().NotBeNull();
        label.GetAttribute("for").Should().Be("inp-42");
        label.TextContent.Should().Contain("Full name");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Marker_When_Required_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputOutline> cut = ctx.Render<BUIInputOutline>(p => p
            .Add(c => c.Label, "Email")
            .Add(c => c.Required, true));

        // Assert
        IElement marker = cut.Find("label.bui-input__label span.bui-input__required");
        marker.TextContent.Should().Be("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Required_Marker_When_Required_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputOutline> cut = ctx.Render<BUIInputOutline>(p => p
            .Add(c => c.Label, "Email"));

        // Assert
        cut.FindAll("span.bui-input__required").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Leave_Notch_Empty_When_Label_Whitespace(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIInputOutline> cut = ctx.Render<BUIInputOutline>(p => p
            .Add(c => c.Label, "   "));

        // Assert
        cut.Find("span.bui-input__outline-notch").Children.Should().BeEmpty();
    }
}
