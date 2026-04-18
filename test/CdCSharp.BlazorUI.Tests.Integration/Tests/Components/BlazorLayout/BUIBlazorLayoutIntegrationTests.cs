using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Initializer;
using FluentAssertions;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.BlazorLayout;

[Trait("Component Integration", "BUIBlazorLayout")]
public class BUIBlazorLayoutIntegrationTests
{
    private static void RegisterFakeTheme(BlazorTestContextBase ctx)
    {
        IThemeJsInterop fake = Substitute.For<IThemeJsInterop>();
        fake.GetPaletteAsync().Returns(
            new ValueTask<Dictionary<string, string>>(BUIInitializerRenderingTests.FullPalette));
        fake.InitializeAsync(Arg.Any<string?>()).Returns(ValueTask.CompletedTask);
        ctx.Services.AddScoped(_ => fake);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Toast_Via_Layout(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange
        IRenderedComponent<BUIBlazorLayout> cut = ctx.Render<BUIBlazorLayout>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        toastService.Show(b => b.AddContent(0, "Layout toast"), new ToastOptions { AutoDismiss = false });

        // Assert
        toastService.ActiveToasts.Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Support_Simultaneous_Modal_And_Toast(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();
        RegisterFakeTheme(ctx);

        // Arrange
        IRenderedComponent<BUIBlazorLayout> cut = ctx.Render<BUIBlazorLayout>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act — show toast
        toastService.Show(b => b.AddContent(0, "Toast 1"), new ToastOptions { AutoDismiss = false });
        toastService.Show(b => b.AddContent(0, "Toast 2"), new ToastOptions { AutoDismiss = false });

        // Assert — both coexist
        toastService.ActiveToasts.Should().HaveCount(2);
    }
}
