using Bunit;
using Microsoft.AspNetCore.Components;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Toast;

[Trait("Component Accessibility", "BUIToast")]
public class BUIToastAccessibilityTests
{
    private static ToastState CreateState(bool closable = true) => new()
    {
        Content = b => b.AddContent(0, "msg"),
        Options = new ToastOptions { Closable = closable, AutoDismiss = false }
    };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Close_Button_Have_AriaLabel(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, CreateState(closable: true)));

        // Assert
        cut.Find("[aria-label='Close']").Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content_With_Toast_Class(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, CreateState()));

        // Assert
        cut.Find(".bui-toast__content").Should().NotBeNull();
    }
}
