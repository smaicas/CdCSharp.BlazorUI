using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Behaviors;

[Trait("Behaviors", "UIComponentBase")]
public class UIComponentBaseBehaviorTests : TestContextBase
{
    private readonly IBehaviorJsInterop _mockBehaviorInterop;

    public UIComponentBaseBehaviorTests()
    {
        _mockBehaviorInterop = Substitute.For<IBehaviorJsInterop>();
        Services.AddSingleton(_mockBehaviorInterop);
    }

    [Fact(DisplayName = "Component_WithRipple_AttachesBehavior")]
    public async Task Component_WithRipple_AttachesBehavior()
    {
        // Arrange
        IJSObjectReference mockJsRef = Substitute.For<IJSObjectReference>();
        _mockBehaviorInterop
            .AttachBehaviorsAsync(Arg.Any<ElementReference>(), Arg.Any<BehaviorConfiguration>())
            .Returns(mockJsRef);

        // Act
        Bunit.IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Ripple Button")
            .Add(p => p.DisableRipple, false));

        // Wait for OnAfterRenderAsync
        await Task.Delay(50);

        // Assert
        await _mockBehaviorInterop.Received(1)
            .AttachBehaviorsAsync(
                Arg.Any<ElementReference>(),
                Arg.Is<BehaviorConfiguration>(c => c.Ripple != null));
    }

    [Fact(DisplayName = "Component_WithDisabledRipple_DoesNotAttachBehavior")]
    public async Task Component_WithDisabledRipple_DoesNotAttachBehavior()
    {
        // Act
        Bunit.IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "No Ripple")
            .Add(p => p.DisableRipple, true));

        // Wait for OnAfterRenderAsync
        await Task.Delay(50);

        // Assert
        await _mockBehaviorInterop.DidNotReceive()
            .AttachBehaviorsAsync(Arg.Any<ElementReference>(), Arg.Any<BehaviorConfiguration>());
    }

    [Fact(DisplayName = "Component_WithCustomRippleConfig_PassesCorrectValues")]
    public async Task Component_WithCustomRippleConfig_PassesCorrectValues()
    {
        // Arrange
        CssColor customColor = new("#FF0000");
        int customDuration = 1000;
        BehaviorConfiguration? capturedConfig = null;

        _mockBehaviorInterop
            .AttachBehaviorsAsync(Arg.Any<ElementReference>(), Arg.Do<BehaviorConfiguration>(c => capturedConfig = c))
            .Returns(Substitute.For<IJSObjectReference>());

        // Act
        Bunit.IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Custom Ripple")
            .Add(p => p.RippleColor, customColor)
            .Add(p => p.RippleDuration, customDuration));

        // Wait for OnAfterRenderAsync
        await Task.Delay(50);

        // Assert
        capturedConfig.Should().NotBeNull();
        capturedConfig!.Ripple.Should().NotBeNull();
        capturedConfig.Ripple!.Color.Should().Be("rgba(255,0,0,1)");
        capturedConfig.Ripple.Duration.Should().Be(1000);
    }

    [Fact(DisplayName = "Component_Dispose_CallsJSDispose")]
    public async Task Component_Dispose_CallsJSDispose()
    {
        // Arrange
        IJSObjectReference mockJsRef = Substitute.For<IJSObjectReference>();
        _mockBehaviorInterop
            .AttachBehaviorsAsync(Arg.Any<ElementReference>(), Arg.Any<BehaviorConfiguration>())
            .Returns(mockJsRef);

        Bunit.IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Dispose Test"));

        // Wait for OnAfterRenderAsync
        await Task.Delay(50);

        // Act
        if (cut is IAsyncDisposable cutAsync)
        {
            await ((IAsyncDisposable)cutAsync).DisposeAsync();
        }

        // Assert
        await mockJsRef.Received(1).InvokeVoidAsync("dispose");
        await mockJsRef.Received(1).DisposeAsync();
    }
}