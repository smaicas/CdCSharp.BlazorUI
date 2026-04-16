using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Checkbox;

[Trait("Component Rendering", "BUIInputCheckbox")]
public class BUIInputCheckboxRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Accept"));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-component").Should().Be("input-checkbox");
        root.GetAttribute("data-bui-variant").Should().Be("default");
        root.GetAttribute("data-bui-size").Should().Be("medium");
        root.GetAttribute("data-bui-disabled").Should().Be("false");
        root.GetAttribute("data-bui-error").Should().Be("false");
        root.GetAttribute("data-bui-active").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Active_When_Checked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Value, true)
            .Add(c => c.Label, "Checked"));

        cut.Find("bui-component").GetAttribute("data-bui-active").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Indeterminate_For_Null_Nullable_Bool(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool?>> cut = ctx.Render<BUIInputCheckbox<bool?>>(p => p
            .Add(c => c.Label, "Indeterminate"));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-indeterminate").Should().Be("true");
        root.GetAttribute("data-bui-active").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Terms")
            .Add(c => c.HelperText, "You must accept."));

        cut.Find(".bui-checkbox__label").TextContent.Should().Contain("Terms");
        cut.Find("._bui-field-helper").TextContent.Should().Contain("You must accept.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Label_When_Not_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>();

        cut.FindAll(".bui-checkbox__label").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Required_Marker(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Accept")
            .Add(c => c.Required, true));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-required").Should().Be("true");
        cut.Find(".bui-field__required").TextContent.Should().Contain("*");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_InlineColor_When_Color_Set(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Label, "Colored")
            .Add(c => c.Color, "rgba(255,0,0,1)"));

        string style = cut.Find("bui-component").GetAttribute("style") ?? string.Empty;
        style.Should().Contain("--bui-inline-color: rgba(255,0,0,1)");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_Size_And_Density_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputCheckbox<bool>> cut = ctx.Render<BUIInputCheckbox<bool>>(p => p
            .Add(c => c.Size, SizeEnum.Large)
            .Add(c => c.Density, DensityEnum.Compact));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-size").Should().Be("large");
        root.GetAttribute("data-bui-density").Should().Be("compact");
    }
}
