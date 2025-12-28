using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Loading;

[Trait("Components", "UILoadingIndicator")]
public class UILoadingIndicatorVariantTests : TestContextBase
{
    [Fact(DisplayName = "SpinnerVariant_HasUniqueStructure")]
    public void LoadingIndicator_SpinnerVariant_HasUniqueStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Spinner));

        // Assert
        cut.Find("svg.ui-loading-spinner").Should().NotBeNull();
        cut.FindAll(".ui-loading-linear").Should().BeEmpty();
        cut.FindAll(".ui-loading-dot").Should().BeEmpty();
        cut.FindAll(".ui-loading-pulse").Should().BeEmpty();
    }

    [Fact(DisplayName = "LinearVariant_HasUniqueStructure")]
    public void LoadingIndicator_LinearVariant_HasUniqueStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.LinearIndeterminate));

        // Assert
        cut.Find(".ui-loading-linear").Should().NotBeNull();
        cut.FindAll("svg").Should().BeEmpty();
        cut.FindAll(".ui-loading-dot").Should().BeEmpty();
        cut.FindAll(".ui-loading-pulse").Should().BeEmpty();
    }

    [Fact(DisplayName = "CircularProgressVariant_HasUniqueStructure")]
    public void LoadingIndicator_CircularProgressVariant_HasUniqueStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.CircularProgress));

        // Assert
        cut.Find("svg.ui-loading-circular").Should().NotBeNull();
        cut.FindAll(".ui-loading-linear").Should().BeEmpty();
        cut.FindAll(".ui-loading-dot").Should().BeEmpty();
        cut.FindAll(".ui-loading-pulse").Should().BeEmpty();
    }

    [Fact(DisplayName = "DotsVariant_HasUniqueStructure")]
    public void LoadingIndicator_DotsVariant_HasUniqueStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Dots));

        // Assert
        cut.FindAll(".ui-loading-dot").Should().HaveCount(3);
        cut.FindAll("svg").Should().BeEmpty();
        cut.FindAll(".ui-loading-linear").Should().BeEmpty();
        cut.FindAll(".ui-loading-pulse").Should().BeEmpty();
    }

    [Fact(DisplayName = "PulseVariant_HasUniqueStructure")]
    public void LoadingIndicator_PulseVariant_HasUniqueStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Pulse));

        // Assert
        cut.Find(".ui-loading-pulse").Should().NotBeNull();
        cut.FindAll("svg").Should().BeEmpty();
        cut.FindAll(".ui-loading-linear").Should().BeEmpty();
        cut.FindAll(".ui-loading-dot").Should().BeEmpty();
    }

    [Theory(DisplayName = "AllVariants_RenderWithoutErrors")]
    [InlineData("Spinner")]
    [InlineData("LinearIndeterminate")]
    [InlineData("CircularProgress")]
    [InlineData("Dots")]
    [InlineData("Pulse")]
    public void LoadingIndicator_AllVariants_RenderWithoutErrors(string variantName)
    {
        // Arrange
        UILoadingIndicatorVariant variant = variantName switch
        {
            "Spinner" => UILoadingIndicatorVariant.Spinner,
            "LinearIndeterminate" => UILoadingIndicatorVariant.LinearIndeterminate,
            "CircularProgress" => UILoadingIndicatorVariant.CircularProgress,
            "Dots" => UILoadingIndicatorVariant.Dots,
            "Pulse" => UILoadingIndicatorVariant.Pulse,
            _ => throw new ArgumentException($"Unknown variant: {variantName}")
        };

        // Act & Assert - Should not throw
        Action act = () =>
        {
            IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
                .Add(p => p.Variant, variant));
        };

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "UnknownVariant_RendersNothing")]
    public void LoadingIndicator_UnknownVariant_RendersNothing()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Custom("Unknown")));

        // Assert
        cut.Markup.Should().BeEmpty();
    }

    [Theory(DisplayName = "AllVariants_ApplyCorrectCssClasses")]
    [InlineData("Spinner", "ui-loading-indicator--spinner")]
    [InlineData("LinearIndeterminate", "ui-loading-indicator--linearindeterminate")]
    [InlineData("CircularProgress", "ui-loading-indicator--circularprogress")]
    [InlineData("Dots", "ui-loading-indicator--dots")]
    [InlineData("Pulse", "ui-loading-indicator--pulse")]
    public void LoadingIndicator_AllVariants_ApplyCorrectCssClasses(string variantName, string expectedClass)
    {
        // Arrange
        UILoadingIndicatorVariant variant = variantName switch
        {
            "Spinner" => UILoadingIndicatorVariant.Spinner,
            "LinearIndeterminate" => UILoadingIndicatorVariant.LinearIndeterminate,
            "CircularProgress" => UILoadingIndicatorVariant.CircularProgress,
            "Dots" => UILoadingIndicatorVariant.Dots,
            "Pulse" => UILoadingIndicatorVariant.Pulse,
            _ => throw new ArgumentException($"Unknown variant: {variantName}")
        };

        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, variant));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-loading-indicator");
        container.ShouldHaveClass(expectedClass);
    }

    [Fact(DisplayName = "VariantWithCustomClasses_MergesCorrectly")]
    public void LoadingIndicator_VariantWithCustomClasses_MergesCorrectly()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Dots)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "custom-loader" }
            }));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-loading-indicator");
        container.ShouldHaveClass("ui-loading-indicator--dots");
        container.ShouldHaveClass("custom-loader");
    }

    [Fact(DisplayName = "SpinnerAndCircular_BothUseSvg")]
    public void LoadingIndicator_SpinnerAndCircular_BothUseSvg()
    {
        // Act - Spinner
        IRenderedComponent<UILoadingIndicator> spinner = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Spinner));

        // Act - Circular
        IRenderedComponent<UILoadingIndicator> circular = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.CircularProgress));

        // Assert
        spinner.Find("svg").Should().NotBeNull();
        circular.Find("svg").Should().NotBeNull();

        // But they have different classes
        spinner.Find("svg").ShouldHaveClass("ui-loading-spinner");
        circular.Find("svg").ShouldHaveClass("ui-loading-circular");
    }
}