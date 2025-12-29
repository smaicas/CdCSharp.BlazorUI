using CdCSharp.BlazorUI.Components.Features.Transitions;
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
        Dictionary<string, string> cssVariables = transitions.GetCssVariables();

        // Assert
        cssVariables.Should().ContainKey($"--ui-transition-{expectedPrefix}-duration");
    }

    [Fact(DisplayName = "GetDataAttributeValue_Empty_ReturnsEmptyString")]
    public void UITransitions_GetDataAttributeValue_Empty_ReturnsEmptyString()
    {
        // Arrange
        UITransitions transitions = new();

        // Act
        string dataAttributeValue = transitions.GetDataAttributeValue();

        // Assert
        dataAttributeValue.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetDataAttributeValue_ReturnsCorrectValue")]
    public void UITransitions_GetDataAttributeValue_ReturnsCorrectValue()
    {
        // Arrange
        UITransitions transitions = new();
        transitions.AddTransition(TransitionTrigger.Hover, new TransitionConfig { Type = TransitionType.Scale });
        transitions.AddTransition(TransitionTrigger.Focus, new TransitionConfig { Type = TransitionType.Shadow });

        // Act
        string dataAttributeValue = transitions.GetDataAttributeValue();

        // Assert
        dataAttributeValue.Should().Contain("ui-transition-hover-scale");
        dataAttributeValue.Should().Contain("ui-transition-focus-shadow");
        string[] values = dataAttributeValue.Split(' ');
        values.Should().HaveCount(2);
    }

    [Fact(DisplayName = "GetDataAttributeValue_MultipleTransitionsPerTrigger")]
    public void UITransitions_GetDataAttributeValue_MultipleTransitionsPerTrigger()
    {
        // Arrange
        UITransitions transitions = new();
        transitions.AddTransition(TransitionTrigger.Hover, new TransitionConfig { Type = TransitionType.Scale });
        transitions.AddTransition(TransitionTrigger.Hover, new TransitionConfig { Type = TransitionType.Fade });

        // Act
        string dataAttributeValue = transitions.GetDataAttributeValue();

        // Assert
        dataAttributeValue.Should().Be("ui-transition-hover-scale ui-transition-hover-fade");
    }

    [Fact(DisplayName = "GetCssVariables_Empty_ReturnsEmptyDictionary")]
    public void UITransitions_GetCssVariables_Empty_ReturnsEmptyDictionary()
    {
        // Arrange
        UITransitions transitions = new();

        // Act
        Dictionary<string, string> cssVariables = transitions.GetCssVariables();

        // Assert
        cssVariables.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetCssVariables_IncludesCustomProperties")]
    public void UITransitions_GetCssVariables_IncludesCustomProperties()
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
        Dictionary<string, string> cssVariables = transitions.GetCssVariables();

        // Assert
        cssVariables.Should().ContainKey("--ui-transition-hover-scale")
            .WhoseValue.Should().Be("1.2");
        cssVariables.Should().ContainKey("--ui-transition-hover-origin")
            .WhoseValue.Should().Be("center");
    }

    [Fact(DisplayName = "GetCssVariables_ReturnsCorrectVariables")]
    public void UITransitions_GetCssVariables_ReturnsCorrectVariables()
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
        Dictionary<string, string> cssVariables = transitions.GetCssVariables();

        // Assert
        cssVariables.Should().ContainKey("--ui-transition-hover-duration")
            .WhoseValue.Should().Be("300ms");
        cssVariables.Should().ContainKey("--ui-transition-hover-delay")
            .WhoseValue.Should().Be("50ms");
        cssVariables.Should().ContainKey("--ui-transition-hover-easing")
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