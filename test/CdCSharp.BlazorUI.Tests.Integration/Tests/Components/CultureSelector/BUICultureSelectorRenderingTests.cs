using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServerSelector = CdCSharp.BlazorUI.Components.Server.BUICultureSelector;
using ServerSettings = CdCSharp.BlazorUI.Localization.Server.LocalizationSettings;
using ServerVariant = CdCSharp.BlazorUI.Components.Server.BUICultureSelectorVariant;
using WasmSelector = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector;
using WasmSettings = CdCSharp.BlazorUI.Localization.Wasm.LocalizationSettings;
using WasmVariant = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelectorVariant;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Rendering", "BUICultureSelector")]
public class BUICultureSelectorRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dropdown_With_Select_Element(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, ServerVariant.Dropdown)).Markup
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, WasmVariant.Dropdown)).Markup;

        // Assert
        markup.Should().Contain("<select");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Options_For_Each_Supported_Culture(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        int expectedCount = scenario.Name == "Server"
            ? ctx.Services.GetRequiredService<ServerSettings>().SupportedCultures.Count
            : ctx.Services.GetRequiredService<WasmSettings>().SupportedCultures.Count;

        IReadOnlyList<IElement> options = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p.Add(c => c.Variant, ServerVariant.Dropdown)).FindAll("option")
            : ctx.Render<WasmSelector>(p => p.Add(c => c.Variant, WasmVariant.Dropdown)).FindAll("option");

        // Assert
        options.Should().HaveCount(expectedCount);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Flags_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IReadOnlyList<IElement> buttons = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Flags)
                .Add(c => c.ShowFlag, true)).FindAll("button")
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Flags)
                .Add(c => c.ShowFlag, true)).FindAll("button");

        // Assert
        buttons.Should().HaveCountGreaterThan(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Flag_Emoji_When_ShowFlag_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Flags)
                .Add(c => c.ShowFlag, true)).Markup
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Flags)
                .Add(c => c.ShowFlag, true)).Markup;

        // Assert
        markup.Should().Contain("🇺🇸");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Culture_Name_When_ShowName_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        string markup = scenario.Name == "Server"
            ? ctx.Render<ServerSelector>(p => p
                .Add(c => c.Variant, ServerVariant.Flags)
                .Add(c => c.ShowFlag, false)
                .Add(c => c.ShowName, true)).Markup
            : ctx.Render<WasmSelector>(p => p
                .Add(c => c.Variant, WasmVariant.Flags)
                .Add(c => c.ShowFlag, false)
                .Add(c => c.ShowName, true)).Markup;

        // Assert
        markup.Should().Contain("English");
    }
}
