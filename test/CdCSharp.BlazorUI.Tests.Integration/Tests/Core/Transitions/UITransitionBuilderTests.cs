using CdCSharp.BlazorUI.Components.Features.Transitions;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.Transitions;

[Trait("Transitions", "UITransitionBuilder")]
public class UITransitionBuilderTests
{
    [Theory(DisplayName = "AllTransitionTypes_CreateCorrectly")]
    [InlineData("Scale", TransitionType.Scale)]
    [InlineData("Rotate", TransitionType.Rotate)]
    [InlineData("Translate", TransitionType.Translate)]
    [InlineData("Fade", TransitionType.Fade)]
    [InlineData("Shadow", TransitionType.Shadow)]
    [InlineData("Blur", TransitionType.Blur)]
    [InlineData("BackdropBlur", TransitionType.BackdropBlur)]
    [InlineData("Lift", TransitionType.Lift)]
    [InlineData("Glow", TransitionType.Glow)]
    public void UITransitionBuilder_AllTransitionTypes_CreateCorrectly(string typeName, TransitionType expectedType)
    {
        // Arrange
        TriggerTransitionBuilder triggerBuilder = new UITransitionsBuilder().OnHover();

        // Act
        UITransitions transitions = typeName switch
        {
            "Scale" => triggerBuilder.Scale().Build(),
            "Rotate" => triggerBuilder.Rotate().Build(),
            "Translate" => triggerBuilder.Translate().Build(),
            "Fade" => triggerBuilder.Fade().Build(),
            "Shadow" => triggerBuilder.Shadow().Build(),
            "Blur" => triggerBuilder.Blur().Build(),
            "BackdropBlur" => triggerBuilder.BackdropBlur().Build(),
            "Lift" => triggerBuilder.Lift().Build(),
            "Glow" => triggerBuilder.Glow().Build(),
            _ => throw new ArgumentException($"Unknown type: {typeName}")
        };

        // Assert
        transitions.Transitions[TransitionTrigger.Hover]
            .Should().ContainSingle(t => t.Type == expectedType);
    }

    [Theory(DisplayName = "AllTriggers_CreateCorrectTransitions")]
    [InlineData("Hover")]
    [InlineData("Focus")]
    [InlineData("Active")]
    [InlineData("Disabled")]
    public void UITransitionBuilder_AllTriggers_CreateCorrectTransitions(string triggerName)
    {
        // Arrange
        UITransitionsBuilder builder = new();
        TransitionTrigger expectedTrigger = Enum.Parse<TransitionTrigger>(triggerName);

        // Act
        UITransitions transitions = triggerName switch
        {
            "Hover" => builder.OnHover().Scale().Build(),
            "Focus" => builder.OnFocus().Scale().Build(),
            "Active" => builder.OnActive().Scale().Build(),
            "Disabled" => builder.OnDisabled().Scale().Build(),
            _ => throw new ArgumentException($"Unknown trigger: {triggerName}")
        };

        // Assert
        transitions.Transitions.Should().ContainKey(expectedTrigger);
    }

