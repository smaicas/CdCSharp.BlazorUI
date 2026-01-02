using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Localization.Abstractions;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using BUICultureSelector = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector;
using BUICultureSelectorVariant = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelectorVariant;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component", "BUICultureSelector")]
[Trait("Pillar", "Rendering")]
public class BUICultureSelectorRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Dropdown_Variant_With_Options(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        LocalizationSettings localizationSettings = ctx.Services.GetRequiredService<LocalizationSettings>();

        // Act
        Bunit.IRenderedComponent<BUICultureSelector> cut = ctx.Render<BUICultureSelector>(p => p
            .Add(c => c.Variant, BUICultureSelectorVariant.Dropdown));

        // Assert
        IElement select = cut.Find("select");
        select.Should().NotBeNull();

        IReadOnlyList<IElement> options = cut.FindAll("option");
        options.Count.Should().Be(localizationSettings.SupportedCultures.Count);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Flags_When_ShowFlag_Is_True(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Act
        Bunit.IRenderedComponent<BUICultureSelector> cut = ctx.Render<BUICultureSelector>(p => p
            .Add(c => c.Variant, BUICultureSelectorVariant.Flags)
            .Add(c => c.ShowFlag, true));

        // Assert
        cut.Markup.Should().Contain("🇺🇸"); // en-US flag
        cut.Markup.Should().Contain("🇪🇸"); // es-ES flag
    }
}
