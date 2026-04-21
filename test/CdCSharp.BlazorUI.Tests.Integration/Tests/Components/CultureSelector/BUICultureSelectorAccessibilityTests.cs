using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using ServerSelector = CdCSharp.BlazorUI.Components.Server.BUICultureSelector;
using ServerVariant = CdCSharp.BlazorUI.Components.Server.BUICultureSelectorVariant;
using WasmSelector = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector;
using WasmVariant = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelectorVariant;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Accessibility", "BUICultureSelector")]
public class BUICultureSelectorAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Title_On_Flag_Buttons(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> buttons = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, ServerVariant.Flags))
                  .FindAll(".bui-culture-selector__flag-button")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, WasmVariant.Flags))
                  .FindAll(".bui-culture-selector__flag-button");

        // Assert
        buttons.Should().NotBeEmpty();
        foreach (IElement btn in buttons)
        {
            btn.GetAttribute("title").Should().NotBeNullOrEmpty();
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Disable_Current_Culture_Flag_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> disabledButtons = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, ServerVariant.Flags))
                  .FindAll("button[disabled]")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, WasmVariant.Flags))
                  .FindAll("button[disabled]");

        // Assert
        disabledButtons.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Use_Native_Select_For_Dropdown(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IElement select = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, ServerVariant.Dropdown)).Find("select")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, WasmVariant.Dropdown)).Find("select");

        // Assert
        select.Should().NotBeNull();
    }
}
