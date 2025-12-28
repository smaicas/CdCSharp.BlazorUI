using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Components.Features.Transitions;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Common;

[Trait("Features", "CommonFeatures")]
public class CommonFeaturesTests : TestContextBase
{
    [Theory(DisplayName = "IHasDensity_AppliesCorrectClasses_AutomaticallyByUIComponentBase")]
    [InlineData(DensityEnum.Comfortable, "ui-density-comfortable")]
    [InlineData(DensityEnum.Standard, "ui-density-standard")]
    [InlineData(DensityEnum.Compact, "ui-density-compact")]
    public void IHasDensity_AppliesCorrectClasses_AutomaticallyByUIComponentBase(DensityEnum density, string expectedClass)
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Density, density));

        // Assert
        IElement element = cut.Find("div");
        element.ShouldHaveClass(expectedClass);
        element.ShouldHaveClass("test-feature-component"); // Component base class
    }

    [Theory(DisplayName = "IHasElevation_AppliesCorrectClasses_AutomaticallyByUIComponentBase")]
    [InlineData(0, "ui-elevation-0")]
    [InlineData(1, "ui-elevation-1")]
    [InlineData(8, "ui-elevation-8")]
    [InlineData(24, "ui-elevation-24")]
    public void IHasElevation_AppliesCorrectClasses_AutomaticallyByUIComponentBase(int elevation, string? expectedClass)
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Elevation, elevation));

        // Assert
        IElement element = cut.Find("div");
        if (expectedClass != null)
        {
            element.ShouldHaveClass(expectedClass);
        }
        else
        {
            element.ClassList.Should().NotContain(c => c.StartsWith("ui-elevation-"));
        }
    }

    [Theory(DisplayName = "IHasElevation_ClampsValues")]
    [InlineData(-5, 0)]
    [InlineData(30, 24)]
    [InlineData(100, 24)]
    public void IHasElevation_ClampsValues(int inputValue, int expectedValue)
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Elevation, inputValue));

        // Assert
        if (expectedValue >= 0)
        {
            IElement element = cut.Find("div");
            element.ShouldHaveClass($"ui-elevation-{expectedValue}");
        }
    }

    [Theory(DisplayName = "IHasFullWidth_AppliesCorrectClass_AutomaticallyByUIComponentBase")]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IHasFullWidth_AppliesCorrectClass_AutomaticallyByUIComponentBase(bool fullWidth, bool shouldHaveClass)
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.FullWidth, fullWidth));

        // Assert
        IElement element = cut.Find("div");
        if (shouldHaveClass)
        {
            element.ShouldHaveClass("ui-full-width");
        }
        else
        {
            element.ShouldNotHaveClass("ui-full-width");
        }
    }

    [Theory(DisplayName = "IHasSize_AppliesCorrectClasses_AutomaticallyByUIComponentBase")]
    [InlineData(SizeEnum.Small, "ui-size-small")]
    [InlineData(SizeEnum.Medium, "ui-size-medium")]
    [InlineData(SizeEnum.Large, "ui-size-large")]
    public void IHasSize_AppliesCorrectClasses_AutomaticallyByUIComponentBase(SizeEnum size, string expectedClass)
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, size));

        // Assert
        IElement element = cut.Find("div");
        element.ShouldHaveClass(expectedClass);
    }

    [Fact(DisplayName = "IHasTransitions_WithTransitions_AppliesCorrectClasses_AutomaticallyByUIComponentBase")]
    public void IHasTransitions_WithTransitions_AppliesCorrectClasses_AutomaticallyByUIComponentBase()
    {
        // Arrange
        UITransitions transitions = UITransitionPresets.HoverScale;

        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Transitions, transitions));

        // Assert
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-has-transitions");
        element.ShouldHaveClass("ui-transition-hover-scale");
    }

    [Fact(DisplayName = "IHasTransitions_WithTransitions_AppliesInlineStyles")]
    public void IHasTransitions_WithTransitions_AppliesInlineStyles()
    {
        // Arrange
        UITransitions transitions = UITransitionPresets.Create()
            .OnHover().Scale(1.2f, opt => opt.Duration = TimeSpan.FromMilliseconds(500))
            .Build();

        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Transitions, transitions));

        // Assert
        IElement element = cut.Find("div");
        string? style = element.GetAttribute("style");
        style.Should().NotBeNull();
        style.Should().Contain("--ui-transition-hover-scale:");
        style.Should().Contain("--ui-transition-hover-duration:");
    }

    [Fact(DisplayName = "IHasTransitions_WithoutTransitions_NoTransitionClasses")]
    public void IHasTransitions_WithoutTransitions_NoTransitionClasses()
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>();

        // Assert
        IElement element = cut.Find("div");
        element.ShouldNotHaveClass("ui-has-transitions");
        element.ClassList.Should().NotContain(c => c.StartsWith("ui-transition-"));
    }

    [Theory(DisplayName = "IHasLoading_AppliesCorrectClass_AutomaticallyByUIComponentBase")]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IHasLoading_AppliesCorrectClass_AutomaticallyByUIComponentBase(bool isLoading, bool shouldHaveClass)
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.IsLoading, isLoading)
            .Add(p => p.LoadingIndicatorVariant, UILoadingIndicatorVariant.Spinner));

        // Assert
        IElement element = cut.Find("div");
        if (shouldHaveClass)
        {
            element.ShouldHaveClass("ui-loading");
            cut.Find(".test-loading-indicator").Should().NotBeNull();
        }
        else
        {
            element.ShouldNotHaveClass("ui-loading");
            cut.FindAll(".test-loading-indicator").Should().BeEmpty();
        }
    }

    [Theory(DisplayName = "IHasRipple_AppliesCorrectClass_AutomaticallyByUIComponentBase")]
    [InlineData(false, true)]  // DisableRipple = false means ripple is enabled
    [InlineData(true, false)]  // DisableRipple = true means ripple is disabled
    public void IHasRipple_AppliesCorrectClass_AutomaticallyByUIComponentBase(bool disableRipple, bool shouldHaveClass)
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.DisableRipple, disableRipple));

        // Assert
        IElement element = cut.Find("div");
        if (shouldHaveClass)
        {
            element.ShouldHaveClass("ui-has-ripple");
        }
        else
        {
            element.ShouldNotHaveClass("ui-has-ripple");
        }
    }

    [Fact(DisplayName = "MultipleFeaturesAtOnce_AllManagedByUIComponentBase")]
    public void MultipleFeaturesAtOnce_AllManagedByUIComponentBase()
    {
        // Arrange
        UITransitions transitions = UITransitionPresets.HoverLift;

        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.Density, DensityEnum.Compact)
            .Add(p => p.Elevation, 8)
            .Add(p => p.FullWidth, true)
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingIndicatorVariant, UILoadingIndicatorVariant.Spinner)
            .Add(p => p.Transitions, transitions)
            .Add(p => p.DisableRipple, false));

        // Assert
        IElement element = cut.Find("div");

        // Verify all classes are present - ALL handled by UIComponentBase automatically
        element.ShouldHaveClass("test-feature-component"); // From GetAdditionalCssClasses
        element.ShouldHaveClass("ui-size-large"); // Auto by UIComponentBase
        element.ShouldHaveClass("ui-density-compact"); // Auto by UIComponentBase
        element.ShouldHaveClass("ui-elevation-8"); // Auto by UIComponentBase
        element.ShouldHaveClass("ui-full-width"); // Auto by UIComponentBase
        element.ShouldHaveClass("ui-loading"); // Auto by UIComponentBase
        element.ShouldHaveClass("ui-has-transitions"); // Auto by UIComponentBase
        element.ShouldHaveClass("ui-transition-hover-lift"); // Auto by UIComponentBase
        element.ShouldHaveClass("ui-has-ripple"); // Auto by UIComponentBase

        // Verify loading indicator is present
        cut.Find(".test-loading-indicator").Should().NotBeNull();
    }

    [Fact(DisplayName = "UserClasses_MergedWithFeatureClasses")]
    public void UserClasses_MergedWithFeatureClasses()
    {
        // Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.Elevation, 4)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-custom-class another-class" }
            }));

        // Assert
        IElement element = cut.Find("div");

        // Component classes + feature classes + user classes
        element.ShouldHaveClass("test-feature-component");
        element.ShouldHaveClass("ui-size-large");
        element.ShouldHaveClass("ui-elevation-4");
        element.ShouldHaveClass("user-custom-class");
        element.ShouldHaveClass("another-class");
    }
}