using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.BaseComponents;

[Trait("Core", "BUIComponentBase")]
public class BUIComponentBaseTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Naming_Should_Remove_BUI_Prefix_And_Apply_KebabCase(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>();
        cut.Find("div").GetAttribute("data-bui-component").Should().Be("component-base_-test-stub");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Attributes_Should_Map_All_Boolean_And_Enum_States(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.Size, SizeEnum.Large)
            .Add(c => c.Density, DensityEnum.Compact)
            .Add(c => c.FullWidth, true)
            .Add(c => c.IsLoading, true)
            .Add(c => c.IsError, true)
            .Add(c => c.IsDisabled, true)
            .Add(c => c.IsReadOnly, true)
            .Add(c => c.IsRequired, true)
        );

        IElement el = cut.Find("div");
        el.GetAttribute("data-bui-size").Should().Be("large");
        el.GetAttribute("data-bui-density").Should().Be("compact");
        el.GetAttribute("data-bui-fullwidth").Should().Be("true");
        el.GetAttribute("data-bui-loading").Should().Be("true");
        el.GetAttribute("data-bui-error").Should().Be("true");
        el.GetAttribute("data-bui-disabled").Should().Be("true");
        el.GetAttribute("data-bui-readonly").Should().Be("true");
        el.GetAttribute("data-bui-required").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Elevation_Should_Handle_Value_And_Null(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p.Add(c => c.Elevation, 12));
        cut.Find("div").GetAttribute("data-bui-elevation").Should().Be("12");

        cut.Render(p => p.Add(c => c.Elevation, null));
        cut.Find("div").GetAttribute("data-bui-elevation").Should().Be("0");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Styles_Should_Handle_Colors_And_Ripple(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.Color, new CssColor("#FF0000"))
            .Add(c => c.BackgroundColor, new CssColor("#00FF00"))
            .Add(c => c.DisableRipple, false)
            .Add(c => c.RippleColor, new CssColor("#FFFFFF"))
            .Add(c => c.RippleDuration, 300)
        );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("--bui-color: rgba(255,0,0,1)");
        style.Should().Contain("--bui-bg-color: rgba(0,255,0,1)");
        style.Should().Contain("--bui-ripple-color: rgba(255,255,255,1)");
        style.Should().Contain("--bui-ripple-duration: 300ms");
        cut.Find("div").GetAttribute("data-bui-ripple").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Borders_Should_Handle_Shorthand_And_Complex_Overrides(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.Border, new BorderStyle("1px", BorderStyleType.Solid, new CssColor("#000"), 5))
            .Add(c => c.BorderBottom, new BorderStyle("2px", BorderStyleType.Dashed, new CssColor("#FFF"), 5))
        );

        string? style = cut.Find("div").GetAttribute("style");
        // Shorthand vars
        style.Should().Contain("--bui-border-width: 1px");
        style.Should().Contain("--bui-border-style: solid");
        style.Should().Contain("--bui-border-radius: 5px");
        // Specific vars
        style.Should().Contain("--bui-border-bottom-width: 2px");
        style.Should().Contain("--bui-border-bottom-style: dashed");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Style_Attribute_Should_Be_Removed_If_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Un componente sin nada que genere CSS vars
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>();
        cut.Find("div").HasAttribute("style").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task User_Styles_Should_Be_Preserved_And_Merged(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Dictionary<string, object> attrs = new()
        { { "style", "display: flex;" } };
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.AdditionalAttributes, attrs)
            .Add(c => c.Color, new CssColor("#FF0000"))
        );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().StartWith("--bui-color:");
        style.Should().Contain("display: flex;");
    }
}
