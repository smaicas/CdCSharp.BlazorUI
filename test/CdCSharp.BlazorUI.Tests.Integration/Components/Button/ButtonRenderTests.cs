using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Components.Generic.Button;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Components.Button;

public class ButtonRenderTests : ComponentVariantTestBase<UIButton, UIButtonVariant>
{
    protected override UIButtonVariant[] GetAllVariants() =>
        [UIButtonVariant.Primary, UIButtonVariant.Secondary, UIButtonVariant.Success, UIButtonVariant.Danger];

    protected override string GetExpectedCssClass(UIButtonVariant variant) =>
        $"btn-{variant.Name.ToLower()}";

    [Fact]
    public void Button_WithContent_RendersContent()
    {
        // Arrange
        const string content = "Click me";

        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.ChildContent, content));
        string markup = cut.Markup;
        // Assert
        cut.Find("button").TextContent.Should().Be(content);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Button_DisabledState_RendersCorrectly(bool disabled)
    {
        // Act
        IRenderedComponent<UIButton> cut = Render<UIButton>(parameters => parameters
            .Add(p => p.Disabled, disabled));

        // Assert
        IElement button = cut.Find("button");
        if (disabled)
        {
            button.HasAttribute("disabled").Should().BeTrue();
        }
        else
        {
            button.HasAttribute("disabled").Should().BeFalse();
        }
    }
}
