using Bunit;
using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Theme;

[Trait("Components", "UIThemeSwitch")]
public class UIThemeSwitchInteractionTests : TestContextBase
{
    private readonly IThemeJsInterop _mockThemeInterop;

    public UIThemeSwitchInteractionTests()
    {
        _mockThemeInterop = Substitute.For<IThemeJsInterop>();
        _mockThemeInterop.GetThemeAsync().Returns("light");
        Services.AddSingleton(_mockThemeInterop);
    }

    [Fact(DisplayName = "Click_TogglesTheme")]
    public async Task ThemeSwitch_Click_TogglesTheme()
    {
        // Arrange
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>()).Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();
        await cut.Find("button").ClickAsync();

        // Assert
        await _mockThemeInterop.Received(1).ToggleThemeAsync(Arg.Is<string[]>(themes =>
            themes.Length == 2 && themes[0] == "light" && themes[1] == "dark"));
    }

    [Fact(DisplayName = "Click_InvokesOnThemeChangedCallback")]
    public async Task ThemeSwitch_Click_InvokesOnThemeChangedCallback()
    {
        // Arrange
        string? changedTheme = null;
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>()).Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.OnThemeChanged, EventCallback.Factory.Create<string>(this, theme => changedTheme = theme)));

        await cut.Find("button").ClickAsync();

        // Assert
        changedTheme.Should().Be("dark");
    }

    [Fact(DisplayName = "Click_WithCustomThemes_TogglesCorrectly")]
    public async Task ThemeSwitch_Click_WithCustomThemes_TogglesCorrectly()
    {
        // Arrange
        string[] customThemes = { "theme1", "theme2", "theme3" };
        _mockThemeInterop.ToggleThemeAsync(customThemes).Returns("theme2");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.AvailableThemes, customThemes));

        await cut.Find("button").ClickAsync();

        // Assert
        await _mockThemeInterop.Received(1).ToggleThemeAsync(customThemes);
    }

    [Fact(DisplayName = "SunMoonVariant_Click_TogglesOnlyLightDark")]
    public async Task ThemeSwitch_SunMoonVariant_Click_TogglesOnlyLightDark()
    {
        // Arrange
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>()).Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon)
            .Add(p => p.AvailableThemes, new[] { "light", "dark", "custom" })); // Should be ignored

        await cut.Find("button").ClickAsync();

        // Assert
        await _mockThemeInterop.Received(1).ToggleThemeAsync(Arg.Is<string[]>(themes =>
            themes.Length == 2 && themes[0] == "light" && themes[1] == "dark"));
    }

    [Fact(DisplayName = "MultipleClicks_PreventedDuringTransition")]
    public async Task ThemeSwitch_MultipleClicks_PreventedDuringTransition()
    {
        // Arrange
        int callCount = 0;
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>())
            .Returns(_ =>
            {
                callCount++;
                return new ValueTask<string>(
                    Task.Run(async () =>
                    {
                        await Task.Delay(200);
                        return "dark";
                    })
                );
            });

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();

        Task firstClick = cut.Find("button").ClickAsync();
        await Task.Delay(50); // During transition

        // Try to click again during transition
        await cut.Find("button").ClickAsync();

        await firstClick; // Wait for first to complete
        await Task.Delay(350); // Wait for transition animation

        // Assert
        callCount.Should().Be(1, "only one toggle should occur during transition");
    }

    [Fact(DisplayName = "OnThemeChanged_NotInvoked_WhenNotProvided")]
    public async Task ThemeSwitch_OnThemeChanged_NotInvoked_WhenNotProvided()
    {
        // Arrange
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>()).Returns("dark");

        // Act & Assert - Should not throw
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();

        Func<Task> act = async () => await cut.Find("button").ClickAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "InitialTheme_LoadedOnInitialization")]
    public async Task ThemeSwitch_InitialTheme_LoadedOnInitialization()
    {
        // Arrange
        _mockThemeInterop.GetThemeAsync().Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();
        await Task.Delay(50); // Wait for initialization

        // Assert
        await _mockThemeInterop.Received(1).GetThemeAsync();

        // Verify UI reflects current theme
        cut.Find(".ui-theme-switch__label").TextContent.Should().Be("Dark");
    }

    [Fact(DisplayName = "ThemeChange_UpdatesUICorrectly")]
    public async Task ThemeSwitch_ThemeChange_UpdatesUICorrectly()
    {
        // Arrange
        _mockThemeInterop.GetThemeAsync().Returns("light");
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>()).Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();

        // Initial state
        cut.Find(".ui-theme-switch__label").TextContent.Should().Be("Light");

        await cut.Find("button").ClickAsync();
        await Task.Delay(350); // Wait for transition

        // Assert
        cut.Find(".ui-theme-switch__label").TextContent.Should().Be("Dark");
        cut.Find(".ui-theme-switch__icon").TextContent.Should().Be("🌙");
    }

    [Fact(DisplayName = "SunMoonVariant_ThemeChange_UpdatesIcon")]
    public async Task ThemeSwitch_SunMoonVariant_ThemeChange_UpdatesIcon()
    {
        // Arrange
        _mockThemeInterop.GetThemeAsync().Returns("light");
        _mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>()).Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));

        // Initial state - sun
        cut.Find(".ui-theme-switch__sun").Should().NotBeNull();

        await cut.Find("button").ClickAsync();
        await Task.Delay(350); // Wait for transition

        // Assert - moon
        cut.FindAll(".ui-theme-switch__sun").Should().BeEmpty();
        cut.Find(".ui-theme-switch__moon").Should().NotBeNull();
    }
}