using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Theme;

[Trait("Components", "UIThemeSwitch")]
public class UIThemeSwitchVariantTests : TestContextBase
{
    public UIThemeSwitchVariantTests()
    {
        IThemeJsInterop mockThemeInterop = Substitute.For<IThemeJsInterop>();
        mockThemeInterop.GetThemeAsync().Returns("light");
        Services.AddSingleton(mockThemeInterop);
    }

    [Fact(DisplayName = "DefaultVariant_RendersCorrectly")]
    public void ThemeSwitch_DefaultVariant_RendersCorrectly()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-theme-switch");
        button.ShouldHaveClass("ui-theme-switch--default");

        // Has icon and label
        cut.Find(".ui-theme-switch__icon").Should().NotBeNull();
        cut.Find(".ui-theme-switch__label").Should().NotBeNull();
    }

    [Fact(DisplayName = "SunMoonVariant_RendersCorrectly")]
    public void ThemeSwitch_SunMoonVariant_RendersCorrectly()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-theme-switch");
        button.ShouldHaveClass("ui-theme-switch--sunmoon");
        button.ShouldHaveClass("ui-theme-switch--light");

        // Has sun/moon container
        cut.Find(".ui-theme-switch__sun-moon-container").Should().NotBeNull();
        cut.Find(".ui-theme-switch__sun").Should().NotBeNull();
    }

    [Fact(DisplayName = "DefaultVariant_HasCorrectStructure")]
    public void ThemeSwitch_DefaultVariant_HasCorrectStructure()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>();

        // Assert
        IElement button = cut.Find("button");

        // Should have content wrapper
        IElement content = button.QuerySelector(".ui-theme-switch__content");
        content.Should().NotBeNull();

        // Content should contain icon and label
        content!.Children.Should().HaveCount(2);
        content.QuerySelector(".ui-theme-switch__icon").Should().NotBeNull();
        content.QuerySelector(".ui-theme-switch__label").Should().NotBeNull();
    }

    [Fact(DisplayName = "SunMoonVariant_HasCorrectStructure")]
    public void ThemeSwitch_SunMoonVariant_HasCorrectStructure()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));

        // Assert
        IElement button = cut.Find("button");

        // Should have container
        IElement container = button.QuerySelector(".ui-theme-switch__sun-moon-container");
        container.Should().NotBeNull();

        // Container should have icon wrapper
        IElement wrapper = container!.QuerySelector(".ui-theme-switch__icon-wrapper");
        wrapper.Should().NotBeNull();

        // Wrapper should contain sun (for light theme)
        wrapper!.QuerySelector(".ui-theme-switch__sun").Should().NotBeNull();
    }

    [Fact(DisplayName = "UnknownVariant_RendersNothing")]
    public void ThemeSwitch_UnknownVariant_RendersNothing()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.Custom("Unknown")));

        // Assert
        cut.Markup.Should().BeEmpty();
    }

    [Fact(DisplayName = "DefaultVariant_WithCustomClasses_MergesCorrectly")]
    public void ThemeSwitch_DefaultVariant_WithCustomClasses_MergesCorrectly()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "custom-theme-switch" }
            }));

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-theme-switch");
        button.ShouldHaveClass("ui-theme-switch--default");
        button.ShouldHaveClass("custom-theme-switch");
    }

    [Fact(DisplayName = "SunMoonVariant_LightTheme_HasLightClass")]
    public void ThemeSwitch_SunMoonVariant_LightTheme_HasLightClass()
    {
        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-theme-switch--light");
        button.ShouldNotHaveClass("ui-theme-switch--dark");
    }

    [Fact(DisplayName = "SunMoonVariant_DarkTheme_HasDarkClass")]
    public async Task ThemeSwitch_SunMoonVariant_DarkTheme_HasDarkClass()
    {
        // Arrange
        IThemeJsInterop mockThemeInterop = Services.GetRequiredService<IThemeJsInterop>();
        mockThemeInterop.GetThemeAsync().Returns("dark");

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, UIThemeSwitchVariant.SunMoon));
        await Task.Delay(50); // Wait for initialization

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-theme-switch--dark");
        button.ShouldNotHaveClass("ui-theme-switch--light");
    }

    [Theory(DisplayName = "AllVariants_HaveBaseClasses")]
    [InlineData("Default")]
    [InlineData("SunMoon")]
    public void ThemeSwitch_AllVariants_HaveBaseClasses(string variantName)
    {
        // Arrange
        UIThemeSwitchVariant variant = variantName switch
        {
            "Default" => UIThemeSwitchVariant.Default,
            "SunMoon" => UIThemeSwitchVariant.SunMoon,
            _ => throw new ArgumentException($"Unknown variant: {variantName}")
        };

        // Act
        IRenderedComponent<UIThemeSwitch> cut = Render<UIThemeSwitch>(parameters => parameters
            .Add(p => p.Variant, variant));

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-theme-switch");
        button.ShouldHaveClass($"ui-theme-switch--{variantName.ToLower()}");
    }
}