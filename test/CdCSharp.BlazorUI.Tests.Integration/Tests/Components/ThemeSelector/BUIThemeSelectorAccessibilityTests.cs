using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Accessibility", "BUIThemeSelector")]
public class BUIThemeSelectorAccessibilityTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx, string theme = "light")
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>(theme));
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Button_Be_Present_And_Enabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>();

        // Assert
        cut.Find("button").HasAttribute("disabled").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Theme_Label_Be_Visible(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx, "light");

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>();

        // Assert
        cut.Find(".bui-theme-switch__label").Should().NotBeNull();
    }
}
