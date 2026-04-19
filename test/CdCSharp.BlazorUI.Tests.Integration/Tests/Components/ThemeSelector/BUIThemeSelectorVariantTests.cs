using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.ThemeSelector;

[Trait("Component Variants", "BUIThemeSelector")]
public class BUIThemeSelectorVariantTests
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
    public async Task Should_Render_Default_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>(p => p
            .Add(c => c.Variant, BUIThemeSelectorVariant.Default));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-variant").Should().Be("default");
        cut.Find("button").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_SunMoon_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange & Act
        IRenderedComponent<BUIThemeSelector> cut = ctx.Render<BUIThemeSelector>(p => p
            .Add(c => c.Variant, BUIThemeSelectorVariant.SunMoon));

        // Assert — SunMoon renders BUISwitch (which has a bui-component)
        cut.Find("bui-component").Should().NotBeNull();
    }
}
