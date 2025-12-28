using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Theme;

[Trait("Components", "UIThemeSwitch")]
public class UIThemeSwitchRenderTests : TestContextBase
{
    private readonly IThemeJsInterop _mockThemeInterop;

    public UIThemeSwitchRenderTests()
    {
        _mockThemeInterop = Substitute.For<IThemeJsInterop>();
        _mockThemeInterop.GetThemeAsync().Returns("light");
        Services.AddSingleton(_mockThemeInterop);
    }

    [Fact(DisplayName = "DefaultVariant_RendersCorrectStructure")]
    public void ThemeSwitch_DefaultVariant_RendersCorrectStructure()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();

        // Assert
        IElement button = cut.Find("button");
        button.Should().NotBeNull();
        button.ShouldHaveClass("ui-theme-switch");
        button.ShouldHaveClass("ui-theme-switch--default");

        IElement content = cut.Find(".ui-theme-switch__content");
        content.Should().NotBeNull();

        // Default shows icon
        IElement icon = cut.Find(".ui-theme-switch__icon");
        icon.Should().NotBeNull();

        IElement label = cut.Find(".ui-theme-switch__label");
        label.Should().NotBeNull();
        label.TextContent.Should().Be("Light");
    }

    [Fact(DisplayName = "SunMoonVariant_RendersCorrectStructure")]
    public void ThemeSwitch_SunMoonVariant_RendersCorrectStructure()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-theme-switch");
        button.ShouldHaveClass("ui-theme-switch--sunmoon");
        button.ShouldHaveClass("ui-theme-switch--light");

        IElement container = cut.Find(".ui-theme-switch__sun-moon-container");
        container.Should().NotBeNull();

        IElement sun = cut.Find(".ui-theme-switch__sun");
        sun.Should().NotBeNull();
        sun.TextContent.Should().Be("☀️");
    }

    [Fact(DisplayName = "WithDarkTheme_RendersCorrectContent")]
    public async Task ThemeSwitch_WithDarkTheme_RendersCorrectContent()
    {
        // Arrange
        _mockThemeInterop.GetThemeAsync().Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();
        await Task.Delay(50); // Wait for initialization

        // Assert
        IElement label = cut.Find(".ui-theme-switch__label");
        label.TextContent.Should().Be("Dark");
    }

    [Fact(DisplayName = "WithoutIcon_DoesNotRenderIcon")]
    public void ThemeSwitch_WithoutIcon_DoesNotRenderIcon()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.ShowIcon, false));

        // Assert
        cut.FindAll(".ui-theme-switch__icon").Should().BeEmpty();

        // Label should still be present
        IElement label = cut.Find(".ui-theme-switch__label");
        label.Should().NotBeNull();
    }

    [Fact(DisplayName = "WithCustomThemes_RendersCorrectTheme")]
    public async Task ThemeSwitch_WithCustomThemes_RendersCorrectTheme()
    {
        // Arrange
        string[] customThemes = { "theme1", "theme2", "theme3" };
        _mockThemeInterop.GetThemeAsync().Returns("theme2");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.AvailableThemes, customThemes));
        await Task.Delay(50);

        // Assert
        IElement label = cut.Find(".ui-theme-switch__label");
        label.TextContent.Should().Be("Theme2");
    }

    [Fact(DisplayName = "WithCustomThemeIcons_RendersCustomIcon")]
    public void ThemeSwitch_WithCustomThemeIcons_RendersCustomIcon()
    {
        // Arrange
        Dictionary<string, string> customIcons = new()
        {
            { "light", "💡" },
            { "dark", "🌚" }
        };

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.ThemeIcons, customIcons));

        // Assert
        IElement icon = cut.Find(".ui-theme-switch__icon");
        icon.TextContent.Should().Be("💡");
    }

    [Fact(DisplayName = "WithAdditionalAttributes_MergesCorrectly")]
    public void ThemeSwitch_WithAdditionalAttributes_MergesCorrectly()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "data-test-id", "theme-switch" },
                { "class", "custom-class" }
            }));

        // Assert
        IElement button = cut.Find("button");
        button.GetAttribute("data-test-id").Should().Be("theme-switch");
        button.ShouldHaveClass("ui-theme-switch");
        button.ShouldHaveClass("ui-theme-switch--default");
        button.ShouldHaveClass("custom-class");
    }

    [Fact(DisplayName = "TransitioningState_AppliesCorrectClass")]
    public async Task ThemeSwitch_TransitioningState_AppliesCorrectClass()
    {
        // Arrange
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>())
            .Returns(_ => new ValueTask<string>(DelayReturn("dark", 100)));

        static async Task<string> DelayReturn(string value, int delay)
        {
            await Task.Delay(delay);
            return value;
        }

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();
        Task clickTask = cut.Find("button").ClickAsync();

        // Wait a bit for transition to start
        await Task.Delay(50);

        // Assert - during transition
        IElement content = cut.Find(".ui-theme-switch__content");
        content.ShouldHaveClass("ui-theme-switch__content--transitioning");

        // Wait for transition to complete
        await clickTask;
        await Task.Delay(350);

        // Assert - after transition
        content = cut.Find(".ui-theme-switch__content");
        content.ShouldNotHaveClass("ui-theme-switch__content--transitioning");
    }

    [Fact(DisplayName = "SunMoonVariant_WithDarkTheme_ShowsMoon")]
    public async Task ThemeSwitch_SunMoonVariant_WithDarkTheme_ShowsMoon()
    {
        // Arrange
        _mockThemeInterop.GetThemeAsync().Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));
        await Task.Delay(50);

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-theme-switch--dark");

        cut.FindAll(".ui-theme-switch__sun").Should().BeEmpty();
        IElement moon = cut.Find(".ui-theme-switch__moon");
        moon.Should().NotBeNull();
        moon.TextContent.Should().Be("🌙");
    }

    [Fact(DisplayName = "AccessibilityAttributes_SetCorrectly")]
    public void ThemeSwitch_AccessibilityAttributes_SetCorrectly()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));

        // Assert
        IElement button = cut.Find("button");
        button.GetAttribute("aria-label").Should().Be("Switch to dark mode");
    }

    [Fact(DisplayName = "DisabledDuringTransition_PreventsInteraction")]
    public async Task ThemeSwitch_DisabledDuringTransition_PreventsInteraction()
    {
        // Arrange
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>())
            .Returns(_ => new ValueTask<string>(
                    Task.Run(async () =>
                    {
                        await Task.Delay(200);
                        return "dark";
                    })
                ));

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();
        Task clickTask = cut.Find("button").ClickAsync();

        await Task.Delay(50);

        // Assert
        IElement button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();

        // Complete the transition
        await clickTask;
        await Task.Delay(350);

        button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeFalse();
    }
}