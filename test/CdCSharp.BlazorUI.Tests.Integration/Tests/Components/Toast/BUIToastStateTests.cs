using Bunit;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Components.Layout.Services;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Toast;

[Trait("Component State", "BUIToast")]
public class BUIToastStateTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Closing_DataAttribute_When_IsClosing(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        ToastState state = new()
        {
            Content = b => b.AddContent(0, "msg"),
            Options = ToastOptions.Default
        };

        // Act — set IsClosing = true
        state.IsClosing = true;

        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, state));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-closing").Should().Be("true");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Show_Paused_DataAttribute_When_IsPaused(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        ToastState state = new()
        {
            Content = b => b.AddContent(0, "msg"),
            Options = ToastOptions.Default
        };
        state.IsPaused = true;

        // Act
        IRenderedComponent<BUIToast> cut = ctx.Render<BUIToast>(p => p
            .Add(c => c.State, state));

        // Assert
        cut.Find("bui-component").GetAttribute("data-bui-paused").Should().Be("true");
    }
}
