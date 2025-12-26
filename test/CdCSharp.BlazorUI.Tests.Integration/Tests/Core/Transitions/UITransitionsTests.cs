using CdCSharp.BlazorUI.Components.Features.Transitions;
using CdCSharp.BlazorUI.Css;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.Transitions;

[Trait("Transitions", "UITransitions")]
public class UITransitionsTests
{
    [Fact(DisplayName = "AddTransition_AllowsMultipleTransitionsPerTrigger")]
    public void UITransitions_AddTransition_AllowsMultipleTransitionsPerTrigger()
    {
        // Arrange
        UITransitions transitions = new();
        TransitionConfig firstConfig = new() { Type = TransitionType.Scale };
        TransitionConfig secondConfig = new() { Type = TransitionType.Fade };

        // Act
        transitions.AddTransition(TransitionTrigger.Hover, firstConfig);
        transitions.AddTransition(TransitionTrigger.Hover, secondConfig);

        // Assert
        transitions.Transitions.Should().HaveCount(1);

        transitions.Transitions[TransitionTrigger.Hover]
            .Should().HaveCount(2);

        transitions.Transitions[TransitionTrigger.Hover]
            .Select(t => t.Type)
            .Should().ContainInOrder(
                TransitionType.Scale,
                TransitionType.Fade
            );
    }

    [Theory(DisplayName = "AllTriggerTypes_GenerateCorrectCssVariables")]
    [InlineData(TransitionTrigger.Hover, "hover")]
    [InlineData(TransitionTrigger.Focus, "focus")]
    [InlineData(TransitionTrigger.Active, "active")]
    [InlineData(TransitionTrigger.Disabled, "disabled")]
    public void UITransitions_AllTriggerTypes_GenerateCorrectCssVariables(TransitionTrigger trigger, string expectedPrefix)
    {
        // Arrange
        UITransitions transitions = new();
        TransitionConfig config = new()
        {
            Type = TransitionType.Scale,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        transitions.AddTransition(trigger, config);

        // Act
        Dictionary<string, string> styles = transitions.GetInlineStyles();

        // Assert
        styles.Should().ContainKey($"--ui-transition-{expectedPrefix}-duration");
    }

    [Fact(DisplayName = "GetCssClasses_Empty_ReturnsEmptyString")]
    public void UITransitions_GetCssClasses_Empty_ReturnsEmptyString()
    {
        // Arrange
        UITransitions transitions = new();

        // Act
        string cssClasses = transitions.GetCssClasses();

        // Assert
        cssClasses.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetCssClasses_ReturnsCorrectClasses")]
    public void UITransitions_GetCssClasses_ReturnsCorrectClasses()
    {
        // Arrange
        UITransitions transitions = new();
        transitions.AddTransition(TransitionTrigger.Hover, new TransitionConfig { Type = TransitionType.Scale });
        transitions.AddTransition(TransitionTrigger.Focus, new TransitionConfig { Type = TransitionType.Shadow });

        // Act
        string cssClasses = transitions.GetCssClasses();

        // Assert
        cssClasses.Should().Contain(CssClassesReference.Transition(TransitionTrigger.Hover, TransitionType.Scale));
        cssClasses.Should().Contain(CssClassesReference.Transition(TransitionTrigger.Focus, TransitionType.Shadow));
    }

    [Fact(DisplayName = "GetInlineStyles_Empty_ReturnsEmptyDictionary")]
    public void UITransitions_GetInlineStyles_Empty_ReturnsEmptyDictionary()
    {
        // Arrange
        UITransitions transitions = new();

        // Act
        Dictionary<string, string> styles = transitions.GetInlineStyles();

        // Assert
        styles.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetInlineStyles_IncludesCustomProperties")]
    public void UITransitions_GetInlineStyles_IncludesCustomProperties()
    {
        // Arrange
        UITransitions transitions = new();
        TransitionConfig config = new()
        {
            Type = TransitionType.Scale,
            CustomProperties = new Dictionary<string, string>
            {
                ["scale"] = "1.2",
                ["origin"] = "center"
            }
        };
        transitions.AddTransition(TransitionTrigger.Hover, config);

        // Act
        Dictionary<string, string> styles = transitions.GetInlineStyles();

        // Assert
        styles.Should().ContainKey("--ui-transition-hover-scale")
            .WhoseValue.Should().Be("1.2");
        styles.Should().ContainKey("--ui-transition-hover-origin")
            .WhoseValue.Should().Be("center");
    }

    [Fact(DisplayName = "GetInlineStyles_ReturnsCorrectStyles")]
    public void UITransitions_GetInlineStyles_ReturnsCorrectStyles()
    {
        // Arrange
        UITransitions transitions = new();
        TransitionConfig config = new()
        {
            Type = TransitionType.Scale,
            Duration = TimeSpan.FromMilliseconds(300),
            Delay = TimeSpan.FromMilliseconds(50),
            Easing = "ease-in-out"
        };
        transitions.AddTransition(TransitionTrigger.Hover, config);

        // Act
        Dictionary<string, string> styles = transitions.GetInlineStyles();

        // Assert
        styles.Should().ContainKey("--ui-transition-hover-duration")
            .WhoseValue.Should().Be("300ms");
        styles.Should().ContainKey("--ui-transition-hover-delay")
            .WhoseValue.Should().Be("50ms");
        styles.Should().ContainKey("--ui-transition-hover-easing")
            .WhoseValue.Should().Be("ease-in-out");
    }

    [Fact(DisplayName = "HasTransitions_ReturnsFalse_WhenEmpty")]
    public void UITransitions_HasTransitions_ReturnsFalse_WhenEmpty()
    {
        // Arrange
        UITransitions transitions = new();

        // Assert
        transitions.HasTransitions.Should().BeFalse();
        transitions.Transitions.Should().BeEmpty();
    }

    [Fact(DisplayName = "HasTransitions_ReturnsTrue_WhenHasTransitions")]
    public void UITransitions_HasTransitions_ReturnsTrue_WhenHasTransitions()
    {
        // Arrange
        UITransitions transitions = new();
        transitions.AddTransition(TransitionTrigger.Hover, new TransitionConfig { Type = TransitionType.Scale });

        // Assert
        transitions.HasTransitions.Should().BeTrue();
        transitions.Transitions.Should().HaveCount(1);
    }
}