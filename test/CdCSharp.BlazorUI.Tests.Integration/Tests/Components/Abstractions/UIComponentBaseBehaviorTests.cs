//using AngleSharp.Dom;
//using Bunit;
//using CdCSharp.BlazorUI.Components.Features.Behaviors;
//using CdCSharp.BlazorUI.Core.Css;
//using CdCSharp.BlazorUI.Css;
//using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
//using FluentAssertions;
//using Microsoft.AspNetCore.Components;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using Microsoft.JSInterop;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Abstractions;

//[Trait("Component", "UIComponentBase_Behaviors")]
//public class UIComponentBaseBehaviorTests : TestContext
//{
//    private readonly Mock<IBehaviorJsInterop> _mockBehaviorInterop;
//    private readonly Mock<IJSObjectReference> _mockJsObjectReference;

//    public UIComponentBaseBehaviorTests()
//    {
//        _mockBehaviorInterop = new Mock<IBehaviorJsInterop>();
//        _mockJsObjectReference = new Mock<IJSObjectReference>();

//        Services.AddSingleton(_mockBehaviorInterop.Object);
//    }

//    [Fact(DisplayName = "RippleBehavior_AttachedWhenEnabled")]
//    public async Task RippleBehavior_AttachedWhenEnabled()
//    {
//        // Arrange
//        BehaviorConfiguration? capturedConfig = null;
//        _mockBehaviorInterop
//            .Setup(x => x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()))
//            .Callback<ElementReference, BehaviorConfiguration>((_, config) => capturedConfig = config)
//            .ReturnsAsync(_mockJsObjectReference.Object);

//        // Act
//        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, false)
//            .Add(p => p.RippleColor, UIColor.Blue)
//            .Add(p => p.RippleDuration, 800));

//        // Wait for OnAfterRenderAsync
//        await Task.Delay(50);

//        // Assert
//        _mockBehaviorInterop.Verify(x =>
//            x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()),
//            Times.Once);

//        capturedConfig.Should().NotBeNull();
//        capturedConfig!.Ripple.Should().NotBeNull();
//        capturedConfig.Ripple!.Color.Should().Be(UIColor.Blue.ToString(ColorOutputFormats.Rgba));
//        capturedConfig.Ripple.Duration.Should().Be(800);
//    }

//    [Fact(DisplayName = "RippleBehavior_NotAttachedWhenDisabled")]
//    public async Task RippleBehavior_NotAttachedWhenDisabled()
//    {
//        // Act
//        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, true));

//        // Wait for OnAfterRenderAsync
//        await Task.Delay(50);

//        // Assert
//        _mockBehaviorInterop.Verify(x =>
//            x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()),
//            Times.Never);
//    }

//    [Fact(DisplayName = "RippleBehavior_NotAttachedWhenLoading")]
//    public async Task RippleBehavior_NotAttachedWhenLoading()
//    {
//        // Arrange - Component is loading (disabled state)
//        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, false)
//            .Add(p => p.IsLoading, true));

//        // Wait for OnAfterRenderAsync
//        await Task.Delay(50);

//        // Assert - Behavior not attached when component is disabled
//        _mockBehaviorInterop.Verify(x =>
//            x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()),
//            Times.Never);
//    }

//    [Fact(DisplayName = "BehaviorDisposal_CalledOnComponentDispose")]
//    public async Task BehaviorDisposal_CalledOnComponentDispose()
//    {
//        // Arrange
//        _mockBehaviorInterop
//            .Setup(x => x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()))
//            .ReturnsAsync(_mockJsObjectReference.Object);

//        _mockJsObjectReference
//            .Setup(x => x.InvokeVoidAsync("dispose", It.IsAny<object[]>()))
//            .Returns(ValueTask.CompletedTask);

//        _mockJsObjectReference
//            .Setup(x => x.DisposeAsync())
//            .Returns(ValueTask.CompletedTask);

//        // Act
//        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, false));

//        await Task.Delay(50); // Wait for behavior attachment

//        // Dispose the component
//        cut.Instance.Dispose();
//        await cut.Instance.DisposeAsync();

//        // Assert
//        _mockJsObjectReference.Verify(x => x.InvokeVoidAsync("dispose", It.IsAny<object[]>()), Times.Once);
//        _mockJsObjectReference.Verify(x => x.DisposeAsync(), Times.Once);
//    }

