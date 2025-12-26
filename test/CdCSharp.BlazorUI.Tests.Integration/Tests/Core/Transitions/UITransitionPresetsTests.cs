using CdCSharp.BlazorUI.Components.Features.Transitions;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.Transitions;

[Trait("Transitions", "UITransitionPresets")]
public class UITransitionPresetsTests
{
    [Theory(DisplayName = "AllPresets_HaveTransitions")]
    [InlineData("HoverScale")]
    [InlineData("HoverFade")]
    [InlineData("HoverShadow")]
    [InlineData("HoverLift")]
    [InlineData("HoverGlow")]
    [InlineData("FocusRing")]
    [InlineData("Interactive")]
    [InlineData("ModernGlass")]
    [InlineData("MaterialButton")]
    [InlineData("Pulse")]
    [InlineData("Skeleton")]
    [InlineData("BounceIn")]
    [InlineData("ElasticScale")]
    [InlineData("Wiggle")]
    [InlineData("Shake")]
    [InlineData("Neumorphism")]
    [InlineData("GlassMorphism")]
    [InlineData("CardHover")]
    [InlineData("ColorShift")]
    [InlineData("Perspective")]
    [InlineData("AccessibleFocus")]
    [InlineData("DisabledState")]
    [InlineData("PremiumButton")]
    public void UITransitionPresets_AllPresets_HaveTransitions(string presetName)
    {
        // Arrange
        UITransitions transitions = typeof(UITransitionPresets)
            .GetProperty(presetName)!
            .GetValue(null) as UITransitions
            ?? throw new InvalidOperationException($"Preset {presetName} not found");

        // Assert
        transitions.Should().NotBeNull();
        transitions.HasTransitions.Should().BeTrue();
        transitions.Transitions.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Create_ReturnsNewBuilder")]
    public void UITransitionPresets_Create_ReturnsNewBuilder()
    {
        // Act
        UITransitionsBuilder builder = UITransitionPresets.Create();

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<UITransitionsBuilder>();
    }

    [Fact(DisplayName = "DisabledState_HasDisabledTrigger")]
    public void UITransitionPresets_DisabledState_HasDisabledTrigger()
    {
        UITransitions transitions = UITransitionPresets.DisabledState;

        transitions.Transitions.Should().ContainKey(TransitionTrigger.Disabled);

        transitions.Transitions[TransitionTrigger.Disabled]
            .Select(t => t.Type)
            .Should()
            .Contain(new[] { TransitionType.Fade, TransitionType.Blur });
    }

    [Fact(DisplayName = "GlassMorphism_HoverHasMultipleTransitions")]
    public void UITransitionPresets_GlassMorphism_HoverHasMultipleTransitions()
    {
        UITransitions transitions = UITransitionPresets.GlassMorphism;

        transitions.Transitions[TransitionTrigger.Hover]
            .Select(t => t.Type)
            .Should()
            .Contain(new[]
            {
            TransitionType.BackdropBlur,
            TransitionType.Shadow,
            TransitionType.Scale
            });
    }

    [Fact(DisplayName = "Interactive_HasMultipleTriggers")]
    public void UITransitionPresets_Interactive_HasMultipleTriggers()
    {
        // Act
        UITransitions transitions = UITransitionPresets.Interactive;

        // Assert
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Hover);
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Focus);
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Active);
    }

    [Theory(DisplayName = "LoadingPresets_HaveAppropriateProperties")]
    [InlineData("Pulse")]
    [InlineData("Skeleton")]
    public void UITransitionPresets_LoadingPresets_HaveAppropriateProperties(string presetName)
    {
        // Arrange
        UITransitions transitions = typeof(UITransitionPresets)
            .GetProperty(presetName)!
            .GetValue(null) as UITransitions
            ?? throw new InvalidOperationException($"Preset {presetName} not found");

        // Assert
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Hover);

        TransitionConfig config = transitions.Transitions[TransitionTrigger.Hover]
            .Should().ContainSingle().Which;

        config.Duration?.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(1000);
    }

    [Fact(DisplayName = "MaterialButton_HasCustomEasing")]
    public void UITransitionPresets_MaterialButton_HasCustomEasing()
    {
        UITransitions transitions = UITransitionPresets.MaterialButton;

        transitions.Transitions[TransitionTrigger.Hover]
            .Should().ContainSingle()
            .Which.Easing.Should().StartWith("cubic-bezier");

        transitions.Transitions[TransitionTrigger.Active]
            .Should().ContainSingle()
            .Which.Easing.Should().StartWith("cubic-bezier");
    }

    [Fact(DisplayName = "PremiumButton_HasAllInteractiveTriggers")]
    public void UITransitionPresets_PremiumButton_HasAllInteractiveTriggers()
    {
        // Act
        UITransitions transitions = UITransitionPresets.PremiumButton;

        // Assert
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Hover);
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Active);

        transitions.Transitions
            .SelectMany(t => t.Value)
            .Count(t => t.Type == TransitionType.Scale)
            .Should().BeGreaterThanOrEqualTo(1);
    }
}