using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Dialog;

[Trait("Component Interaction", "BUIDialog")]
public class BUIDialogInteractionTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OpenChanged_False_When_Overlay_Clicked(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? openChangedValue = null;
        IRenderedComponent<BUIDialog> cut = ctx.Render<BUIDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.CloseOnOverlayClick, true)
            .Add(c => c.OpenChanged, v => openChangedValue = v));

        // Act
        cut.Find(".bui-dialog-overlay").Click();

        // Assert
        openChangedValue.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Not_Close_On_Overlay_Click_When_Disabled(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? openChangedValue = null;
        IRenderedComponent<BUIDialog> cut = ctx.Render<BUIDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.CloseOnOverlayClick, false)
            .Add(c => c.OpenChanged, v => openChangedValue = v));

        // Act
        cut.Find(".bui-dialog-overlay").Click();

        // Assert
        openChangedValue.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Fire_OpenChanged_False_On_Escape_Key(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange
        bool? openChangedValue = null;
        IRenderedComponent<BUIDialog> cut = ctx.Render<BUIDialog>(p => p
            .Add(c => c.Open, true)
            .Add(c => c.CloseOnEscape, true)
            .Add(c => c.OpenChanged, v => openChangedValue = v));

        // Act
        cut.Find(".bui-dialog-host").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        // Assert
        openChangedValue.Should().BeFalse();
    }
}
