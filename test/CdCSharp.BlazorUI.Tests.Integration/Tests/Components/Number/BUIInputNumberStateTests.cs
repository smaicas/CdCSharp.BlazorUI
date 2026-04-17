using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Number;

[Trait("Component State", "BUIInputNumber")]
public class BUIInputNumberStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Value, 10));

        cut.Find("input.bui-input__field").GetAttribute("value").Should().Be("10");

        cut.Render(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Value, 99));

        cut.Find("input.bui-input__field").GetAttribute("value").Should().Be("99");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-disabled").Should().Be("false");

        cut.Render(p => p.Add(c => c.Disabled, true));

        root.GetAttribute("data-bui-disabled").Should().Be("true");
        cut.Find("input.bui-input__field").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Loading_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Loading, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-loading").Should().Be("false");

        cut.Render(p => p.Add(c => c.Loading, true));

        root.GetAttribute("data-bui-loading").Should().Be("true");
        cut.Find("input.bui-input__field").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Error, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-error").Should().Be("false");

        cut.Render(p => p.Add(c => c.Error, true));

        root.GetAttribute("data-bui-error").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.ReadOnly, false));

        cut.Find("bui-component").GetAttribute("data-bui-readonly").Should().Be("false");

        cut.Render(p => p.Add(c => c.ReadOnly, true));

        cut.Find("bui-component").GetAttribute("data-bui-readonly").Should().Be("true");
        cut.Find("input.bui-input__field").HasAttribute("readonly").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Dictionary<string, object> extra = new()
        {
            { "data-testid", "qty-input" },
            { "class", "my-num" }
        };

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.AdditionalAttributes, extra));

        cut.Render(p => p
            .Add(c => c.AdditionalAttributes, extra)
            .Add(c => c.Disabled, true));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-testid").Should().Be("qty-input");
        root.ClassList.Should().Contain("my-num");
    }
}
