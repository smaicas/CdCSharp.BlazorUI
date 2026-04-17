using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Button;

[Trait("Component Accessibility", "BUIButton")]
public class BUIButtonAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Disabled_On_Inner_Button_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Click me")
            .Add(c => c.Disabled, true));

        // Assert — HTML disabled attribute on the inner <button>
        cut.Find("button").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Data_Bui_Disabled_On_Root_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Click me")
            .Add(c => c.Disabled, true));

        // Assert — data-bui-disabled on root element
        cut.Find("bui-component").GetAttribute("data-bui-disabled").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Set_Data_Bui_Loading_On_Root_When_Loading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Loading")
            .Add(c => c.Loading, true));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-loading").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Inner_Button_When_Loading(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — Loading also disables the button
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Loading")
            .Add(c => c.Loading, true));

        // Assert — loading makes button disabled
        cut.Find("button").HasAttribute("disabled").Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Pass_Aria_Label_To_Root_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — aria-label passes through AdditionalAttributes to bui-component root
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Icon button")
            .AddUnmatched("aria-label", "Close dialog"));

        // Assert
        cut.Find("bui-component").GetAttribute("aria-label").Should().Be("Close dialog");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_Click_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int clickCount = 0;
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Disabled")
            .Add(c => c.Disabled, true)
            .Add(c => c.OnClick, _ => clickCount++));

        // Act — click on a disabled button should not fire the callback
        try { cut.Find("button").Click(); } catch { /* bunit may throw for disabled */ }

        // Assert
        clickCount.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Button_With_Type_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIButton> cut = ctx.Render<BUIButton>(p => p
            .Add(c => c.Text, "Submit"));

        // Assert — type="button" prevents accidental form submission
        cut.Find("button").GetAttribute("type").Should().Be("button");
    }
}
