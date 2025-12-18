using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Svg;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Generic.Svg;

[Trait("Components", "UISvgIcon")]
public class UISvgIconInteractionTests : TestContextBase
{
    // Use <path></path> instead of <path/> because AngleSharp normalizes <path/> to <path></path>
    private const string TestIcon = "<path d=\"M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z\"></path>";

    [Fact(DisplayName = "Click_InvokesCallback")]
    public async Task SvgIcon_Click_InvokesCallback()
    {
        // Arrange
        bool wasClicked = false;

        // Act
        IRenderedComponent<UISvgIcon> cut = Render<UISvgIcon>(parameters => parameters
            .Add(p => p.Icon, TestIcon)
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => wasClicked = true)));

        await cut.Find("svg").ClickAsync();

        // Assert
        wasClicked.Should().BeTrue();
    }
}
