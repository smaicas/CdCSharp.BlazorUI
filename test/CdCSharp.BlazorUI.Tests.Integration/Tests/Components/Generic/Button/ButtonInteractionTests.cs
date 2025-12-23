using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Generic.Button;

[Trait("Components", "UIButton")]
public class ButtonInteractionTests : TestContextBase
{
    [Fact(DisplayName = "Click_InvokesCallback")]
    public async Task Button_Click_InvokesCallback()
    {
        // Arrange
        bool wasClicked = false;
        Bunit.IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => wasClicked = true)));

        // Act
        await cut.Find("button").ClickAsync();

        // Assert
        wasClicked.Should().BeTrue();
    }
}