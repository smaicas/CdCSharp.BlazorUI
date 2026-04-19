using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Toast;

[Trait("Component Interaction", "BUIToastHost")]
public class BUIToastHostInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Remove_Toast_After_CloseAll(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIToastHost> cut = ctx.Render<BUIToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();
        toastService.Show(b => b.AddContent(0, "msg"), new ToastOptions { AutoDismiss = false });
        cut.FindAll("[data-bui-component='toast-host']").Should().HaveCount(1);

        // Act
        toastService.CloseAll();

        // Assert — toasts marked IsClosing, host still renders but toast has closing attr
        // (actual removal happens after animation completes)
        toastService.ActiveToasts.All(t => t.IsClosing).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Multiple_Toasts(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIToastHost> cut = ctx.Render<BUIToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        toastService.Show(b => b.AddContent(0, "Toast 1"), new ToastOptions { AutoDismiss = false });
        toastService.Show(b => b.AddContent(0, "Toast 2"), new ToastOptions { AutoDismiss = false });

        // Assert
        toastService.ActiveToasts.Should().HaveCount(2);
    }
}
