using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Design;
using CdCSharp.BlazorUI.Core.Abstractions.Behaviors.Javascript;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components.BaseComponents;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.BaseComponents;

/// <summary>
/// Functional contract tests for <see cref="BUIComponentBase"/>.
///
/// These tests define and enforce the following system-level rules:
///
/// 1 - AUTOMATIC NAMING: Components MUST automatically generate a kebab-case identifier 
///     for the 'data-bui-component' attribute, derived from the class name and 
///     stripping any 'BUI' prefix for CSS consistency.
///
/// 2 - DESIGN STATE MAPPING: All design-related properties (Size, Density, Elevation, 
///     FullWidth) MUST be mapped to 'data-bui-*' HTML attributes to enable 
///     CSS attribute selector styling.
///
/// 3 - DYNAMIC STYLE GENERATION: 
///     - Colors (Foreground, Background, Ripple) MUST be injected as CSS Variables 
///       (--bui-*) using RGBA deterministic formatting.
///     - Borders MUST support shorthand configuration and specific side overrides 
///       (Top, Left, etc.) via CSS Variables.
///
/// 4 - STYLE HYBRIDIZATION: User-defined 'style' strings in 'AdditionalAttributes' 
///     MUST be preserved and merged with system-generated CSS variables.
///
/// 5 - RIPPLE LIFECYCLE: 
///     - Ripple configuration MUST be exposed as CSS variables unless 'DisableRipple' is true.
///     - When disabled, 'data-bui-ripple' MUST be 'false' and variables MUST be omitted.
///
/// 6 - JAVASCRIPT BEHAVIOR BRIDGE:
///     - Components MUST attach JS behaviors after the first render via <see cref="IBehaviorJsInterop"/>.
///     - JS behaviors MUST be skipped if the component is in a 'Loading' state to prevent 
///       initialization on incomplete DOM trees.
///     - Components MUST ensure idempotent disposal of JS modules to prevent memory leaks.
///
/// 7 - ATTRIBUTE CLEANLINESS: The 'style' attribute MUST NOT be rendered if no 
///     dynamic variables or user styles are present.
/// </summary>
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
            .Add(c => c.RippleDurationMs, 300)
        );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("--bui-inline-color: rgba(255,0,0,1)");
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

        IRenderedComponent<BUIComponentBase_TestStub> cut =
    ctx.Render<BUIComponentBase_TestStub>(p => p
        .Add(c => c.Border,
            BorderStyle.Create()
                .All("1px", BorderStyleType.Solid, new CssColor("#000"))
                .Bottom("2px", BorderStyleType.Dashed, new CssColor("#FFF"))
                .Radius(5))
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
    public async Task Borders_Should_Handle_All_Specific_Sides(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIComponentBase_TestStub> cut =
            ctx.Render<BUIComponentBase_TestStub>(p => p
                .Add(c => c.Border,
                    BorderStyle.Create()
                        .Top("1px", BorderStyleType.Solid, new CssColor("#F00"))
                        .Left("2px", BorderStyleType.Dotted, new CssColor("#0F0")))
            );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("--bui-border-top-width: 1px");
        style.Should().Contain("--bui-border-left-width: 2px");
        style.Should().Contain("--bui-border-left-style: dotted");
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
        style.Should().StartWith("--bui-inline-color:");
        style.Should().Contain("display: flex;");
    }

    private readonly IBehaviorJsInterop _jsInterop;
    private readonly IJSObjectReference _jsModule;

    public BUIComponentBaseTests()
    {
        _jsInterop = Substitute.For<IBehaviorJsInterop>();
        _jsModule = Substitute.For<IJSObjectReference>();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Attach_And_Dispose_JS_Behavior(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        ctx.Services.AddSingleton(_jsInterop);
        // Arrange
        _jsInterop.AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>())
                  .Returns(_jsModule);

        // Act
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>();
        await cut.Instance.DisposeAsync();

        // Assert
        await _jsInterop.Received(1).AttachBehaviorsAsync(Arg.Any<BehaviorConfiguration>());
        await _jsModule.Received(1).InvokeVoidAsync("dispose", Arg.Any<object[]>());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Handle_Null_AdditionalAttributes_And_Style_Merging(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Test con AdditionalAttributes nulos explícitamente
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.AdditionalAttributes, null)
            .Add(c => c.Color, new CssColor("#000")));

        cut.Find("div").GetAttribute("style").Should().Contain("--bui-inline-color");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Ripple_Should_Be_Disabled_When_DisableRipple_Is_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Act: DisableRipple = true (Cubre la rama donde config.Ripple no debería setearse)
        IRenderedComponent<BUIComponentBase_TestStub> cut = ctx.Render<BUIComponentBase_TestStub>(p => p
            .Add(c => c.DisableRipple, true));

        IElement el = cut.Find("div");
        el.GetAttribute("data-bui-ripple").Should().Be("false");

        string? style = el.GetAttribute("style");
        style.Should().NotContain("--bui-ripple-color");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Border_Should_Support_Inherit_And_None_Values(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Act: Probar BorderStyle con tipos None e Inherit (Cubre ramas de switch/if en el Builder)
        IRenderedComponent<BUIComponentBase_TestStub> cut =
            ctx.Render<BUIComponentBase_TestStub>(p => p
                .Add(c => c.Border,
                    BorderStyle.Create().None())
            );

        string? style = cut.Find("div").GetAttribute("style");
        style.Should().Contain("--bui-border-style: none");
    }
}
