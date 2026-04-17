using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Color;

[Trait("Component State", "BUIInputColor")]
public class BUIInputColorStateTests
{
    private class Model { public CssColor? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-disabled").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, true));

        root.GetAttribute("data-bui-disabled").Should().Be("true");
        cut.Find("input.bui-input__field").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, true));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-readonly").Should().Be("true");
        cut.Find("input.bui-input__field").HasAttribute("readonly").Should().BeTrue();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, false));

        root.GetAttribute("data-bui-readonly").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Loading_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-loading").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, true));

        root.GetAttribute("data-bui-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-error").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, true));

        root.GetAttribute("data-bui-error").Should().Be("true");
        cut.Find("input.bui-input__field").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Preview_When_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.Value, new CssColor("#0000ff"))
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Open_Picker_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputColor> cut = ctx.Render<BUIInputColor>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, true));

        cut.FindAll(".bui-input-color__dropdown").Should().BeEmpty();
        cut.FindAll(".bui-input-color__dropdown-overlay").Should().BeEmpty();
    }
}
