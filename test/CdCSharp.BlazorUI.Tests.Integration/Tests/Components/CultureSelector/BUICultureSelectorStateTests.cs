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

[Trait("Component State", "BUICultureSelector")]
public class BUICultureSelectorStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Flags_In_Dropdown_When_ShowFlag_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act — ShowFlag=false in Dropdown hides flag emoji from options
        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Dropdown)
                .Add(c => c.ShowFlag, false)).Markup
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Dropdown)
                .Add(c => c.ShowFlag, false)).Markup;

        // Assert — no flag emojis in option text
        markup.Should().NotContain("🇺🇸");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Without_Names_When_ShowName_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> labels = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Flags)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, false)).FindAll(".ui-culture-selector__flag-label")
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Flags)
                .Add(c => c.ShowFlag, true)
                .Add(c => c.ShowName, false)).FindAll(".ui-culture-selector__flag-label");

        // Assert
        labels.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Mark_Current_Culture_Button_As_Active(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> activeButtons = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, ServerVariant.Flags)).FindAll("button.active")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, WasmVariant.Flags)).FindAll("button.active");

        // Assert
        activeButtons.Should().HaveCount(1);
        activeButtons[0].GetAttribute("disabled").Should().NotBeNull();
    }
}
