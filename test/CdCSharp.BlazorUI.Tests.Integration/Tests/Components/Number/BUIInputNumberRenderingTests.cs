using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Number;

[Trait("Component Rendering", "BUIInputNumber")]
public class BUIInputNumberRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_With_Base_DataAttributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Label, "Quantity"));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-component").Should().Be("input-number");
        root.GetAttribute("data-bui-input-base").Should().NotBeNull();
        root.GetAttribute("data-bui-variant").Should().Be("outlined");
        root.GetAttribute("data-bui-size").Should().Be("medium");
        root.GetAttribute("data-bui-density").Should().Be("standard");
        root.GetAttribute("data-bui-disabled").Should().Be("false");
        root.GetAttribute("data-bui-floated").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Step_Buttons_By_Default(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Label, "Count"));

        cut.Find(".bui-input__step-buttons").Should().NotBeNull();
        cut.Find("bui-component").GetAttribute("data-bui-button-placement").Should().Be("right");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Hide_Step_Buttons_When_ShowStepButtons_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.ShowStepButtons, false));

        cut.FindAll(".bui-input__step-buttons").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Label_And_HelperText(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Label, "Amount")
            .Add(c => c.HelperText, "Enter a number."));

        cut.Find("label.bui-input__label").TextContent.Should().Contain("Amount");
        cut.Find("._bui-field-helper").TextContent.Should().Contain("Enter a number.");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Prefix_And_Suffix(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<decimal>> cut = ctx.Render<BUIInputNumber<decimal>>(p => p
            .Add(c => c.PrefixText, "$")
            .Add(c => c.SuffixText, "USD"));

        cut.Find(".bui-input__addon--prefix").TextContent.Should().Contain("$");
        cut.Find(".bui-input__addon--suffix").TextContent.Should().Contain("USD");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Floated_When_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Label, "Qty")
            .Add(c => c.Value, 42));

        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_ButtonPlacement_Left(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.ButtonPlacement, StepButtonPlacement.Left));

        cut.Find("bui-component").GetAttribute("data-bui-button-placement").Should().Be("left");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Design_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputNumber<int>> cut = ctx.Render<BUIInputNumber<int>>(p => p
            .Add(c => c.Size, SizeEnum.Large)
            .Add(c => c.Density, DensityEnum.Compact)
            .Add(c => c.Color, "rgba(10,20,30,1)"));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-size").Should().Be("large");
        root.GetAttribute("data-bui-density").Should().Be("compact");
        root.GetAttribute("style").Should().Contain("--bui-inline-color: rgba(10,20,30,1)");
    }
}
