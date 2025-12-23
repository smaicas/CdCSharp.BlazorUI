using Bunit;
using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Types;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Theme;

[Trait("JSInterop", "ThemeJsInterop")]
public class ThemeJsInteropTests : TestContextBase
{
    [Fact(DisplayName = "GetThemeAsync_ReturnsTheme")]
    public async Task ThemeJsInterop_GetThemeAsync_ReturnsTheme()
    {
        // Arrange
        (ThemeJsInterop? themeInterop, BunitJSModuleInterop? moduleSetup) = CreateThemeInterop();
        moduleSetup.Setup<string>("getTheme").SetResult("dark");

        // Act
        string theme = await themeInterop.GetThemeAsync();

        // Assert
        theme.Should().Be("dark");
        IReadOnlyList<JSRuntimeInvocation> invocations = moduleSetup.Invocations["getTheme"];
        invocations.Should().HaveCount(1);
    }

    [Fact(DisplayName = "InitializeAsync_CallsJSWithDefaultTheme")]
    public async Task ThemeJsInterop_InitializeAsync_CallsJSWithDefaultTheme()
    {
        // Arrange
        (ThemeJsInterop? themeInterop, BunitJSModuleInterop? moduleSetup) = CreateThemeInterop();

        // Act
        await themeInterop.InitializeAsync("light");

        // Assert
        IReadOnlyList<JSRuntimeInvocation> invocations = moduleSetup.Invocations["initialize"];
        invocations.Should().HaveCount(1);
        invocations.First().Arguments[0].Should().Be("light");
    }

    [Fact(DisplayName = "InitializeAsync_CallsJSWithNullTheme")]
    public async Task ThemeJsInterop_InitializeAsync_CallsJSWithNullTheme()
    {
        // Arrange
        (ThemeJsInterop? themeInterop, BunitJSModuleInterop? moduleSetup) = CreateThemeInterop();

        // Act
        await themeInterop.InitializeAsync(null);

        // Assert
        IReadOnlyList<JSRuntimeInvocation> invocations = moduleSetup.Invocations["initialize"];
        invocations.Should().HaveCount(1);
        invocations.First().Arguments[0].Should().BeNull();
    }

    [Fact(DisplayName = "SetThemeAsync_CallsJSWithTheme")]
    public async Task ThemeJsInterop_SetThemeAsync_CallsJSWithTheme()
    {
        // Arrange
        (ThemeJsInterop? themeInterop, BunitJSModuleInterop? moduleSetup) = CreateThemeInterop();

        // Act
        await themeInterop.SetThemeAsync("light");

        // Assert
        IReadOnlyList<JSRuntimeInvocation> invocations = moduleSetup.Invocations["setTheme"];
        invocations.Should().HaveCount(1);
        invocations.First().Arguments[0].Should().Be("light");
    }

    [Fact(DisplayName = "ToggleThemeAsync_CallsJSAndReturnsNewTheme")]
    public async Task ThemeJsInterop_ToggleThemeAsync_CallsJSAndReturnsNewTheme()
    {
        // Arrange
        (ThemeJsInterop? themeInterop, BunitJSModuleInterop? moduleSetup) = CreateThemeInterop();

        // Setup robusto con predicado
        moduleSetup.Setup<string>("toggleTheme", args => true).SetResult("dark");

        string[] themes = { "light", "dark" };

        // Act
        string newTheme = await themeInterop.ToggleThemeAsync(themes);

        // Assert
        newTheme.Should().Be("dark");

        IReadOnlyList<JSRuntimeInvocation> invocations = moduleSetup.Invocations["toggleTheme"];
        invocations.Should().HaveCount(1);

        // Argumentos pasados al módulo
        object[]? args = invocations.First().Arguments[0] as object[];
        args.Should().BeEquivalentTo(themes);
    }

    private (ThemeJsInterop themeInterop, BunitJSModuleInterop moduleSetup) CreateThemeInterop()
    {
        BunitJSModuleInterop moduleSetup = JSInterop.SetupModule(JSModulesReference.ThemeJs);
        ThemeJsInterop themeInterop = new(JSInterop.JSRuntime);
        return (themeInterop, moduleSetup);
    }
}