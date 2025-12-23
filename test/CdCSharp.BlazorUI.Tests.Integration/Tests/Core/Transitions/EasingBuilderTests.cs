using CdCSharp.BlazorUI.Core.Transitions;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Core.Transitions;

[Trait("Transitions", "EasingBuilder")]
public class EasingBuilderTests
{
    [Fact(DisplayName = "StaticConstants_HaveCorrectValues")]
    public void Easing_StaticConstants_HaveCorrectValues()
    {
        // Assert
        Easing.Linear.Should().Be("linear");
        Easing.Ease.Should().Be("ease");
        Easing.EaseIn.Should().Be("ease-in");
        Easing.EaseOut.Should().Be("ease-out");
        Easing.EaseInOut.Should().Be("ease-in-out");
    }

    [Fact(DisplayName = "ChainedOperations_OverwritePrevious")]
    public void EasingBuilder_ChainedOperations_OverwritePrevious()
    {
        // Act
        string result = Easing.Create()
            .Linear()
            .EaseIn()
            .CubicBezier().MaterialStandard()
            .Build();

        // Assert
        result.Should().Be("cubic-bezier(0.400, 0.000, 0.200, 1.000)");
    }

    [Fact(DisplayName = "Create_ReturnsNewBuilder")]
    public void EasingBuilder_Create_ReturnsNewBuilder()
    {
        // Act
        EasingBuilder builder = Easing.Create();

        // Assert
        builder.Should().NotBeNull();
        builder.Build().Should().Be("ease");
    }

    [Theory(DisplayName = "CubicBezier_Presets_ReturnCorrectValues")]
    [InlineData("MaterialStandard", "cubic-bezier(0.400, 0.000, 0.200, 1.000)")]
    [InlineData("MaterialDecelerate", "cubic-bezier(0.000, 0.000, 0.200, 1.000)")]
    [InlineData("MaterialAccelerate", "cubic-bezier(0.400, 0.000, 1.000, 1.000)")]
    [InlineData("MaterialSharp", "cubic-bezier(0.400, 0.000, 0.600, 1.000)")]
    [InlineData("Bounce", "cubic-bezier(0.680, -0.550, 0.265, 1.550)")]
    [InlineData("Elastic", "cubic-bezier(0.680, -0.600, 0.320, 1.600)")]
    [InlineData("BackOut", "cubic-bezier(0.175, 0.885, 0.320, 1.275)")]
    public void EasingBuilder_CubicBezier_Presets_ReturnCorrectValues(string preset, string expected)
    {
        // Arrange
        CubicBezierBuilder bezierBuilder = Easing.Create().CubicBezier();

        // Act
        string result = preset switch
        {
            "MaterialStandard" => bezierBuilder.MaterialStandard().Build(),
            "MaterialDecelerate" => bezierBuilder.MaterialDecelerate().Build(),
            "MaterialAccelerate" => bezierBuilder.MaterialAccelerate().Build(),
            "MaterialSharp" => bezierBuilder.MaterialSharp().Build(),
            "Bounce" => bezierBuilder.Bounce().Build(),
            "Elastic" => bezierBuilder.Elastic().Build(),
            "BackOut" => bezierBuilder.BackOut().Build(),
            _ => throw new ArgumentException($"Unknown preset: {preset}")
        };

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "CubicBezier_WithControlPoints_ReturnsCorrectValue")]
    public void EasingBuilder_CubicBezier_WithControlPoints_ReturnsCorrectValue()
    {
        // Act
        string result = Easing.Create()
            .CubicBezier()
            .WithControlPoints(0.4, 0.0, 0.2, 1)
            .Build();

        // Assert
        result.Should().Be("cubic-bezier(0.400, 0.000, 0.200, 1.000)");
    }

    [Fact(DisplayName = "Custom_ReturnsProvidedValue")]
    public void EasingBuilder_Custom_ReturnsProvidedValue()
    {
        // Arrange
        const string customEasing = "cubic-bezier(0.1, 0.2, 0.3, 0.4)";

        // Act
        string result = Easing.Create()
            .Custom(customEasing)
            .Build();

        // Assert
        result.Should().Be(customEasing);
    }

    [Fact(DisplayName = "ImplicitConversion_ReturnsBuiltValue")]
    public void EasingBuilder_ImplicitConversion_ReturnsBuiltValue()
    {
        // Arrange
        EasingBuilder builder = Easing.Create().Linear();

        // Act
        string result = builder; // Implicit conversion

        // Assert
        result.Should().Be("linear");
    }

    [Theory(DisplayName = "PredefinedEasings_ReturnCorrectValues")]
    [InlineData("Linear", "linear")]
    [InlineData("Ease", "ease")]
    [InlineData("EaseIn", "ease-in")]
    [InlineData("EaseOut", "ease-out")]
    [InlineData("EaseInOut", "ease-in-out")]
    public void EasingBuilder_PredefinedEasings_ReturnCorrectValues(string methodName, string expected)
    {
        // Arrange
        EasingBuilder builder = Easing.Create();

        // Act
        string result = methodName switch
        {
            "Linear" => builder.Linear().Build(),
            "Ease" => builder.Ease().Build(),
            "EaseIn" => builder.EaseIn().Build(),
            "EaseOut" => builder.EaseOut().Build(),
            "EaseInOut" => builder.EaseInOut().Build(),
            _ => throw new ArgumentException($"Unknown method: {methodName}")
        };

        // Assert
        result.Should().Be(expected);
    }

    [Theory(DisplayName = "Steps_Positions_ReturnCorrectValues")]
    [InlineData("Start", "steps(3, start)")]
    [InlineData("End", "steps(3, end)")]
    [InlineData("JumpStart", "steps(3, jump-start)")]
    [InlineData("JumpEnd", "steps(3, jump-end)")]
    [InlineData("JumpNone", "steps(3, jump-none)")]
    [InlineData("JumpBoth", "steps(3, jump-both)")]
    public void EasingBuilder_Steps_Positions_ReturnCorrectValues(string position, string expected)
    {
        // Arrange
        StepsBuilder stepsBuilder = Easing.Create().Steps(3);

        // Act
        string result = position switch
        {
            "Start" => stepsBuilder.Start().Build(),
            "End" => stepsBuilder.End().Build(),
            "JumpStart" => stepsBuilder.JumpStart().Build(),
            "JumpEnd" => stepsBuilder.JumpEnd().Build(),
            "JumpNone" => stepsBuilder.JumpNone().Build(),
            "JumpBoth" => stepsBuilder.JumpBoth().Build(),
            _ => throw new ArgumentException($"Unknown position: {position}")
        };

        // Assert
        result.Should().Be(expected);
    }

    [Fact(DisplayName = "Steps_WithCount_ReturnsCorrectValue")]
    public void EasingBuilder_Steps_WithCount_ReturnsCorrectValue()
    {
        // Act
        string result = Easing.Create()
            .Steps(5)
            .Build();

        // Assert
        result.Should().Be("steps(5, end)");
    }
}