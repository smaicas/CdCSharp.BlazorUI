using CdCSharp.BlazorUI.Components;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Library;

/// <summary>
/// LIB-06: BUITransitionPresets produce valid data-bui-transitions values and CSS variables.
/// </summary>
[Trait("Library", "BUITransitionPresets")]
public class BUITransitionPresetsTests
{
    [Theory]
    [InlineData(nameof(BUITransitionPresets.HoverScale))]
    [InlineData(nameof(BUITransitionPresets.HoverShadow))]
    [InlineData(nameof(BUITransitionPresets.HoverFade))]
    [InlineData(nameof(BUITransitionPresets.HoverLift))]
    [InlineData(nameof(BUITransitionPresets.HoverGlow))]
    [InlineData(nameof(BUITransitionPresets.CardHover))]
    [InlineData(nameof(BUITransitionPresets.FocusRing))]
    [InlineData(nameof(BUITransitionPresets.Interactive))]
    [InlineData(nameof(BUITransitionPresets.MaterialButton))]
    [InlineData(nameof(BUITransitionPresets.PremiumButton))]
    [InlineData(nameof(BUITransitionPresets.GlassMorphism))]
    [InlineData(nameof(BUITransitionPresets.Neumorphism))]
    public void Preset_Should_Have_Transitions(string presetName)
    {
        // Arrange
        BUITransitions transitions = (BUITransitions)typeof(BUITransitionPresets)
            .GetProperty(presetName)!
            .GetValue(null)!;

        // Assert
        transitions.HasTransitions.Should().BeTrue(
            because: $"preset '{presetName}' must define at least one transition entry");
    }

    [Fact]
    public void HoverScale_DataAttribute_Should_Contain_Hover_Scale()
    {
        // Arrange & Act
        string attr = BUITransitionPresets.HoverScale.GetDataAttributeValue();

        // Assert
        attr.Should().Contain("hover:scale");
    }

    [Fact]
    public void HoverShadow_DataAttribute_Should_Contain_Hover_BoxShadow()
    {
        // Arrange & Act
        string attr = BUITransitionPresets.HoverShadow.GetDataAttributeValue();

        // Assert
        attr.Should().Contain("hover:box-shadow");
    }

    [Fact]
    public void Interactive_DataAttribute_Should_Contain_Hover_Focus_Active()
    {
        // Arrange & Act
        string attr = BUITransitionPresets.Interactive.GetDataAttributeValue();

        // Assert
        attr.Should().Contain("hover:");
        attr.Should().Contain("focus:");
        attr.Should().Contain("active:");
    }

    [Fact]
    public void HoverLift_CssVariables_Should_Include_Translate_And_Shadow()
    {
        // Arrange & Act
        Dictionary<string, string> vars = BUITransitionPresets.HoverLift.GetCssVariables();

        // Assert — both translate and box-shadow vars present
        vars.Keys.Should().Contain(k => k.Contains("translate"),
            because: "HoverLift includes a translate transition");
        vars.Keys.Should().Contain(k => k.Contains("box-shadow"),
            because: "HoverLift includes a box-shadow transition");
    }

    [Fact]
    public void GetCssVariables_Should_Always_Include_Transition_Shorthand()
    {
        // Arrange & Act
        Dictionary<string, string> vars = BUITransitionPresets.HoverScale.GetCssVariables();

        // Assert — --bui-t-transition always present
        vars.Should().ContainKey("--bui-t-transition");
        vars["--bui-t-transition"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BUITransitions_MergeWith_Should_Override_Matching_Properties()
    {
        // Arrange
        BUITransitions base_ = BUITransitionPresets.HoverShadow;
        BUITransitions override_ = BUITransitionPresets.HoverGlow;

        // Act — merge; both have hover:box-shadow, override should win
        BUITransitions merged = base_.MergeWith(override_);
        Dictionary<string, string> mergedVars = merged.GetCssVariables();
        Dictionary<string, string> overrideVars = override_.GetCssVariables();

        // Assert — merged box-shadow value equals the override's value
        string? mergedShadow = mergedVars.GetValueOrDefault("--bui-t-hover-box-shadow");
        string? overrideShadow = overrideVars.GetValueOrDefault("--bui-t-hover-box-shadow");
        mergedShadow.Should().Be(overrideShadow);
    }
}
