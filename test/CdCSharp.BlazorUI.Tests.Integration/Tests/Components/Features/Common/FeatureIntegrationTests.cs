using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Behaviors;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Components.Features.Transitions;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Common;

[Trait("Features", "Integration")]
public class FeatureIntegrationTests : TestContextBase
{
    private readonly IBehaviorJsInterop _mockBehaviorInterop;

    public FeatureIntegrationTests()
    {
        _mockBehaviorInterop = Substitute.For<IBehaviorJsInterop>();
        Services.AddSingleton(_mockBehaviorInterop);
    }

    [Fact(DisplayName = "UIButton_WithAllFeatures_AppliesAllCorrectly")]
    public void UIButton_WithAllFeatures_AppliesAllCorrectly()
    {
        // Arrange
        UITransitions transitions = UITransitionPresets.MaterialButton;
        CssColor rippleColor = UIColor.Blue.Default;

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Full Featured Button")
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.Density, DensityEnum.Compact)
            .Add(p => p.Elevation, 8)
            .Add(p => p.FullWidth, true)
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingIndicatorVariant, UILoadingIndicatorVariant.Spinner)
            .Add(p => p.Transitions, transitions)
            .Add(p => p.DisableRipple, false)
            .Add(p => p.RippleColor, rippleColor));

        // Assert
        IElement button = cut.Find("button");

        // Verify all feature classes - all managed by UIComponentBase
        button.ShouldHaveClass("ui-button");
        button.ShouldHaveClass("ui-button--default"); // From variant
        button.ShouldHaveClass("ui-size-large"); // Auto by UIComponentBase
        button.ShouldHaveClass("ui-density-compact"); // Auto by UIComponentBase
        button.ShouldHaveClass("ui-elevation-8"); // Auto by UIComponentBase
        button.ShouldHaveClass("ui-full-width"); // Auto by UIComponentBase
        button.ShouldHaveClass("ui-loading"); // Auto by UIComponentBase
        button.ShouldHaveClass("ui-has-transitions"); // Auto by UIComponentBase
        button.ShouldHaveClass("ui-has-ripple"); // Auto by UIComponentBase

        // Verify loading indicator is present
        cut.Find(".ui-loading-indicator").Should().NotBeNull();

        // Verify disabled state due to loading
        button.HasAttribute("disabled").Should().BeTrue();

        // Verify transition styles
        string? style = button.GetAttribute("style");
        style.Should().Contain("--ui-transition");
    }

    [Fact(DisplayName = "SizeAndDensity_WorkTogetherCorrectly")]
    public void SizeAndDensity_WorkTogetherCorrectly()
    {
        // Test combinations of size and density
        var combinations = new[]
        {
            new { Size = SizeEnum.Small, Density = DensityEnum.Compact },
            new { Size = SizeEnum.Medium, Density = DensityEnum.Standard },
            new { Size = SizeEnum.Large, Density = DensityEnum.Comfortable }
        };

        foreach (var combo in combinations)
        {
            // Act
            IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
                .Add(p => p.Size, combo.Size)
                .Add(p => p.Density, combo.Density));

            // Assert
            IElement element = cut.Find("div");
            element.ShouldHaveClass(CssClassesReference.Size(combo.Size));
            element.ShouldHaveClass(CssClassesReference.Density(combo.Density));
        }
    }

    [Fact(DisplayName = "LoadingState_DisablesTransitionsAndRipple")]
    public async Task LoadingState_DisablesTransitionsAndRipple()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Loading Button")
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingIndicatorVariant, UILoadingIndicatorVariant.Spinner)
            .Add(p => p.DisableRipple, false)
            .Add(p => p.Transitions, UITransitionPresets.HoverScale));

        // Assert
        // Button should be disabled
        IElement button = cut.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();

        // Ripple behavior should not be attached when loading
        await Task.Delay(50);
        await _mockBehaviorInterop.DidNotReceive()
            .AttachBehaviorsAsync(Arg.Any<ElementReference>(), Arg.Any<BehaviorConfiguration>());

        // Click should not work
        bool wasClicked = false;
        cut.Render(parameters => parameters
            .Add(p => p.OnClick, () => wasClicked = true));

        await button.ClickAsync();
        wasClicked.Should().BeFalse();
    }

    [Fact(DisplayName = "ElevationAndTransitions_CreateLayeredEffect")]
    public void ElevationAndTransitions_CreateLayeredEffect()
    {
        // Arrange
        UITransitions liftTransition = UITransitionPresets.HoverLift;

        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Elevation, 4)
            .Add(p => p.Transitions, liftTransition));

        // Assert
        IElement element = cut.Find("div");

        // Has base elevation
        element.ShouldHaveClass("ui-elevation-4");

        // Has transition that will modify elevation on hover
        element.ShouldHaveClass("ui-has-transitions");
        element.ShouldHaveClass("ui-transition-hover-lift");
    }

    [Fact(DisplayName = "FullWidthWithDensity_AppliesBothCorrectly")]
    public void FullWidthWithDensity_AppliesBothCorrectly()
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.FullWidth, true)
            .Add(p => p.Density, DensityEnum.Comfortable));

        // Assert
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-full-width");
        element.ShouldHaveClass("ui-density-comfortable");
    }

    [Fact(DisplayName = "RippleWithCustomColor_ConfiguredCorrectly")]
    public async Task RippleWithCustomColor_ConfiguredCorrectly()
    {
        // Arrange
        CssColor customRippleColor = new("#FF5733");
        BehaviorConfiguration? capturedConfig = null;

        _mockBehaviorInterop
            .AttachBehaviorsAsync(Arg.Any<ElementReference>(), Arg.Do<BehaviorConfiguration>(c => capturedConfig = c))
            .Returns(Substitute.For<IJSObjectReference>());

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Custom Ripple")
            .Add(p => p.DisableRipple, false)
            .Add(p => p.RippleColor, customRippleColor)
            .Add(p => p.RippleDuration, 800));

        // Wait for OnAfterRenderAsync
        await Task.Delay(50);

        // Assert
        await _mockBehaviorInterop.Received(1)
            .AttachBehaviorsAsync(Arg.Any<ElementReference>(), Arg.Any<BehaviorConfiguration>());

        capturedConfig.Should().NotBeNull();
        capturedConfig!.Ripple.Should().NotBeNull();
        capturedConfig.Ripple!.Color.Should().Be("rgba(255,87,51,1)");
        capturedConfig.Ripple.Duration.Should().Be(800);
    }

    [Fact(DisplayName = "ComplexTransitions_WithMultipleTriggers")]
    public void ComplexTransitions_WithMultipleTriggers()
    {
        // Arrange
        UITransitions complexTransitions = UITransitionPresets.Create()
            .OnHover()
                .Scale(1.1f)
                .Shadow("0 8px 16px rgba(0,0,0,0.1)")
                .Background("rgba(0,0,0,0.05)")
                .And()
            .OnFocus()
                .Border("2px solid #2196F3")
                .And()
            .OnActive()
                .Scale(0.98f)
            .Build();

        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Transitions, complexTransitions));

        // Assert
        IElement element = cut.Find("div");

        // Has all transition classes
        element.ShouldHaveClass("ui-has-transitions");
        string classes = element.GetAttribute("class") ?? "";
        classes.Should().Contain("ui-transition-hover");
        classes.Should().Contain("ui-transition-focus");
        classes.Should().Contain("ui-transition-active");

        // Has all transition styles
        string? style = element.GetAttribute("style");
        style.Should().Contain("--ui-transition-hover-scale");
        style.Should().Contain("--ui-transition-hover-shadow");
        style.Should().Contain("--ui-transition-hover-background");
        style.Should().Contain("--ui-transition-focus-border");
        style.Should().Contain("--ui-transition-active-scale");
    }

    [Fact(DisplayName = "LoadingIndicatorSize_MatchesComponentSize")]
    public void LoadingIndicatorSize_MatchesComponentSize()
    {
        // Test that loading indicator size matches button size
        var sizeMappings = new[]
        {
            new { ButtonSize = SizeEnum.Small, ExpectedLoadingSize = "ui-size-small" },
            new { ButtonSize = SizeEnum.Medium, ExpectedLoadingSize = "ui-size-medium" },
            new { ButtonSize = SizeEnum.Large, ExpectedLoadingSize = "ui-size-large" }
        };

        foreach (var mapping in sizeMappings)
        {
            // Act
            IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
                .Add(p => p.Text, "Loading")
                .Add(p => p.Size, mapping.ButtonSize)
                .Add(p => p.IsLoading, true)
                .Add(p => p.LoadingIndicatorVariant, UILoadingIndicatorVariant.Spinner));

            // Assert
            IElement loadingIndicator = cut.Find(".ui-loading-indicator");
            loadingIndicator.ShouldHaveClass(mapping.ExpectedLoadingSize);
        }
    }

    [Fact(DisplayName = "StateChange_UpdatesFeaturesCorrectly")]
    public void StateChange_UpdatesFeaturesCorrectly()
    {
        // Arrange - Start with minimal features
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Small)
            .Add(p => p.Elevation, 0));

        // Initial state
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-size-small");
        element.ShouldHaveClass("ui-elevation-0");

        // Act - Update to add more features
        cut.Render(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.Elevation, 12)
            .Add(p => p.FullWidth, true)
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingIndicatorVariant, UILoadingIndicatorVariant.Dots));

        // Assert - All new features applied
        element = cut.Find("div");
        element.ShouldHaveClass("ui-size-large");
        element.ShouldNotHaveClass("ui-size-small");
        element.ShouldNotHaveClass("ui-elevation-0");
        element.ShouldHaveClass("ui-elevation-12");
        element.ShouldHaveClass("ui-full-width");
        element.ShouldHaveClass("ui-loading");
        cut.Find(".test-loading-indicator").Should().NotBeNull();
    }
}