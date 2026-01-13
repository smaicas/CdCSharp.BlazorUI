using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using BUICultureSelector = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector;
using BUICultureSelectorVariant = CdCSharp.BlazorUI.Components.Wasm.BUICultureSelectorVariant;
using LocalizationSettings = CdCSharp.BlazorUI.Localization.Wasm.LocalizationSettings;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.CultureSelector;

[Trait("Component Rendering", "BUICultureSelector")]
public class Wasm_BUICultureSelectorRenderingTests : TestFixtureBase<WasmTestContext>
{
    public Wasm_BUICultureSelectorRenderingTests(WasmTestContext context) : base(context)
    {
    }

    [Fact]
    public async Task Should_Render_Dropdown_Variant_With_Options()
    {
        // Arrange
        LocalizationSettings localizationSettings = Context.Services.GetRequiredService<LocalizationSettings>();

        // Act
        Bunit.IRenderedComponent<BUICultureSelector> cut = Context.Render<BUICultureSelector>(p => p
            .Add(c => c.Variant, BUICultureSelectorVariant.Dropdown));

        // Assert
        IElement select = cut.Find("select");
        select.Should().NotBeNull();

        IReadOnlyList<IElement> options = cut.FindAll("option");
        options.Count.Should().Be(localizationSettings.SupportedCultures.Count);
    }

    [Fact]
    public async Task Should_Render_Flags_When_ShowFlag_Is_True()
    {
        // Act
        Bunit.IRenderedComponent<BUICultureSelector> cut = Context.Render<BUICultureSelector>(p => p
            .Add(c => c.Variant, BUICultureSelectorVariant.Flags)
            .Add(c => c.ShowFlag, true));

        // Assert
        cut.Markup.Should().Contain("🇺🇸"); // en-US flag
        cut.Markup.Should().Contain("🇪🇸"); // es-ES flag
    }
}