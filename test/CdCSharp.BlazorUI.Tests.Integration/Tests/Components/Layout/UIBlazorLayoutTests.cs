using Bunit;
using CdCSharp.BlazorUI.Components.Features.Theme.ThemeSwitch;
using CdCSharp.BlazorUI.Components.Layout;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Layout;

[Trait("Components", "UIBlazorLayout")]
public class UIBlazorLayoutTests : TestContextBase
{
    private readonly IThemeJsInterop _mockThemeInterop;

    public UIBlazorLayoutTests()
    {
        _mockThemeInterop = Substitute.For<IThemeJsInterop>();
        Services.AddSingleton(_mockThemeInterop);
    }

    [Fact(DisplayName = "InitializesThemeOnFirstRender")]
    public async Task UIBlazorLayout_InitializesThemeOnFirstRender()
    {
        // Act
        IRenderedComponent<UIBlazorLayout> cut = Render<UIBlazorLayout>(parameters => parameters
            .Add(p => p.Body, "<p>Test Content</p>"));

        // Assert
        await _mockThemeInterop.Received(1).InitializeAsync("dark");
    }

    [Fact(DisplayName = "RendersBodyContent")]
    public void UIBlazorLayout_RendersBodyContent()
    {
        // Arrange
        const string bodyContent = "<div class='test-content'>Hello World</div>";

        // Act
        IRenderedComponent<UIBlazorLayout> cut = Render<UIBlazorLayout>(parameters => parameters
            .Add(p => p.Body, bodyContent));

        // Assert
        cut.Markup.Should().Contain("test-content");
        cut.Find(".test-content").TextContent.Should().Be("Hello World");
    }
}