//    [Fact(DisplayName = "RippleStyles_AddedToInlineStyles")]
//    public void RippleStyles_AddedToInlineStyles()
//    {
//        // Arrange & Act
//        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, false)
//            .Add(p => p.RippleColor, UIColor.Purple.Default)
//            .Add(p => p.RippleDuration, 1000));

//        // Assert
//        IElement element = cut.Find("div");
//        string? style = element.GetAttribute("style");

//        style.Should().Contain("--ui-ripple-color");
//        style.Should().Contain(UIColor.Purple.ToString(ColorOutputFormats.Rgba));
//        style.Should().Contain("--ui-ripple-duration: 1000ms");
//    }

//    [Fact(DisplayName = "MultipleBehaviors_CanBeConfigured")]
//    public async Task MultipleBehaviors_CanBeConfigured()
//    {
//        // This test demonstrates extensibility for future behaviors
//        // Arrange
//        BehaviorConfiguration? capturedConfig = null;
//        _mockBehaviorInterop
//            .Setup(x => x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()))
//            .Callback<ElementReference, BehaviorConfiguration>((_, config) => capturedConfig = config)
//            .ReturnsAsync(_mockJsObjectReference.Object);

//        // Create a component that could have multiple behaviors
//        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, false)
//            .Add(p => p.RippleColor, UIColor.Blue.Default));

//        // Wait for OnAfterRenderAsync
//        await Task.Delay(50);

//        // Assert
//        capturedConfig.Should().NotBeNull();
//        capturedConfig!.HasAnyBehavior.Should().BeTrue();
//        capturedConfig.Ripple.Should().NotBeNull();

//        // In the future, we could have:
//        // capturedConfig.Tooltip.Should().NotBeNull();
//        // capturedConfig.FocusTrap.Should().NotBeNull();
//        // etc.
//    }

//    [Fact(DisplayName = "BehaviorConfiguration_HasCorrectDefaults")]
//    public async Task BehaviorConfiguration_HasCorrectDefaults()
//    {
//        // Arrange
//        BehaviorConfiguration? capturedConfig = null;
//        _mockBehaviorInterop
//            .Setup(x => x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()))
//            .Callback<ElementReference, BehaviorConfiguration>((_, config) => capturedConfig = config)
//            .ReturnsAsync(_mockJsObjectReference.Object);

//        // Act - Ripple with default values
//        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, false));

//        await Task.Delay(50);

//        // Assert
//        capturedConfig.Should().NotBeNull();
//        capturedConfig!.Ripple.Should().NotBeNull();
//        capturedConfig.Ripple!.Duration.Should().Be(600); // Default duration
//        capturedConfig.Ripple.Color.Should().BeNull(); // No custom color
//    }

//    [Fact(DisplayName = "NoJsInterop_DoesNotThrow")]
//    public async Task NoJsInterop_DoesNotThrow()
//    {
//        // Arrange - Remove the JS interop service
//        Services.RemoveAll<IBehaviorJsInterop>();

//        // Act & Assert - Should not throw
//        Func<IRenderedComponent<TestFeatureComponent>> act = () => Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, false));

//        act.Should().NotThrow();

//        // Wait to ensure OnAfterRenderAsync completes
//        await Task.Delay(50);
//    }

//    [Fact(DisplayName = "StateChange_DoesNotReattachBehaviors")]
//    public async Task StateChange_DoesNotReattachBehaviors()
//    {
//        // Arrange
//        _mockBehaviorInterop
//            .Setup(x => x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()))
//            .ReturnsAsync(_mockJsObjectReference.Object);

//        // Act - Initial render
//        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
//            .Add(p => p.DisableRipple, false));

//        await Task.Delay(50); // Wait for first attachment

//        // Re-render with different state
//        cut.Render(parameters => parameters
//            .Add(p => p.Size, SizeEnum.Large));

//        await Task.Delay(50); // Wait to ensure no second attachment

//        // Assert - Behavior attached only once (on first render)
//        _mockBehaviorInterop.Verify(x =>
//            x.AttachBehaviorsAsync(It.IsAny<ElementReference>(), It.IsAny<BehaviorConfiguration>()),
//            Times.Once);
//    }