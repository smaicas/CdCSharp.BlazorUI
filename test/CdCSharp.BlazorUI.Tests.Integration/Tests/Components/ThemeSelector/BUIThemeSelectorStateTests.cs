using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component State", "BUIThemeSelector")]
public class BUIThemeSelectorStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Light_Label_When_Theme_Is_Light(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — GetThemeAsync returns "light"
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>("light"));
        ctx.Services.AddScoped(_ => fake);

        // Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>();

        // Assert — after firstRender, label = "Light"
        cut.Find(".bui-theme-switch__label").TextContent.Should().Be("Light");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Dark_Label_When_GetThemeAsync_Returns_Dark(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange — GetThemeAsync returns "dark"
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetThemeAsync().Returns(new ValueTask<string>("dark"));
        ctx.Services.AddScoped(_ => fake);

        // Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>();

        // Assert
        cut.Find(".bui-theme-switch__label").TextContent.Should().Be("Dark");
    }
}
