using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Initializer;

[Trait("Component Interaction", "BUIInitializer")]
public class BUIInitializerInteractionTests
{
    private static IThemeJsInterop BuildFake(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BUIInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
        return fake;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Subscribe_To_OnThemeChanged_On_Init(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = BuildFake(ctx);

        // Arrange & Act
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>();

        // Assert — component subscribed: verify via InitializeAsync being called (lifecycle completed)
        await fake.Received(1).InitializeAsync(Arg.Any<string?>());
        fake.Received(1).OnThemeChanged += Arg.Any<Action<string>?>();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Unsubscribe_From_OnThemeChanged_On_Dispose(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        IThemeJsInterop fake = BuildFake(ctx);

        // Arrange
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>();

        // Act
        cut.Instance.Dispose();

        // Assert
        fake.Received(1).OnThemeChanged -= Arg.Any<Action<string>?>();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Reload_Palette_When_ThemeChanged_Fires(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        Action<string>? registeredHandler = null;
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BUIInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        fake.OnThemeChanged += Arg.Do<Action<string>>(h => registeredHandler = h);
        ctx.Services.AddScoped(_ => fake);

        // Arrange
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>(p => p
            .AddChildContent("<span class='child'>ok</span>"));

        int callsBefore = fake.ReceivedCalls()
            .Count(c => c.GetMethodInfo().Name == "GetPaletteAsync");

        // Act — simulate theme change event
        registeredHandler?.Invoke("light");
        cut.WaitForState(() => true, TimeSpan.FromMilliseconds(300));

        // Assert — GetPaletteAsync called again
        int callsAfter = fake.ReceivedCalls()
            .Count(c => c.GetMethodInfo().Name == "GetPaletteAsync");
        callsAfter.Should().BeGreaterThan(callsBefore);
    }
}