    [Fact(DisplayName = "And_AllowsMultipleTriggers")]
    public void UITransitionBuilder_And_AllowsMultipleTriggers()
    {
        // Act
        UITransitions transitions = new UITransitionsBuilder()
            .OnHover().Scale()
            .And()
            .OnFocus().Shadow()
            .And()
            .OnActive().Fade()
            .Build();

        // Assert
        transitions.Transitions.Should().HaveCount(3);
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Hover);
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Focus);
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Active);
    }

    [Fact(DisplayName = "Build_FromTriggerTransitionBuilder_Works")]
    public void UITransitionBuilder_Build_FromTriggerTransitionBuilder_Works()
    {
        // Act
        UITransitions transitions = new UITransitionsBuilder()
            .OnHover()
            .Scale()
            .Build();

        // Assert
        transitions.HasTransitions.Should().BeTrue();
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Hover);
    }

    [Fact(DisplayName = "ComplexChain_BuildsCorrectly")]
    public void UITransitionBuilder_ComplexChain_BuildsCorrectly()
    {
        // Act
        UITransitions transitions = new UITransitionsBuilder()
            .OnHover().Scale(1.1f, options =>
            {
                options.Duration = TimeSpan.FromMilliseconds(200);
                options.Easing = easing => easing.CubicBezier().MaterialStandard();
            })
            .And()
            .OnFocus().Shadow("0 0 0 3px rgba(59, 130, 246, 0.3)")
            .And()
            .OnActive().Scale(0.98f, options =>
            {
                options.Duration = TimeSpan.FromMilliseconds(50);
                options.Easing = easing => easing.EaseOut();
            })
            .And()
            .OnDisabled().Fade(0.5f)
            .Build();

        // Assert
        transitions.Transitions.Should().HaveCount(4);

        TransitionConfig hover = transitions.Transitions[TransitionTrigger.Hover].Single();
        hover.Type.Should().Be(TransitionType.Scale);
        hover.Duration.Should().Be(TimeSpan.FromMilliseconds(200));
        hover.Easing.Should().Be("cubic-bezier(0.400, 0.000, 0.200, 1.000)");

        transitions.Transitions[TransitionTrigger.Focus]
            .Should().ContainSingle(t => t.Type == TransitionType.Shadow);

        TransitionConfig active = transitions.Transitions[TransitionTrigger.Active].Single();
        active.Type.Should().Be(TransitionType.Scale);
        active.Duration.Should().Be(TimeSpan.FromMilliseconds(50));
        active.Easing.Should().Be("ease-out");

        transitions.Transitions[TransitionTrigger.Disabled]
            .Should().ContainSingle(t => t.Type == TransitionType.Fade);
    }

    [Fact(DisplayName = "CustomProperties_StoredCorrectly")]
    public void UITransitionBuilder_CustomProperties_StoredCorrectly()
    {
        // Act
        UITransitions transitions = new UITransitionsBuilder()
            .OnHover().Scale(1.5f)
            .And()
            .OnHover().Rotate("45deg")
            .And()
            .OnHover().Shadow("0 10px 20px rgba(0,0,0,0.3)")
            .Build();

        // Assert
        transitions.Transitions[TransitionTrigger.Hover]
            .Should().HaveCount(3);

        transitions.Transitions[TransitionTrigger.Hover]
            .Select(t => t.Type)
            .Should().ContainInOrder(
                TransitionType.Scale,
                TransitionType.Rotate,
                TransitionType.Shadow
            );

        TransitionConfig shadowConfig = transitions.Transitions[TransitionTrigger.Hover].Last();
        shadowConfig.CustomProperties["shadow"]
            .Should().Be("0 10px 20px rgba(0,0,0,0.3)");
    }

    [Fact(DisplayName = "EasingBuilder_ConvertedToString")]
    public void UITransitionBuilder_EasingBuilder_ConvertedToString()
    {
        // Act
        UITransitions transitions = new UITransitionsBuilder()
            .OnHover().Scale(1.2f, options =>
            {
                options.Easing = easing => easing.CubicBezier().MaterialStandard();
            })
            .Build();

        // Assert
        TransitionConfig config = transitions.Transitions[TransitionTrigger.Hover].Single();
        config.Easing.Should().Be("cubic-bezier(0.400, 0.000, 0.200, 1.000)");
    }

    [Fact(DisplayName = "OnHover_CreatesHoverTransition")]
    public void UITransitionBuilder_OnHover_CreatesHoverTransition()
    {
        // Act
        UITransitions transitions = new UITransitionsBuilder()
            .OnHover().Scale()
            .Build();

        // Assert
        transitions.HasTransitions.Should().BeTrue();
        transitions.Transitions.Should().ContainKey(TransitionTrigger.Hover);
        transitions.Transitions[TransitionTrigger.Hover]
            .Should().ContainSingle(t => t.Type == TransitionType.Scale);
    }

    [Fact(DisplayName = "TransitionOptions_AppliedCorrectly")]
    public void UITransitionBuilder_TransitionOptions_AppliedCorrectly()
    {
        // Act
        UITransitions transitions = new UITransitionsBuilder()
            .OnHover().Scale(1.2f, options =>
            {
                options.Duration = TimeSpan.FromMilliseconds(500);
                options.Delay = TimeSpan.FromMilliseconds(100);
                options.Easing = easing => easing.Custom("custom-easing");
            })
            .Build();

        // Assert
        TransitionConfig config = transitions.Transitions[TransitionTrigger.Hover].Single();
        config.Duration.Should().Be(TimeSpan.FromMilliseconds(500));
        config.Delay.Should().Be(TimeSpan.FromMilliseconds(100));
        config.Easing.Should().Be("custom-easing");
    }
}