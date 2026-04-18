using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Interaction", "BUIThemeSelector")]
public class BUIThemeSelectorInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Call_ToggleThemeAsync_On_Button_Click(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>("light"));
        fake.ToggleThemeAsync(Arg.Any<string[]>()).Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);

        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>();

        // Act
        cut.Find("button").Click();

        // Assert — wait for async switch (Task.Delay(50) inside SwitchTheme)
        await Task.Delay(100);
        await fake.Received(1).ToggleThemeAsync(Arg.Any<string[]>());
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OnThemeChanged_After_Toggle(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        string? capturedTheme = null;
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>("light"));
        fake.ToggleThemeAsync(Arg.Any<string[]>()).Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);

        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>(p => p
            .Add(c => c.OnThemeChanged, t => capturedTheme = t));

        // Act
        cut.Find("button").Click();
        await Task.Delay(100);

        // Assert
        capturedTheme.Should().Be("dark");
    }
}
