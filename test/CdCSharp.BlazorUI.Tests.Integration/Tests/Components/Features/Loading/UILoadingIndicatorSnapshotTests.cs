using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Features.Loading;
using CdCSharp.BlazorUI.Core.Css;
using CdCSharp.BlazorUI.Css;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Features.Loading;

[Trait("Components", "UILoadingIndicator")]
public class UILoadingIndicatorSnapshotTests : TestContextBase
{
    [Fact(DisplayName = "AllVariants_MatchSnapshot")]
    public Task LoadingIndicator_AllVariants_MatchSnapshot()
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

        // Act
        var results = variants.Select(variant =>
        {
            IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
                .Add(p => p.Variant, variant));

            return new { Variant = variant.Name, Html = cut.Markup };
        });

        // Assert
        return Verify(results);
    }

    [Fact(DisplayName = "AllSizes_MatchSnapshot")]
    public Task LoadingIndicator_AllSizes_MatchSnapshot()
    {
        // Arrange
        SizeEnum[] sizes =
        [
            SizeEnum.Small,
            SizeEnum.Medium,
            SizeEnum.Large
        ];

        // Act
        var results = sizes.Select(size =>
        {
            IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
                .Add(p => p.Size, size));

            return new { Size = size.ToString(), Html = cut.Markup };
        });

        // Assert
        return Verify(results);
    }

    [Fact(DisplayName = "AllVariantsWithSizes_MatchSnapshot")]
    public Task LoadingIndicator_AllVariantsWithSizes_MatchSnapshot()
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

        SizeEnum[] sizes =
        [
            SizeEnum.Small,
            SizeEnum.Medium,
            SizeEnum.Large
        ];

        // Act
        var results = from variant in variants
                      from size in sizes
                      select new
                      {
                          Variant = variant.Name,
                          Size = size.ToString(),
                          Html = Render<UILoadingIndicator>(parameters => parameters
                              .Add(p => p.Variant, variant)
                              .Add(p => p.Size, size)).Markup
                      };

        // Assert
        return Verify(results);
    }

    [Fact(DisplayName = "WithColors_MatchSnapshot")]
    public Task LoadingIndicator_WithColors_MatchSnapshot()
    {
        // Arrange
        var colorConfigs = new[]
        {
            new { Name = "Primary", Color = UIColor.Blue.Default },
            new { Name = "Success", Color = UIColor.Green.Default },
            new { Name = "Warning", Color = UIColor.Orange.Default },
            new { Name = "Error", Color = UIColor.Red.Default },
            new { Name = "Custom", Color = new CssColor("#9C27B0") }
        };

        // Act
        var results = colorConfigs.Select(config =>
        {
            IRenderedComponent<UILoadingIndicator> cut = Render<UILoadingIndicator>(parameters => parameters
                .Add(p => p.Color, config.Color));

            return new { ColorName = config.Name, Html = cut.Markup };
        });

        // Assert
        return Verify(results);
    }

    [Fact(DisplayName = "ComplexScenarios_MatchSnapshot")]
    public Task LoadingIndicator_ComplexScenarios_MatchSnapshot()
    {
        // Arrange & Act
        var results = new[]
        {
            new
            {
                Scenario = "Large_Dots_WithColor",
                Html = Render<UILoadingIndicator>(parameters => parameters
                    .Add(p => p.Variant, UILoadingIndicatorVariant.Dots)
                    .Add(p => p.Size, SizeEnum.Large)
                    .Add(p => p.Color, UIColor.Purple.Default)).Markup
            },
            new
            {
                Scenario = "Small_Spinner_WithCustomClass",
                Html = Render<UILoadingIndicator>(parameters => parameters
                    .Add(p => p.Variant, UILoadingIndicatorVariant.Spinner)
                    .Add(p => p.Size, SizeEnum.Small)
                    .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
                    {
                        { "class", "custom-spinner" }
                    })).Markup
            },
            new
            {
                Scenario = "Linear_WithInlineStyles",
                Html = Render<UILoadingIndicator>(parameters => parameters
                    .Add(p => p.Variant, UILoadingIndicatorVariant.LinearIndeterminate)
                    .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
                    {
                        { "style", "width: 200px; margin: 20px;" }
                    })).Markup
            }
        };

        // Assert
        return Verify(results);
    }
}