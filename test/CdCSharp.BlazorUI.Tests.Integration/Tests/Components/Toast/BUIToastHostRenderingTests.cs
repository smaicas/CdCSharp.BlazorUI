using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Toast;

[Trait("Component Rendering", "BUIToastHost")]
public class BUIToastHostRenderingTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Nothing_When_No_Toasts(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToastHost> cut = ctx.Render<BUIToastHost>();

        // Assert
        cut.FindAll("[data-bui-component='toast-host']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toast_When_Show_Called(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIToastHost> cut = ctx.Render<BUIToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        toastService.Show(b => b.AddContent(0, "Hello"), new ToastOptions { AutoDismiss = false });

        // Assert
        cut.FindAll("[data-bui-component='toast-host']").Should().HaveCount(1);
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Toast_In_Correct_Position(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        IRenderedComponent<BUIToastHost> cut = ctx.Render<BUIToastHost>();
        IToastService toastService = ctx.Services.GetRequiredService<IToastService>();

        // Act
        toastService.Show(b => b.AddContent(0, "Bottom Left Toast"),
            new ToastOptions { AutoDismiss = false, Position = ToastPosition.BottomLeft });

        // Assert
        cut.Find("[data-bui-position='bottom-left']").Should().NotBeNull();
    }
}
