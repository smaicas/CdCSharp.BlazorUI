using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Abstractions;

[Trait("Component", "UIComponentBase_ExtensionUsage")]
public class UIComponentBaseExtensionUsageTests : TestContextBase
{
    [Fact(DisplayName = "UsingExtensions_SimplifiesAssertions")]
    public void UsingExtensions_SimplifiesAssertions()
    {
        // Arrange & Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.Density, DensityEnum.Comfortable)
            .Add(p => p.Elevation, 8)
            .Add(p => p.FullWidth, true)
            .Add(p => p.IsLoading, true)
            .Add(p => p.LoadingIndicatorVariant, UILoadingIndicatorVariant.Spinner)
            .Add(p => p.DisableRipple, false)
            .AddUnmatched("class", "custom-class"));

        // Assert using extensions
        IElement element = cut.Find("div");

        // Individual assertions
        element.ShouldHaveClass("custom-class");
        element.ShouldHaveClass("ui-size-large");
        element.ShouldNotHaveClass("ui-size-medium");

        // Feature classes assertion
        element.ShouldHaveFeatureClasses(
            expectedSize: SizeEnum.Large,
            expectedDensity: DensityEnum.Comfortable,
            expectedElevation: 8,
            expectFullWidth: true,
            expectLoading: true,
            expectRipple: true);

        // No duplicates
        element.ShouldHaveNoDuplicateClasses();

        // Exactly one of each type
        element.ShouldHaveExactlyOneClassWithPrefix("ui-size-");
        element.ShouldHaveExactlyOneClassWithPrefix("ui-density-");
        element.ShouldHaveExactlyOneClassWithPrefix("ui-elevation-");
    }

    [Fact(DisplayName = "StyleAssertions_WorkCorrectly")]
    public void StyleAssertions_WorkCorrectly()
    {
        // Arrange & Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Color, UIColor.Blue.Default)
            .Add(p => p.DisableRipple, false)
            .Add(p => p.RippleColor, UIColor.Red.Default)
            .AddUnmatched("style", "margin: 20px"));

        // Assert
        IElement element = cut.Find("div");

        element.ShouldHaveStyle("color");
        element.ShouldHaveStyle("margin", "20px");
        element.ShouldHaveStyle("--ui-ripple-color");
        element.ShouldNotHaveStyle("--ui-transition-duration");
    }

    [Fact(DisplayName = "RenderMultipleTimes_TestsStability")]
    public void RenderMultipleTimes_TestsStability()
    {
        // This tests that rendering multiple times doesn't accumulate classes
        this.RenderMultipleTimes<TestFeatureComponent>(
            times: 10,
            parameterBuilder: parameters => parameters
                .Add(p => p.Size, SizeEnum.Medium)
                .AddUnmatched("class", "stable-class"),
            assertion: (cut, iteration) =>
            {
                IElement element = cut.Find("div");

                // Should always have exactly these classes
                element.ShouldHaveClass("test-feature-component");
                element.ShouldHaveClass("ui-size-medium");
                element.ShouldHaveClass("stable-class");
                element.ShouldHaveClass("ui-has-ripple");
                element.ShouldHaveClass("ui-density-standard");

                // Should not accumulate
                element.ShouldHaveNoDuplicateClasses();

                // Total class count should be consistent
                int classCount = element.ClassList.Count();
                classCount.Should().Be(5, $"at iteration {iteration}");
            });
    }

    [Fact(DisplayName = "GetClassWithPrefix_ReturnsCorrectClass")]
    public void GetClassWithPrefix_ReturnsCorrectClass()
    {
        // Arrange & Act
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.Elevation, 12));

        // Assert
        IElement element = cut.Find("div");

        string? sizeClass = element.GetClassWithPrefix("ui-size-");
        sizeClass.Should().Be("ui-size-large");

        string? elevationClass = element.GetClassWithPrefix("ui-elevation-");
        elevationClass.Should().Be("ui-elevation-12");

        string? nonExistentClass = element.GetClassWithPrefix("ui-nonexistent-");
        nonExistentClass.Should().BeNull();
    }

    [Fact(DisplayName = "ComplexScenario_UsingAllExtensions")]
    public void ComplexScenario_UsingAllExtensions()
    {
        // Initial render
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Small)
            .Add(p => p.Density, DensityEnum.Compact)
            .AddUnmatched("class", "user-class-1 user-class-2")
            .AddUnmatched("style", "padding: 10px"));

        IElement element = cut.Find("div");

        // Initial state assertions
        element.ShouldHaveFeatureClasses(
            expectedSize: SizeEnum.Small,
            expectedDensity: DensityEnum.Compact);
        element.ShouldHaveClass("user-class-1");
        element.ShouldHaveClass("user-class-2");
        element.ShouldHaveStyle("padding", "10px");

        // Update state
        cut.Render(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.Density, DensityEnum.Comfortable)
            .Add(p => p.Elevation, 8)
            .Add(p => p.FullWidth, true));

        // Updated state assertions
        element.ShouldHaveFeatureClasses(
            expectedSize: SizeEnum.Large,
            expectedDensity: DensityEnum.Comfortable,
            expectedElevation: 8,
            expectFullWidth: true);

        // Old classes removed
        element.ShouldNotHaveClass("ui-size-small");
        element.ShouldNotHaveClass("ui-density-compact");

        // User classes preserved
        element.ShouldHaveClass("user-class-1");
        element.ShouldHaveClass("user-class-2");

        // No duplicates after update
        element.ShouldHaveNoDuplicateClasses();

        // Correct number of feature classes
        element.ShouldHaveExactlyOneClassWithPrefix("ui-size-");
        element.ShouldHaveExactlyOneClassWithPrefix("ui-density-");
        element.ShouldHaveExactlyOneClassWithPrefix("ui-elevation-");
    }
}
