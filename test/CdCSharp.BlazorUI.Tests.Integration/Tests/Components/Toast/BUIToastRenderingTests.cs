using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Toast;

[Trait("Component Rendering", "BUIToast")]
public class BUIToastRenderingTests
{
    private static ToastState CreateState(
        string content = "Toast message",
        bool closable = true,
        bool autoDismiss = false,
        ToastPosition position = ToastPosition.TopRight) =>
        new()
        {
            Content = b => b.AddContent(0, content),
            Options = new ToastOptions
            {
                Closable = closable,
                AutoDismiss = autoDismiss,
                Position = position,
            }
        };

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Root_With_Correct_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, CreateState()));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be("toast");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Content(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, CreateState("Hello Toast")));

        // Assert
        cut.Find(".bui-toast__content").TextContent.Should().Contain("Hello Toast");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Close_Button_When_Closable(BlazorScenario scenario)
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
    public async Task Should_Not_Render_Close_Button_When_Not_Closable(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, CreateState(closable: false)));

        // Assert
        cut.FindAll("[aria-label='Close']").Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Position_DataAttribute(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, CreateState(position: ToastPosition.BottomLeft)));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-position").Should().Be("bottom-left");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Render_Progress_Bar_When_AutoDismiss(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, CreateState(autoDismiss: true)));

        // Assert
        cut.Find(".bui-toast__progress").Should().NotBeNull();
    }
}
