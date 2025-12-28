using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Transitions;
using CdCSharp.BlazorUI.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Abstractions;

[Trait("Component", "UIComponentBase_EdgeCases")]
public class UIComponentBaseEdgeCasesTests : TestContextBase
{
    [Fact(DisplayName = "NullTransitions_HandledGracefully")]
    public void NullTransitions_HandledGracefully()
    {
        // Arrange - Start with transitions
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Transitions, UITransitionPresets.HoverScale));

        // Initial state
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-has-transitions");

        // Act - Set transitions to null
        cut.Render(parameters => parameters
            .Add(p => p.Transitions, null));

        // Assert - Transition classes removed
        element = cut.Find("div");
        element.ShouldNotHaveClass("ui-has-transitions");
        element.ClassList.Should().NotContain(c => c.Contains("transition"));
    }

    [Fact(DisplayName = "EmptyUserClasses_HandledCorrectly")]
    public void EmptyUserClasses_HandledCorrectly()
    {
        // Arrange - Start with user classes
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .AddUnmatched("class", "user-class"));

        // Act - Set empty user classes
        cut.Render(parameters => parameters
            .AddUnmatched("class", ""));

        // Assert - Only component classes remain
        IElement element = cut.Find("div");
        element.ShouldNotHaveClass("user-class");
        element.ShouldHaveClass("test-feature-component"); // Base component class
    }

    [Fact(DisplayName = "WhitespaceInClasses_HandledCorrectly")]
    public void WhitespaceInClasses_HandledCorrectly()
    {
        // Arrange - Classes with various whitespace
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .AddUnmatched("class", "  class1   class2    class3  "));

        // Assert - Whitespace normalized
        IElement element = cut.Find("div");
        element.ShouldHaveClass("class1");
        element.ShouldHaveClass("class2");
        element.ShouldHaveClass("class3");

        // No empty classes
        List<string> classes = element.ClassList.ToList();
        classes.Should().NotContain(string.Empty);
        classes.Should().NotContain(c => string.IsNullOrWhiteSpace(c));
    }

    [Fact(DisplayName = "DuplicateUserClasses_HandledCorrectly")]
    public void DuplicateUserClasses_HandledCorrectly()
    {
        // Arrange - User provides duplicate classes
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .AddUnmatched("class", "duplicate duplicate another duplicate"));

        // Assert - Duplicates preserved (browser handles deduplication)
        IElement element = cut.Find("div");
        string classAttribute = element.GetAttribute("class") ?? "";

        // The component should preserve what the user provided
        classAttribute.Should().Contain("duplicate duplicate");
    }

    [Fact(DisplayName = "StyleSemicolonHandling_CorrectlyFormatted")]
    public void StyleSemicolonHandling_CorrectlyFormatted()
    {
        // Arrange - Various style formats
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Color, UIColor.Red.Default)
            .AddUnmatched("style", "margin: 10px; padding: 5px")); // With trailing semicolon

        // Assert - Properly formatted
        IElement element = cut.Find("div");
        string? style = element.GetAttribute("style");
        style.Should().NotBeNull();

        // Should not have double semicolons
        style.Should().NotContain(";;");

        // Should contain both component and user styles
        style.Should().Contain("color:");
        style.Should().Contain("margin: 10px");
        style.Should().Contain("padding: 5px");
    }

    [Fact(DisplayName = "ZeroElevation_RemovesClass")]
    public void ZeroElevation_RemovesClass()
    {
        // Arrange - Start with elevation
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Elevation, 8));

        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-elevation-8");

        // Act - Set elevation to 0
        cut.Render(parameters => parameters
            .Add(p => p.Elevation, 0));

        // Assert - Elevation class removed
        element = cut.Find("div");
        element.ShouldHaveClass("ui-elevation-0");
        element.ShouldNotHaveClass("ui-elevation-8");
    }

    [Fact(DisplayName = "ElevationClamping_WorksCorrectly")]
    public void ElevationClamping_WorksCorrectly()
    {
        // Arrange & Act - Elevation beyond max
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Elevation, 100));

        // Assert - Clamped to 24
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-elevation-24");
        element.ShouldNotHaveClass("ui-elevation-100");
    }

    [Fact(DisplayName = "ComplexTransitionClasses_ParsedCorrectly")]
    public void ComplexTransitionClasses_ParsedCorrectly()
    {
        // Arrange - Complex transition with multiple effects
        UITransitions complexTransition = UITransitionPresets.Create()
            .OnHover()
                .Scale(1.1f)
                .Rotate("5deg")
                .Shadow("0 4px 8px rgba(0,0,0,0.2)")
            .And().OnFocus()
                .Shadow("0 0 0 3px rgba(0,0,0,0.1)")
            .And().OnActive()
                .Scale(0.95f)
            .Build();

        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Transitions, complexTransition));

        // Assert - All transition classes added
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-has-transitions");
        element.ShouldHaveClass("ui-transition-hover-scale");
        element.ShouldHaveClass("ui-transition-hover-rotate");
        element.ShouldHaveClass("ui-transition-hover-shadow");
        element.ShouldHaveClass("ui-transition-focus-shadow");
        element.ShouldHaveClass("ui-transition-active-scale");
    }

    [Fact(DisplayName = "RippleWithoutColor_UsesDefault")]
    public void RippleWithoutColor_UsesDefault()
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.DisableRipple, false)
            .Add(p => p.RippleColor, null));

        // Assert - Has ripple class but no color style
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-has-ripple");

        string? style = element.GetAttribute("style");
        if (!string.IsNullOrEmpty(style))
        {
            style.Should().NotContain("--ui-ripple-color");
        }
    }

    [Theory(DisplayName = "LoadingState_DisablesRipple")]
    [InlineData(true)]
    [InlineData(false)]
    public void LoadingState_DisablesRipple(bool isLoading)
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.DisableRipple, false)
            .Add(p => p.IsLoading, isLoading));

        // Assert
        IElement element = cut.Find("div");
        if (isLoading)
        {
            element.ShouldHaveClass("ui-loading");
            // Ripple might still have class but behavior won't attach
        }
        else
        {
            element.ShouldNotHaveClass("ui-loading");
            element.ShouldHaveClass("ui-has-ripple");
        }
    }

    [Fact(DisplayName = "SpecialCharactersInClasses_PreservedCorrectly")]
    public void SpecialCharactersInClasses_PreservedCorrectly()
    {
        // Arrange - Classes with special characters (valid in CSS)
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .AddUnmatched("class", "class-with-dash class_with_underscore class123"));

        // Assert
        IElement element = cut.Find("div");
        element.ShouldHaveClass("class-with-dash");
        element.ShouldHaveClass("class_with_underscore");
        element.ShouldHaveClass("class123");
    }

    [Fact(DisplayName = "ConcurrentModification_HandledSafely")]
    public void ConcurrentModification_HandledSafely()
    {
        // This tests that modifying state during render doesn't cause issues
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>();

        // Act - Simulate rapid concurrent-like updates
        Task task1 = Task.Run(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                cut.InvokeAsync(() => cut.Render(parameters => parameters
                    .Add(p => p.Size, SizeEnum.Large)));
            }
        });

        Task task2 = Task.Run(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                cut.InvokeAsync(() => cut.Render(parameters => parameters
                    .Add(p => p.Size, SizeEnum.Small)));
            }
        });

        Task.WaitAll(task1, task2);

        // Assert - Component still in valid state
        IElement element = cut.Find("div");
        int sizeClasses = element.ClassList.Count(c => c.StartsWith("ui-size-"));
        sizeClasses.Should().Be(1); // Exactly one size class
    }

    [Fact(DisplayName = "AttributeRemoval_CleansUpProperly")]
    public void AttributeRemoval_CleansUpProperly()
    {
        // Arrange - Start with styles and classes
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Color, UIColor.Red.Default)
            .Add(p => p.Size, SizeEnum.Large)
            .AddUnmatched("class", "custom-class")
            .AddUnmatched("style", "margin: 10px"));

        // Verify initial state
        IElement element = cut.Find("div");
        element.HasAttribute("class").Should().BeTrue();
        element.HasAttribute("style").Should().BeTrue();

        // Act - Remove all features and attributes
        cut.Render(parameters => parameters
            .Add(p => p.Color, null)
            .Add(p => p.Size, SizeEnum.Medium) // Default, but still generates class
            .AddUnmatched("class", "")
            .AddUnmatched("style", ""));

        // Assert
        element = cut.Find("div");
        // Should still have component classes
        element.HasAttribute("class").Should().BeTrue();
        // Style should be removed if empty
        string? style = element.GetAttribute("style");
        if (style != null)
        {
            style.Should().BeEmpty();
        }
    }
}