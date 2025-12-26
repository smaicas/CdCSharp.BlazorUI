using CdCSharp.BlazorUI.Components.Features.Behaviors;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Behaviors;

[Trait("Behaviors", "BehaviorConfiguration")]
public class BehaviorConfigurationTests
{
    [Fact(DisplayName = "HasAnyBehavior_WithRipple_ReturnsTrue")]
    public void BehaviorConfiguration_HasAnyBehavior_WithRipple_ReturnsTrue()
    {
        // Arrange
        BehaviorConfiguration config = new()
        {
            Ripple = new RippleConfiguration()
        };

        // Assert
        config.HasAnyBehavior.Should().BeTrue();
    }

    [Fact(DisplayName = "HasAnyBehavior_Empty_ReturnsFalse")]
    public void BehaviorConfiguration_HasAnyBehavior_Empty_ReturnsFalse()
    {
        // Arrange
        BehaviorConfiguration config = new();

        // Assert
        config.HasAnyBehavior.Should().BeFalse();
    }

    [Fact(DisplayName = "RippleConfiguration_Properties")]
    public void RippleConfiguration_Properties()
    {
        // Arrange
        RippleConfiguration config = new()
        {
            Color = "rgba(0,0,0,0.5)",
            Duration = 300
        };

        // Assert
        config.Color.Should().Be("rgba(0,0,0,0.5)");
        config.Duration.Should().Be(300);
    }
}
