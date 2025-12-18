using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Generic.Svg;

[Trait("Components", "UISvgIcon")]
public class UISvgIconSnapshotTests : TestContextBase
{
    private const string TestIcon = "<path d=\"M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z\"/>";

    [Fact(DisplayName = "AllSizes_MatchSnapshot")]
    public Task SvgIcon_AllSizes_MatchSnapshot()
    {
        // Arrange
        UISvgIcon.UISvgIconSize[] sizes =
        [
            UISvgIcon.UISvgIconSize.Small,
            UISvgIcon.UISvgIconSize.Medium,
            UISvgIcon.UISvgIconSize.Large,
            UISvgIcon.UISvgIconSize.XLarge,
            UISvgIcon.UISvgIconSize.XXLarge
        ];

        // Act
        var results = sizes.Select(size =>
        {
            IRenderedComponent<UISvgIcon> cut = Render<UISvgIcon>(parameters => parameters
                .Add(p => p.Icon, TestIcon)
                .Add(p => p.Size, size));

            return new { Size = size.ToString(), Html = cut.Markup };
        });

        // Assert
        return Verify(results);
    }
}
