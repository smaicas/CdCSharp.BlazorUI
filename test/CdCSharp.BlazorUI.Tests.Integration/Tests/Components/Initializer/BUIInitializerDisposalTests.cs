using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Initializer;

[Trait("Component Disposal", "BUIInitializer")]
public class BUIInitializerDisposalTests
{
    private sealed class TrackedThemeInterop : IThemeJsInterop
    {
        public event Action<string>? OnThemeChanged;

        public Func<ValueTask<Dictionary<string, string>>> PaletteFactory { get; set; }
            = () => new ValueTask<Dictionary<string, string>>(BUIInitializerRenderingTests.FullPalette);

        public int SubscriberCount => OnThemeChanged?.GetInvocationList().Length ?? 0;

        public ValueTask InitializeAsync(string? defaultTheme = null) => ValueTask.CompletedTask;

        public ValueTask<Dictionary<string, string>> GetPaletteAsync() => PaletteFactory();

        public ValueTask<string> GetThemeAsync() => new("dark");

        public ValueTask SetThemeAsync(string theme) => ValueTask.CompletedTask;

        public ValueTask<string> ToggleThemeAsync(string[] themes) => new("dark");

        public void RaiseThemeChanged(string theme) => OnThemeChanged?.Invoke(theme);
    }

    private static TrackedThemeInterop RegisterFake(BlazorTestContextBase ctx)
    {
        TrackedThemeInterop fake = new();
        ctx.Services.AddScoped<IThemeJsInterop>(_ => fake);
        return fake;
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Unsubscribe_From_OnThemeChanged_When_Disposed(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TrackedThemeInterop fake = RegisterFake(ctx);

        // Arrange
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>();
        fake.SubscriberCount.Should().Be(1);

        // Act
        cut.Instance.Dispose();

        // Assert
        fake.SubscriberCount.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Leak_Subscribers_Across_Mount_Unmount_Cycles(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TrackedThemeInterop fake = RegisterFake(ctx);

        // Arrange & Act & Assert
        for (int i = 0; i < 10; i++)
        {
            IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>();
            fake.SubscriberCount.Should().Be(1, $"cycle {i}: only the live initializer must be subscribed");

            cut.Instance.Dispose();
            fake.SubscriberCount.Should().Be(0, $"cycle {i}: dispose must unsubscribe");
        }
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Count_Disposed_Instances_As_Subscribers(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TrackedThemeInterop fake = RegisterFake(ctx);

        // Arrange — simulate multiple mounts + one dispose in between
        IRenderedComponent<BUIInitializer> first = ctx.Render<BUIInitializer>();
        IRenderedComponent<BUIInitializer> second = ctx.Render<BUIInitializer>();
        fake.SubscriberCount.Should().Be(2);

        // Act
        first.Instance.Dispose();

        // Assert
        fake.SubscriberCount.Should().Be(1);

        second.Instance.Dispose();
        fake.SubscriberCount.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Throw_When_Dispose_Called_Multiple_Times(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFake(ctx);

        // Arrange
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>();

        // Act
        Action doubleDispose = () =>
        {
            cut.Instance.Dispose();
            cut.Instance.Dispose();
        };

        // Assert
        doubleDispose.Should().NotThrow();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Absorb_JSDisconnectedException_In_Theme_Change_Handler(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TrackedThemeInterop fake = RegisterFake(ctx);

        // Arrange
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>(p => p
            .AddChildContent("<span class='child'>ok</span>"));

        fake.PaletteFactory = () =>
            ValueTask.FromException<Dictionary<string, string>>(
                new JSDisconnectedException("circuit disposed"));

        // Act — async void handler catches JSDisconnectedException
        fake.RaiseThemeChanged("light");
        await Task.Delay(100);

        // Assert — no exception escaped; component still alive
        cut.FindAll(".child").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Absorb_ObjectDisposedException_In_Theme_Change_Handler(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        TrackedThemeInterop fake = RegisterFake(ctx);

        // Arrange
        IRenderedComponent<BUIInitializer> cut = ctx.Render<BUIInitializer>(p => p
            .AddChildContent("<span class='child'>ok</span>"));

        fake.PaletteFactory = () =>
            ValueTask.FromException<Dictionary<string, string>>(
                new ObjectDisposedException("circuit"));

        // Act
        fake.RaiseThemeChanged("light");
        await Task.Delay(100);

        // Assert — no exception escaped
        cut.FindAll(".child").Should().HaveCount(1);
    }
}
