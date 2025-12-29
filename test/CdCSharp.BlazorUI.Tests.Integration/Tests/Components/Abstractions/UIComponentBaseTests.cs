using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Components.Features.Transitions;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Abstractions;

[Trait("Abstractions", "UIComponentBase")]
public class UIComponentBaseTests : TestContextBase
{
    [Fact(DisplayName = "RenderedElement_ContainsAllDataAttributes")]
    public async Task UIComponentBase_RenderedElement_ContainsAllDataAttributes()
    {
        // Arrange & Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.IsPrimary, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
            { "data-custom", "user-value" },
            { "id", "test-id" },
            { "class", "user-class" }
            }));

        // Assert - verify in the rendered DOM
        IElement element = cut.Find("[data-ui-component]");

        // Verify component data attributes
        element.ShouldHaveDataAttribute("ui-component", "test-component");
        element.ShouldHaveDataAttribute("ui-variant", "primary"); // Assuming IsPrimary maps to variant

        // Verify additional attributes are merged
        element.ShouldHaveDataAttribute("custom", "user-value");
        element.GetAttribute("id").Should().Be("test-id");

        // If classes are still supported
        element.GetAttribute("class").Should().Contain("user-class");
    }

    [Fact(DisplayName = "ConditionalClasses_AppliedCorrectly")]
    public void UIComponentBase_ConditionalClasses_AppliedCorrectly()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.IsPrimary, true));

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("class").Should().Be("test-component test-component--primary");
    }

    [Fact(DisplayName = "EmptyUserClass_HandledCorrectly")]
    public void UIComponentBase_EmptyUserClass_HandledCorrectly()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "" }
            }));

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("class").Should().Be("test-component");
    }

    [Fact(DisplayName = "InlineStyles_AppliedCorrectly")]
    public void UIComponentBase_InlineStyles_AppliedCorrectly()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.Color, "red")
            .Add(p => p.BackgroundColor, "blue"));

        // Assert
        IElement div = cut.Find("div");
        string? style = div.GetAttribute("style");
        style.Should().Contain("color: red");
        style.Should().Contain("background-color: blue");
    }

    [Fact(DisplayName = "OtherAttributes_PassedThrough")]
    public void UIComponentBase_OtherAttributes_PassedThrough()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "id", "test-id" },
                { "data-test", "value" },
                { "aria-label", "Test component" }
            }));

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("id").Should().Be("test-id");
        div.GetAttribute("data-test").Should().Be("value");
        div.GetAttribute("aria-label").Should().Be("Test component");
    }

    [Fact(DisplayName = "UserStyles_MergedWithComponentStyles")]
    public void UIComponentBase_UserStyles_MergedWithComponentStyles()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.Color, "red")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "style", "margin: 10px" }
            }));

        // Assert
        IElement div = cut.Find("div");
        string? style = div.GetAttribute("style");
        style.Should().Contain("color: red");
        style.Should().Contain("margin: 10px");
    }

    [Fact(DisplayName = "WithoutUserClasses_RendersOnlyComponentClasses")]
    public void UIComponentBase_WithoutUserClasses_RendersOnlyComponentClasses()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>();

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("class").Should().Be("test-component");
    }

    [Fact(DisplayName = "WithUserClasses_MergesClasses")]
    public void UIComponentBase_WithUserClasses_MergesClasses()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-class-1 user-class-2" }
            }));

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("class").Should().Be("test-component user-class-1 user-class-2");
    }

    #region CSS Classes Tests

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

        // Assert - All new features applied and old ones removed
        element = cut.Find("div");
        element.ShouldHaveClass("ui-size-large");
        element.ShouldNotHaveClass("ui-size-small"); // This should now pass
        element.ShouldNotHaveClass("ui-elevation-0"); // This should now pass
        element.ShouldHaveClass("ui-elevation-12");
        element.ShouldHaveClass("ui-full-width");
        element.ShouldHaveClass("ui-loading");
        cut.Find(".test-loading-indicator").Should().NotBeNull();
    }

    [Fact(DisplayName = "UserClasses_PreservedAcrossRerenders")]
    public void UserClasses_PreservedAcrossRerenders()
    {
        // Arrange - Component with user classes
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Small)
            .AddUnmatched("class", "user-custom-class another-class"));

        // Initial state
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-size-small");
        element.ShouldHaveClass("user-custom-class");
        element.ShouldHaveClass("another-class");

        // Act - Change component state
        cut.Render(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large));

        // Assert - User classes preserved, component classes updated
        element = cut.Find("div");
        element.ShouldHaveClass("ui-size-large");
        element.ShouldNotHaveClass("ui-size-small");
        element.ShouldHaveClass("user-custom-class");
        element.ShouldHaveClass("another-class");
    }

    [Fact(DisplayName = "MultipleRerenders_NoClassAccumulation")]
    public void MultipleRerenders_NoClassAccumulation()
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Small)
            .AddUnmatched("class", "user-class"));

        // Act - Multiple rerenders with different states
        for (int i = 0; i < 5; i++)
        {
            cut.Render(parameters => parameters
                .Add(p => p.Size, i % 2 == 0 ? SizeEnum.Small : SizeEnum.Large));
        }

        // Assert - No duplicate classes
        IElement element = cut.Find("div");
        List<string> classList = element.ClassList.ToList();

        // Should have exactly one size class
        classList.Count(c => c.StartsWith("ui-size-")).Should().Be(1);

        // Should have the user class exactly once
        classList.Count(c => c == "user-class").Should().Be(1);

        // Final state should be Small (5 iterations, last is even)
        element.ShouldHaveClass("ui-size-small");
        element.ShouldNotHaveClass("ui-size-large");
    }

    [Fact(DisplayName = "UserClasses_UpdatedExternally_RecognizedCorrectly")]
    public void UserClasses_UpdatedExternally_RecognizedCorrectly()
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .AddUnmatched("class", "original-class"));

        // Initial state
        IElement element = cut.Find("div");
        element.ShouldHaveClass("original-class");

        // Act - Update user classes externally
        cut.Render(parameters => parameters
            .AddUnmatched("class", "new-class another-new-class"));

        // Assert - New user classes recognized
        element = cut.Find("div");
        element.ShouldHaveClass("new-class");
        element.ShouldHaveClass("another-new-class");
        element.ShouldNotHaveClass("original-class");
    }

    [Fact(DisplayName = "DefaultValues_ApplyCorrectClasses")]
    public void DefaultValues_ApplyCorrectClasses()
    {
        // Arrange - Component with default values
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>();

        // Assert - Should have default feature classes
        IElement element = cut.Find("div");
        element.ShouldHaveClass("test-feature-component"); // Base component class
        element.ShouldHaveClass("ui-size-medium"); // Default size
        element.ShouldHaveClass("ui-density-standard"); // Default density
        element.ShouldHaveClass("ui-has-ripple"); // Ripple enabled by default
        element.ShouldHaveClass("ui-has-ripple"); // Ripple enabled by default

        // Should NOT have classes for features that are off/zero by default
        element.ShouldNotHaveClass("ui-full-width"); // FullWidth = false by default
        element.ShouldNotHaveClass("ui-loading"); // IsLoading = false by default
        element.ShouldNotHaveClassWithPrefix("ui-elevation-");
    }

    [Fact(DisplayName = "MinimalComponent_NoFeatureClasses")]
    public void MinimalComponent_NoFeatureClasses()
    {
        // Arrange - Truly minimal component with no feature interfaces
        IRenderedComponent<MinimalTestComponent> cut = Render<MinimalTestComponent>();

        // Assert - Should have only the base component class, no ui- classes
        IElement element = cut.Find("div");
        element.ShouldHaveClass("minimal-component");
        element.ClassList.Should().NotContain(c => c.StartsWith("ui-"),
            because: "a component with no feature interfaces should not have ui- classes");
    }

    [Theory(DisplayName = "DynamicFeatures_ToggleCorrectly")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void DynamicFeatures_ToggleCorrectly(bool initialState, bool finalState)
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.FullWidth, initialState)
            .Add(p => p.DisableRipple, !initialState));

        // Initial assertion
        IElement element = cut.Find("div");
        if (initialState)
        {
            element.ShouldHaveClass("ui-full-width");
            element.ShouldHaveClass("ui-has-ripple");
        }
        else
        {
            element.ShouldNotHaveClass("ui-full-width");
            element.ShouldNotHaveClass("ui-has-ripple");
        }

        // Act - Toggle states
        cut.Render(parameters => parameters
            .Add(p => p.FullWidth, finalState)
            .Add(p => p.DisableRipple, !finalState));

        // Final assertion
        if (finalState)
        {
            element.ShouldHaveClass("ui-full-width");
            element.ShouldHaveClass("ui-has-ripple");
        }
        else
        {
            element.ShouldNotHaveClass("ui-full-width");
            element.ShouldNotHaveClass("ui-has-ripple");
        }
    }

    #endregion

    #region Inline Styles Tests

    [Fact(DisplayName = "InlineStyles_NoAccumulationOnRerender")]
    public void InlineStyles_NoAccumulationOnRerender()
    {
        // Arrange - Component with user styles and transitions
        UITransitions transitions = UITransitionPresets.Create()
            .OnHover().Scale(1.1f)
            .Build();

        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Transitions, transitions)
            .AddUnmatched("style", "color: red; margin: 10px"));

        // Initial state
        IElement element = cut.Find("div");
        string? initialStyle = element.GetAttribute("style");
        initialStyle.Should().Contain("color: red");
        initialStyle.Should().Contain("margin: 10px");
        initialStyle.Should().Contain("--ui-transition-hover-scale");

        // Count occurrences
        initialStyle.Split(';').Count(s => s.Trim().StartsWith("color:")).Should().Be(1);

        // Act - Rerender with different transitions
        UITransitions newTransitions = UITransitionPresets.Create()
            .OnHover().Fade(0.5f)
            .Build();

        cut.Render(parameters => parameters
            .Add(p => p.Transitions, newTransitions));

        // Assert - Old transition styles removed, new ones added, user styles preserved
        element = cut.Find("div");
        string? updatedStyle = element.GetAttribute("style");
        updatedStyle.Should().Contain("color: red");
        updatedStyle.Should().Contain("margin: 10px");
        updatedStyle.Should().Contain("--ui-transition-hover-opacity");
        updatedStyle.Should().NotContain("--ui-transition-hover-scale");

        // Ensure no duplicate styles
        int styleCount = updatedStyle.Split(';').Count(s => s.Trim().StartsWith("color:"));
        styleCount.Should().Be(1);
    }

    [Fact(DisplayName = "UserStyles_UpdatedExternally_RecognizedCorrectly")]
    public void UserStyles_UpdatedExternally_RecognizedCorrectly()
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .AddUnmatched("style", "background: blue"));

        // Initial state
        IElement element = cut.Find("div");
        element.GetAttribute("style").Should().Contain("background: blue");

        // Act - Update user styles externally
        cut.Render(parameters => parameters
            .AddUnmatched("style", "background: green; padding: 5px"));

        // Assert - New user styles recognized
        element = cut.Find("div");
        string? style = element.GetAttribute("style");
        style.Should().Contain("background: green");
        style.Should().Contain("padding: 5px");
        style.Should().NotContain("background: blue");
    }

    [Fact(DisplayName = "ComponentStyles_CombineWithUserStyles")]
    public void ComponentStyles_CombineWithUserStyles()
    {
        // Arrange - Component with color (generates inline style) and user styles
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Color, UIColor.Red.Default)
            .AddUnmatched("style", "font-size: 20px"));

        // Assert
        IElement element = cut.Find("div");
        string? style = element.GetAttribute("style");
        style.Should().Contain($"color: {UIColor.Red.Default}");
        style.Should().Contain("font-size: 20px");
    }

    [Fact(DisplayName = "RippleStyles_AddedCorrectly")]
    public void RippleStyles_AddedCorrectly()
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.DisableRipple, false)
            .Add(p => p.RippleColor, UIColor.Blue.Default)
            .Add(p => p.RippleDuration, 800));

        // Assert
        IElement element = cut.Find("div");
        string? style = element.GetAttribute("style");
        style.Should().Contain("--ui-ripple-color");
        style.Should().Contain(UIColor.Blue.Default.ToString(ColorOutputFormats.Rgba));
        style.Should().Contain("--ui-ripple-duration: 800ms");
    }

    [Fact(DisplayName = "EmptyStyles_RemoveAttributeCompletely")]
    public void EmptyStyles_RemoveAttributeCompletely()
    {
        // Arrange - Start with styles
        UITransitions transitions = UITransitionPresets.HoverScale;
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Transitions, transitions));

        // Verify initial state has styles
        IElement element = cut.Find("div");
        element.HasAttribute("style").Should().BeTrue();

        // Act - Remove all style-generating features
        cut.Render(parameters => parameters
            .Add(p => p.Transitions, null));

        // Assert - Style attribute should be removed
        element = cut.Find("div");
        element.HasAttribute("style").Should().BeFalse();
    }

    #endregion

    #region Complex Scenarios

    [Fact(DisplayName = "ComplexScenario_MultipleFeaturesAndRerenders")]
    public void ComplexScenario_MultipleFeaturesAndRerenders()
    {
        // Arrange - Complex initial state
        UITransitions transitions = UITransitionPresets.Create()
            .OnHover().Scale(1.2f).Shadow("0 4px 8px rgba(0,0,0,0.2)")
            .And().OnFocus().Shadow("0 0 0 3px rgba(0,0,0,0.1)")
            .Build();

        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Medium)
            .Add(p => p.Density, DensityEnum.Comfortable)
            .Add(p => p.Elevation, 4)
            .Add(p => p.Transitions, transitions)
            .Add(p => p.DisableRipple, false)
            .Add(p => p.RippleColor, UIColor.Purple.Default)
            .AddUnmatched("class", "custom-class")
            .AddUnmatched("style", "margin: 20px"));

        // Initial verification
        IElement element = cut.Find("div");
        List<string> initialClasses = element.ClassList.ToList();
        string? initialStyles = element.GetAttribute("style");

        // Should have all expected classes
        element.ShouldHaveClass("ui-size-medium");
        element.ShouldHaveClass("ui-density-comfortable");
        element.ShouldHaveClass("ui-elevation-4");
        element.ShouldHaveClass("ui-has-transitions");
        element.ShouldHaveClass("ui-has-ripple");
        element.ShouldHaveClass("custom-class");

        // Should have all expected styles
        initialStyles.Should().Contain("margin: 20px");
        initialStyles.Should().Contain("--ui-transition-hover-scale");
        initialStyles.Should().Contain("--ui-ripple-color");

        // Act - Multiple complex updates
        cut.Render(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.IsLoading, true));

        cut.Render(parameters => parameters
            .Add(p => p.Density, DensityEnum.Compact)
            .Add(p => p.Elevation, 8));

        cut.Render(parameters => parameters
            .Add(p => p.DisableRipple, true)
            .Add(p => p.IsLoading, false));

        // Final assertions
        element = cut.Find("div");
        List<string> finalClasses = element.ClassList.ToList();

        // Correct classes after all updates
        element.ShouldHaveClass("ui-size-large");
        element.ShouldNotHaveClass("ui-size-medium");
        element.ShouldHaveClass("ui-density-compact");
        element.ShouldNotHaveClass("ui-density-comfortable");
        element.ShouldHaveClass("ui-elevation-8");
        element.ShouldNotHaveClass("ui-elevation-4");
        element.ShouldNotHaveClass("ui-has-ripple"); // Disabled
        element.ShouldNotHaveClass("ui-loading"); // Loading ended
        element.ShouldHaveClass("custom-class"); // User class preserved

        // No duplicate classes
        finalClasses.GroupBy(c => c).Any(g => g.Count() > 1).Should().BeFalse();

        // Styles updated correctly
        string? finalStyles = element.GetAttribute("style");
        finalStyles.Should().Contain("margin: 20px"); // User style preserved
        finalStyles.Should().NotContain("--ui-ripple-color"); // Ripple disabled
    }

    [Fact(DisplayName = "EdgeCase_RapidStateChanges")]
    public void EdgeCase_RapidStateChanges()
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>();

        // Act - Rapid state changes
        SizeEnum[] sizes = new[] { SizeEnum.Small, SizeEnum.Medium, SizeEnum.Large };
        DensityEnum[] densities = new[] { DensityEnum.Comfortable, DensityEnum.Standard, DensityEnum.Compact };

        for (int i = 0; i < 10; i++)
        {
            cut.Render(parameters => parameters
                .Add(p => p.Size, sizes[i % 3])
                .Add(p => p.Density, densities[i % 3])
                .Add(p => p.Elevation, (i * 2) % 24)
                .Add(p => p.FullWidth, i % 2 == 0));
        }

        // Assert - Final state is consistent
        IElement element = cut.Find("div");
        List<string> classes = element.ClassList.ToList();

        // Should have exactly one of each type
        classes.Count(c => c.StartsWith("ui-size-")).Should().Be(1);
        classes.Count(c => c.StartsWith("ui-density-")).Should().Be(1);
        classes.Count(c => c.StartsWith("ui-elevation-")).Should().Be(1);

        // Final values (i=9)
        element.ShouldHaveClass("ui-size-small"); // 9 % 3 = 0
        element.ShouldHaveClass("ui-density-comfortable"); // 9 % 3 = 0
        element.ShouldHaveClass("ui-elevation-18"); // (9 * 2) % 24 = 18
        element.ShouldNotHaveClass("ui-full-width"); // 9 % 2 = 1 (false)
    }

    [Fact(DisplayName = "Border_WithAllFeatures_AppliesCorrectly")]
    public void Border_WithAllFeatures_AppliesCorrectly()
    {
        // Arrange
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Border, new BorderStyle("2px", BorderStyleType.Solid, UIColor.Palette.Primary, 12))
            .Add(p => p.BorderTop, new BorderStyle("4px", BorderStyleType.Dashed, UIColor.Red.Default))
            .AddUnmatched("style", "margin: 10px"));

        // Assert
        IElement element = cut.Find("div");
        string? style = element.GetAttribute("style");

        // All borders should be present
        style.Should().Contain("border: 2px solid var(--palette-primary)");
        style.Should().Contain("border-top: 4px dashed");
        style.Should().Contain("border-radius: 12px");
        // User style preserved
        style.Should().Contain("margin: 10px");
    }

    #endregion

    #region Transition Classes Tests

    [Fact(DisplayName = "TransitionClasses_UpdateCorrectly")]
    public void TransitionClasses_UpdateCorrectly()
    {
        // Arrange - Start with one transition
        UITransitions initialTransition = UITransitionPresets.HoverScale;
        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Transitions, initialTransition));

        // Initial state
        IElement element = cut.Find("div");
        element.ShouldHaveClass("ui-has-transitions");
        element.ShouldHaveClass("ui-transition-hover-scale");

        // Act - Change to different transition
        UITransitions newTransition = UITransitionPresets.Create()
            .OnHover().Fade(0.8f).Rotate("10deg")
            .Build();

        cut.Render(parameters => parameters
            .Add(p => p.Transitions, newTransition));

        // Assert - Old transition classes removed, new ones added
        element = cut.Find("div");
        element.ShouldHaveClass("ui-has-transitions");
        element.ShouldNotHaveClass("ui-transition-hover-scale");
        element.ShouldHaveClass("ui-transition-hover-fade");
        element.ShouldHaveClass("ui-transition-hover-rotate");
    }

    #endregion

    #region Performance Tests

    [Fact(DisplayName = "Performance_LargeNumberOfClasses")]
    public void Performance_LargeNumberOfClasses()
    {
        // Arrange - Component with many user classes
        string userClasses = string.Join(" ", Enumerable.Range(1, 50).Select(i => $"user-class-{i}"));

        IRenderedComponent<TestFeatureComponent> cut = Render<TestFeatureComponent>(parameters => parameters
            .Add(p => p.Size, SizeEnum.Large)
            .Add(p => p.Elevation, 12)
            .AddUnmatched("class", userClasses));

        // Act - Multiple rerenders
        for (int i = 0; i < 10; i++)
        {
            cut.Render(parameters => parameters
                .Add(p => p.Size, i % 2 == 0 ? SizeEnum.Small : SizeEnum.Large));
        }

        // Assert - All user classes preserved, no duplicates
        IElement element = cut.Find("div");
        List<string> classes = element.ClassList.ToList();

        // All 50 user classes should be present
        for (int i = 1; i <= 50; i++)
        {
            element.ShouldHaveClass($"user-class-{i}");
        }

        // No duplicates
        classes.GroupBy(c => c).Any(g => g.Count() > 1).Should().BeFalse();
    }

    #endregion
}