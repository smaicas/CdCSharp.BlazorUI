using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

namespace CdCSharp.BlazorUI.Tests.Integration.Components.Button;

public class ButtonSnapshotTests : TestContextBase
{
    [Fact]
    public Task Button_AllVariants_MatchSnapshot()
    {
        // Arrange
        UIButtonVariant[] variants =
        [
            UIButtonVariant.Primary,
            UIButtonVariant.Secondary,
            UIButtonVariant.Success,
            UIButtonVariant.Danger
        ];

        // Act
        var results = variants.Select(variant =>
        {
            IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
                .Add(p => p.Variant, variant)
                .Add(p => p.ChildContent, $"{variant.Name} Button"));

            return new { Variant = variant.Name, Html = cut.Markup };
        });

        // Assert
        return Verify(results);
    }
}
