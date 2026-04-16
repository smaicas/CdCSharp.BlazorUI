using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Forms;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Switch;

[Trait("Component Accessibility", "BUIInputSwitch")]
public class BUIInputSwitchAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Switch_Role(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>();

        cut.Find("input.bui-switch__input").GetAttribute("role").Should().Be("switch");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaChecked_Matching_Value(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Value, false));

        cut.Find("input.bui-switch__input").GetAttribute("aria-checked").Should().Be("false");

        cut.Render(p => p.Add(c => c.Value, true));

        cut.Find("input.bui-switch__input").GetAttribute("aria-checked").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Emit_AriaLabel_Toggle_When_No_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>();

        cut.Find("input.bui-switch__input").GetAttribute("aria-label").Should().Be("Toggle");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Emit_AriaLabel_When_Label_Provided(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Label, "Dark mode"));

        // Label acts as accessible name via `for` association — aria-label should be null
        cut.Find("input.bui-switch__input").GetAttribute("aria-label").Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Associate_Label_With_Input_Via_For(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Label, "Feature"));

        IElement input = cut.Find("input.bui-switch__input");
        string? inputId = input.GetAttribute("id");
        inputId.Should().NotBeNullOrWhiteSpace();

        IElement label = cut.Find("label.bui-switch");
        label.GetAttribute("for").Should().Be(inputId);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Link_AriaDescribedBy_To_Helper(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.HelperText, "Enable notifications."));

        string? describedBy = cut.Find("input.bui-switch__input").GetAttribute("aria-describedby");
        describedBy.Should().NotBeNullOrWhiteSpace();

        IElement helper = cut.Find("._bui-field-helper");
        helper.GetAttribute("id").Should().Be(describedBy);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Input_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        IRenderedComponent<BUIInputSwitch> cut = ctx.Render<BUIInputSwitch>(p => p
            .Add(c => c.Disabled, true));

        cut.Find("input.bui-switch__input").HasAttribute("disabled").Should().BeTrue();
    }
}
