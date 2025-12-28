using Bunit;
using CdCSharp.BlazorUI.Components.Features.Common;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Generic.Button;

[Trait("Components", "UIButton")]
public class ButtonSnapshotTests : TestContextBase
{
    [Fact(DisplayName = "AllVariants_MatchSnapshot")]
    public Task Button_AllVariants_MatchSnapshot()
    {
        // Arrange
        UIButtonVariant[] variants =
        [
            UIButtonVariant.Default,
        ];

        // Act
        var results = variants.Select(variant =>
        {
            IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
                .Add(p => p.Variant, variant)
                .Add(p => p.Text, $"{variant.Name} Button"));

            return new { Variant = variant.Name, Html = cut.Markup };
        });

        // Assert
        return Verify(results);
    }

    [Fact(DisplayName = "WithBorders_MatchSnapshot")]
    public Task Button_WithBorders_MatchSnapshot()
    {
        // Arrange
        var borderConfigs = new[]
        {
        new { Name = "Default", Border = BorderPresets.Default },
        new { Name = "Primary", Border = BorderPresets.Primary },
        new { Name = "Rounded", Border = BorderPresets.Rounded },
        new { Name = "Pill", Border = BorderPresets.Pill }
    };

        // Act
        var results = borderConfigs.Select(config =>
        {
            IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
                .Add(p => p.Text, $"{config.Name} Border")
                .Add(p => p.Border, config.Border));

            return new { BorderType = config.Name, Html = cut.Markup };
        });

        // Assert
        return Verify(results);
    }
}