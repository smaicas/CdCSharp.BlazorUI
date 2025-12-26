using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Transitions;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.Transitions;

[Trait("Components", "Transitions")]
public class TransitionIntegrationTests : TestContextBase
{
    [Theory(DisplayName = "AllPresets_RenderSuccessfully")]
    [InlineData("HoverScale")]
    [InlineData("MaterialButton")]
    [InlineData("BounceIn")]
    [InlineData("GlassMorphism")]
    public void AllPresets_RenderSuccessfully(string presetName)
    {
        // Arrange
        UITransitions transitions = typeof(UITransitionPresets)
            .GetProperty(presetName)!
            .GetValue(null) as UITransitions
            ?? throw new InvalidOperationException($"Preset {presetName} not found");

        // Act & Assert - Should not throw
        Action act = () =>
        {
            IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
                .Add(p => p.Text, presetName)
                .Add(p => p.Transitions, transitions));
        };

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "Component_TransitionsWithCustomProperties_AppliedCorrectly")]
    public void Component_TransitionsWithCustomProperties_AppliedCorrectly()
    {
        // Arrange
        UITransitions glowTransition = UITransitionPresets.Create()
            .OnHover().Glow("rgba(255, 0, 0, 0.5)")
            .Build();

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Glow Button")
            .Add(p => p.Transitions, glowTransition));

        // Assert
        IElement button = cut.Find("button");
        string style = button.GetAttribute("style") ?? "";

        style.Should().Contain("--ui-transition-hover-color: rgba(255, 0, 0, 0.5)");
    }

    [Fact(DisplayName = "Component_WithMultipleTransitions_HasAllClasses")]
    public void Component_WithMultipleTransitions_HasAllClasses()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Interactive Button")
            .Add(p => p.Transitions, UITransitionPresets.Interactive));

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-has-transitions");

        // Interactive has hover, focus, and active
        string classes = button.GetAttribute("class") ?? "";
        classes.Should().Contain("ui-transition-hover");
        classes.Should().Contain("ui-transition-focus");
        classes.Should().Contain("ui-transition-active");
    }

    [Fact(DisplayName = "Component_WithoutTransitions_NoTransitionClasses")]
    public void Component_WithoutTransitions_NoTransitionClasses()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "No Transition"));

        // Assert
        IElement button = cut.Find("button");
        button.ShouldNotHaveClass("ui-has-transitions");

        string classes = button.GetAttribute("class") ?? "";
        classes.Should().NotContain("ui-transition-");
    }

    [Fact(DisplayName = "Component_WithTransitions_HasCorrectClasses")]
    public void Component_WithTransitions_HasCorrectClasses()
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Transition Button")
            .Add(p => p.Transitions, UITransitionPresets.HoverScale));

        // Assert
        IElement button = cut.Find("button");
        button.ShouldHaveClass("ui-has-transitions");
        button.ShouldHaveClass("ui-transition-hover-scale");
    }

    [Fact(DisplayName = "Component_WithTransitions_HasInlineStyles")]
    public void Component_WithTransitions_HasInlineStyles()
    {
        // Arrange
        UITransitions customTransition = UITransitionPresets.Create()
            .OnHover().Scale(1.5f, options =>
            {
                options.Duration = TimeSpan.FromMilliseconds(500);
                options.Delay = TimeSpan.FromMilliseconds(100);
                options.Easing = easing => easing.EaseInOut();
            })
            .Build();

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Text, "Custom Transition")
            .Add(p => p.Transitions, customTransition));

        // Assert
        IElement button = cut.Find("button");
        string style = button.GetAttribute("style") ?? "";

        style.Should().Contain("--ui-transition-hover-duration: 500ms");
        style.Should().Contain("--ui-transition-hover-delay: 100ms");
        style.Should().Contain("--ui-transition-hover-easing: ease-in-out");
        style.Should().Contain("--ui-transition-hover-scale: 1.5");
    }
}