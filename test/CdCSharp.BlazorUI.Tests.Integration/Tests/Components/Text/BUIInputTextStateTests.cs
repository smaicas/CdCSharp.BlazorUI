using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Text;

[Trait("Component State", "BUIInputText")]
public class BUIInputTextStateTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reflect_Value_Changes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Value, "initial")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bui-input__field").GetAttribute("value").Should().Be("initial");

        cut.Render(p => p
            .Add(c => c.Value, "updated")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("input.bui-input__field").GetAttribute("value").Should().Be("updated");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Disabled_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-disabled").Should().Be("false");
        cut.Find("input").HasAttribute("disabled").Should().BeFalse();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Disabled, true));

        root.GetAttribute("data-bui-disabled").Should().Be("true");
        cut.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_ReadOnly_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, true));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-readonly").Should().Be("true");
        cut.Find("input").HasAttribute("readonly").Should().BeTrue();

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ReadOnly, false));

        root.GetAttribute("data-bui-readonly").Should().Be("false");
        cut.Find("input").HasAttribute("readonly").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Required_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Required, true));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-required").Should().Be("true");
        cut.Find("input").HasAttribute("required").Should().BeTrue();
        cut.FindAll(".bui-input__required").Should().NotBeEmpty();

        cut.Render(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Required, false));

        root.GetAttribute("data-bui-required").Should().Be("false");
        cut.Find("input").HasAttribute("required").Should().BeFalse();
        cut.FindAll(".bui-input__required").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Error_Via_Explicit_Parameter(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-error").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Error, true));

        root.GetAttribute("data-bui-error").Should().Be("true");
        cut.Find("input").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Toggle_Loading_State(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, false));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-bui-loading").Should().Be("false");

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.Loading, true));

        root.GetAttribute("data-bui-loading").Should().Be("true");
        // Loading forces disabled via IsDisabled computed prop.
        cut.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Label_Helper_Placeholder(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "Old label")
            .Add(c => c.HelperText, "Old helper")
            .Add(c => c.Placeholder, "Old placeholder")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("label.bui-input__label").TextContent.Should().Contain("Old label");
        cut.Find("._bui-field-helper").TextContent.Should().Contain("Old helper");

        cut.Render(p => p
            .Add(c => c.Label, "New label")
            .Add(c => c.HelperText, "New helper")
            .Add(c => c.Placeholder, "New placeholder")
            .Add(c => c.ValueExpression, () => model.Value));

        cut.Find("label.bui-input__label").TextContent.Should().Contain("New label");
        cut.Find("._bui-field-helper").TextContent.Should().Contain("New helper");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Preserve_User_Additional_Attributes(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Model model = new();
        Dictionary<string, object> extra = new()
        {
            { "data-testid", "email-field" },
            { "class", "my-class" },
            { "style", "margin: 4px;" }
        };

        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.AdditionalAttributes, extra));

        cut.Render(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.AdditionalAttributes, extra)
            .Add(c => c.Disabled, true)
            .Add(c => c.Size, BUISize.Small));

        IElement root = cut.Find("bui-component");
        root.GetAttribute("data-testid").Should().Be("email-field");
        root.ClassList.Should().Contain("my-class");
        root.GetAttribute("style").Should().Contain("margin: 4px;");
    }
}
