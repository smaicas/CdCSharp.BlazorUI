using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using System.Globalization;
using ServerSelector = CdCSharp.BlazorUI.Components.Server.BUICultureSelector;
using ServerVariant = CdCSharp.BlazorUI.Components.Server.BUICultureSelectorVariant;
using WasmSelector = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector;
using WasmVariant = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelectorVariant;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Interaction", "BUICultureSelector")]
public class BUICultureSelectorInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnCultureChanged_When_Dropdown_Changes_To_Different_Culture(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        CultureInfo? captured = null;

        if (scenario.Name == "Server")
        {
            ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Dropdown)
                .Add(c => c.OnCultureChanged, (CultureInfo ci) => captured = ci))
                .Find("select").Change("es-ES");
        }
        else
        {
            ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Dropdown)
                .Add(c => c.OnCultureChanged, (CultureInfo ci) => captured = ci))
                .Find("select").Change("es-ES");
        }

        // Assert
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("es-ES");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Fire_OnCultureChanged_For_Same_Culture(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int callCount = 0;
        string currentCulture = CultureInfo.CurrentUICulture.Name;

        if (scenario.Name == "Server")
        {
            ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Dropdown)
                .Add(c => c.OnCultureChanged, (CultureInfo _) => callCount++))
                .Find("select").Change(currentCulture);
        }
        else
        {
            ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Dropdown)
                .Add(c => c.OnCultureChanged, (CultureInfo _) => callCount++))
                .Find("select").Change(currentCulture);
        }

        // Assert — no culture change event for same culture
        callCount.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnCultureChanged_When_Non_Active_Flag_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        CultureInfo? captured = null;
        IReadOnlyList<IElement> buttons;

        if (scenario.Name == "Server")
        {
            IRenderedComponent<ServerSelector> cut = ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Flags)
                .Add(c => c.OnCultureChanged, (CultureInfo ci) => captured = ci));
            buttons = cut.FindAll("button");
        }
        else
        {
            IRenderedComponent<WasmSelector> cut = ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Flags)
                .Add(c => c.OnCultureChanged, (CultureInfo ci) => captured = ci));
            buttons = cut.FindAll("button");
        }

        // Act — click a non-disabled (non-active) button
        IElement? clickable = buttons.FirstOrDefault(b => b.GetAttribute("disabled") == null);
        clickable?.Click();

        // Assert — callback fired if there was a non-active button
        if (clickable != null)
        {
            captured.Should().NotBeNull();
        }
    }
}
