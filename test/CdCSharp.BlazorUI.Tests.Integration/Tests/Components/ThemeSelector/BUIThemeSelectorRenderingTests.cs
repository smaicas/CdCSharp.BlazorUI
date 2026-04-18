using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Rendering", "BUIThemeSelector")]
public class BUIThemeSelectorRenderingTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx, string currentTheme = "light")
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>(currentTheme));
        fake.ToggleThemeAsync(Arg.Any<string[]>()).Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("theme-selector");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant_Button(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>();

        // Assert
        cut.Find("button").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Theme_Icon_When_ShowIcon(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx, "light");

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>(p => p
            .Add(c => c.ShowIcon, true));

        // Assert
        cut.Find(".bui-theme-switch__icon").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Render_Icon_When_ShowIcon_False(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>(p => p
            .Add(c => c.ShowIcon, false));

        // Assert
        cut.FindAll(".bui-theme-switch__icon").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Default_Variant_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>();

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
    }
}
