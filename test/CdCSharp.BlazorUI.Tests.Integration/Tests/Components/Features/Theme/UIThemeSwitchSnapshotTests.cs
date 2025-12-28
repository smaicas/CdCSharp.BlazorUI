using Bunit;
using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Theme;

[Trait("Components", "UIThemeSwitch")]
public class UIThemeSwitchSnapshotTests : TestContextBase
{
    public UIThemeSwitchSnapshotTests()
    {
        IThemeJsInterop mockThemeInterop = Substitute.For<IThemeJsInterop>();
        mockThemeInterop.GetThemeAsync().Returns("light");
        Services.AddSingleton(mockThemeInterop);
    }

    [Fact(DisplayName = "AllVariants_MatchSnapshot")]
    public Task ThemeSwitch_AllVariants_MatchSnapshot()
    {
        // Arrange
        UIThemeSwitchVariant[] variants =
        [
            UIThemeSwitchVariant.Default,
            UIThemeSwitchVariant.SunMoon
        ];

        // Act
        var results = variants.Select(variant =>
        {
            IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
                .Add(p => p.Variant, variant));

            return new { Variant = variant.Name, Html = cut.Markup };
        });

        // Assert
        return Verify(results);
    }

    [Fact(DisplayName = "AllThemes_MatchSnapshot")]
    public async Task ThemeSwitch_AllThemes_MatchSnapshot()
    {
        // Arrange
        string[] themes = { "light", "dark" };
        List<object> results = [];

        foreach (string theme in themes)
        {
            // Update mock for each theme
            IThemeJsInterop mockThemeInterop = Services.GetRequiredService<IThemeJsInterop>();
            mockThemeInterop.GetThemeAsync().Returns(theme);

            // Render default variant
            IRenderedComponent<UIThemeSwitch> defaultCut = Render<UIThemeSwitch>();
            await Task.Delay(50);

            results.Add(new
            {
                Theme = theme,
                Variant = "Default",
                Html = defaultCut.Markup
            });

            // Render SunMoon variant
            IRenderedComponent<UIThemeSwitch> sunMoonCut = Render<UIThemeSwitch>(parameters => parameters
                .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));

            results.Add(new
            {
                Theme = theme,
                Variant = "SunMoon",
                Html = sunMoonCut.Markup
            });
        }

        // Assert
        await Verify(results);
    }

    [Fact(DisplayName = "WithoutIcon_MatchSnapshot")]
    public Task ThemeSwitch_WithoutIcon_MatchSnapshot()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.ShowIcon, false));

        // Assert
        return Verify(cut.Markup);
    }

    [Fact(DisplayName = "WithCustomThemes_MatchSnapshot")]
    public async Task ThemeSwitch_WithCustomThemes_MatchSnapshot()
    {
        // Arrange
        string[] customThemes = { "red", "green", "blue" };
        Dictionary<string, string> customIcons = new()
        {
            { "red", "❤️" },
            { "green", "💚" },
            { "blue", "💙" }
        };

        IThemeJsInterop mockThemeInterop = Services.GetRequiredService<IThemeJsInterop>();
        mockThemeInterop.GetThemeAsync().Returns("green");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.AvailableThemes, customThemes)
            .Add(p => p.ThemeIcons, customIcons));
        await Task.Delay(50);

        // Assert
        await Verify(cut.Markup);
    }

    [Fact(DisplayName = "TransitionStates_MatchSnapshot")]
    public async Task ThemeSwitch_TransitionStates_MatchSnapshot()
    {
        // Arrange
        IThemeJsInterop mockThemeInterop = Services.GetRequiredService<IThemeJsInterop>();
        mockThemeInterop.ToggleThemeAsync(Arg.Any<string[]>())
             .Returns(_ => new ValueTask<string>(DelayReturn("dark", 200)));

        static async Task<string> DelayReturn(string value, int delay)
        {
            await Task.Delay(delay);
            return value;
        }

        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();

        List<object> results =
        [
            // Capture initial state
            new
            {
                State = "Initial",
                Html = cut.Markup
            }
        ];

        // Start transition
        Task clickTask = cut.Find("button").ClickAsync();
        await Task.Delay(50);

        // Capture transitioning state
        results.Add(new
        {
            State = "Transitioning",
            Html = cut.Markup
        });

        // Complete transition
        await clickTask;
        await Task.Delay(350);

        // Capture final state
        results.Add(new
        {
            State = "Final",
            Html = cut.Markup
        });

        // Assert
        await Verify(results);
    }
}