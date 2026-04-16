using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Text;

[Trait("Component Interaction", "BUIInputText")]
public class BUIInputTextInteractionTests
{
    private class Model { public string? Value { get; set; } }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Update_Value_On_Change(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        string? captured = null;
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v));

        // Act
        cut.Find("input.bui-input__field").Change("hello");

        // Assert
        captured.Should().Be("hello");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Commit_Value_On_Input_When_UpdateOnInput_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        string? captured = null;
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v)
            .Add(c => c.UpdateOnInput, false));

        // Act
        cut.Find("input.bui-input__field").Input("typing");

        // Assert
        captured.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Commit_Value_On_Input_When_UpdateOnInput_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        string? captured = null;
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.ValueChanged, v => captured = v)
            .Add(c => c.UpdateOnInput, true));

        // Act
        cut.Find("input.bui-input__field").Input("typing");

        // Assert
        captured.Should().Be("typing");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnInput_Callback_With_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        string? captured = null;
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.ValueExpression, () => model.Value)
            .Add(c => c.OnInput, v => captured = v));

        // Act
        cut.Find("input.bui-input__field").Input("abc");

        // Assert
        captured.Should().Be("abc");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Float_Label_On_Focus_And_Collapse_On_Blur_When_Empty(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement root = cut.Find("bui-component");
        IElement input = cut.Find("input.bui-input__field");
        root.GetAttribute("data-bui-floated").Should().Be("false");

        // Act - focus
        input.Focus();

        // Assert floated while focused
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");

        // Act - blur (empty value)
        cut.Find("input.bui-input__field").Blur();

        // Assert collapsed back
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("false");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Keep_Label_Floated_On_Blur_When_HasValue(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new() { Value = "filled" };
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Value, "filled")
            .Add(c => c.Label, "Name")
            .Add(c => c.ValueExpression, () => model.Value));

        IElement input = cut.Find("input.bui-input__field");

        // Act
        input.Focus();
        input.Blur();

        // Assert - still floated because HasValue
        cut.Find("bui-component").GetAttribute("data-bui-floated").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Expose_Placeholder_While_Floated(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        Model model = new();
        IRenderedComponent<BUIInputText> cut = ctx.Render<BUIInputText>(p => p
            .Add(c => c.Label, "Name")
            .Add(c => c.Placeholder, "type here")
            .Add(c => c.ValueExpression, () => model.Value));

        // Initially not floated -> placeholder attr should be null
        cut.Find("input.bui-input__field").GetAttribute("placeholder").Should().BeNull();

        // Act
        cut.Find("input.bui-input__field").Focus();

        // Assert
        cut.Find("input.bui-input__field").GetAttribute("placeholder").Should().Be("type here");
    }
}
