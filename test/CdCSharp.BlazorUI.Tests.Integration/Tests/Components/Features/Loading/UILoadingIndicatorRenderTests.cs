using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Loading;

[Trait("Components", "UILoadingIndicator")]
public class UILoadingIndicatorRenderTests : TestContextBase
{
    [Fact(DisplayName = "SpinnerVariant_RendersCorrectStructure")]
    public void LoadingIndicator_SpinnerVariant_RendersCorrectStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Spinner));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-loading-indicator");
        container.ShouldHaveClass("ui-loading-indicator--spinner");

        IElement svg = cut.Find("svg.ui-loading-spinner");
        svg.Should().NotBeNull();
        svg.GetAttribute("viewBox").Should().Be("0 0 50 50");

        IElement circle = svg.QuerySelector("circle");
        circle.Should().NotBeNull();
        circle.GetAttribute("cx").Should().Be("25");
        circle.GetAttribute("cy").Should().Be("25");
        circle.GetAttribute("r").Should().Be("20");
        circle.GetAttribute("fill").Should().Be("none");
        circle.GetAttribute("stroke").Should().Be("currentColor");
        circle.GetAttribute("stroke-width").Should().Be("4");
    }

    [Fact(DisplayName = "LinearVariant_RendersCorrectStructure")]
    public void LoadingIndicator_LinearVariant_RendersCorrectStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.LinearIndeterminate));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-loading-indicator");
        container.ShouldHaveClass("ui-loading-indicator--linearindeterminate");

        IElement linear = cut.Find(".ui-loading-linear");
        linear.Should().NotBeNull();

        IElement bar = linear.QuerySelector(".ui-loading-linear__bar");
        bar.Should().NotBeNull();
    }

    [Fact(DisplayName = "CircularProgressVariant_RendersCorrectStructure")]
    public void LoadingIndicator_CircularProgressVariant_RendersCorrectStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.CircularProgress));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-loading-indicator");
        container.ShouldHaveClass("ui-loading-indicator--circularprogress");

        IElement svg = cut.Find("svg.ui-loading-circular");
        svg.Should().NotBeNull();
        svg.GetAttribute("viewBox").Should().Be("0 0 50 50");

        IElement circle = svg.QuerySelector("circle");
        circle.Should().NotBeNull();
    }

    [Fact(DisplayName = "DotsVariant_RendersCorrectStructure")]
    public void LoadingIndicator_DotsVariant_RendersCorrectStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Dots));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-loading-indicator");
        container.ShouldHaveClass("ui-loading-indicator--dots");

        IReadOnlyList<IElement> dots = cut.FindAll("span.ui-loading-dot");
        dots.Should().HaveCount(3);
    }

    [Fact(DisplayName = "PulseVariant_RendersCorrectStructure")]
    public void LoadingIndicator_PulseVariant_RendersCorrectStructure()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Variant, UILoadingIndicatorVariant.Pulse));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-loading-indicator");
        container.ShouldHaveClass("ui-loading-indicator--pulse");

        IElement pulse = cut.Find("span.ui-loading-pulse");
        pulse.Should().NotBeNull();
    }

    [Theory(DisplayName = "WithSize_AppliesCorrectClass")]
    [InlineData(SizeEnum.Small, "ui-size-small")]
    [InlineData(SizeEnum.Medium, "ui-size-medium")]
    [InlineData(SizeEnum.Large, "ui-size-large")]
    public void LoadingIndicator_WithSize_AppliesCorrectClass(SizeEnum size, string expectedClass)
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Size, size));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass(expectedClass);
    }

    [Fact(DisplayName = "WithColor_AppliesCorrectStyle")]
    public void LoadingIndicator_WithColor_AppliesCorrectStyle()
    {
        // Arrange
        CssColor customColor = new("#FF5733");

        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Color, customColor));

        // Assert
        IElement container = cut.Find("div");
        string? style = container.GetAttribute("style");
        style.Should().Contain("color: rgba(255,87,51,1)");
    }

    [Fact(DisplayName = "WithAdditionalAttributes_MergesCorrectly")]
    public void LoadingIndicator_WithAdditionalAttributes_MergesCorrectly()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "data-test-id", "loading" },
                { "class", "custom-loading" },
                { "style", "margin: 10px" }
            }));

        // Assert
        IElement container = cut.Find("div");
        container.GetAttribute("data-test-id").Should().Be("loading");
        container.ShouldHaveClass("ui-loading-indicator");
        container.ShouldHaveClass("ui-loading-indicator--spinner");
        container.ShouldHaveClass("custom-loading");

        string? style = container.GetAttribute("style");
        style.Should().Contain("margin: 10px");
    }

    [Fact(DisplayName = "DefaultVariant_IsSpinner")]
    public void LoadingIndicator_DefaultVariant_IsSpinner()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>();

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-loading-indicator--spinner");
        cut.Find("svg.ui-loading-spinner").Should().NotBeNull();
    }

    [Fact(DisplayName = "DefaultSize_IsMedium")]
    public void LoadingIndicator_DefaultSize_IsMedium()
    {
        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>();

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-size-medium");
    }

    [Fact(DisplayName = "AllVariants_HaveBaseClass")]
    public void LoadingIndicator_AllVariants_HaveBaseClass()
    {
        // Arrange
        UILoadingIndicatorVariant[] variants =
        [
            UILoadingIndicatorVariant.Spinner,
            UILoadingIndicatorVariant.LinearIndeterminate,
            UILoadingIndicatorVariant.CircularProgress,
            UILoadingIndicatorVariant.Dots,
            UILoadingIndicatorVariant.Pulse
        ];

        foreach (UILoadingIndicatorVariant variant in variants)
        {
            // Act
            IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
                .Add(p => p.Variant, variant));

            // Assert
            IElement container = cut.Find("div");
            container.ShouldHaveClass("ui-loading-indicator");
            container.ShouldHaveClass($"ui-loading-indicator--{variant.Name.ToLower()}");
        }
    }

    [Fact(DisplayName = "SizeAndColor_WorkTogether")]
    public void LoadingIndicator_SizeAndColor_WorkTogether()
    {
        // Arrange
        CssColor color = UIColor.Blue.Default;
        SizeEnum size = SizeEnum.Large;

        // Act
        IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
            .Add(p => p.Size, size)
            .Add(p => p.Color, color));

        // Assert
        IElement container = cut.Find("div");
        container.ShouldHaveClass("ui-size-large");

        string? style = container.GetAttribute("style");
        style.Should().Contain("color:");
    }
}