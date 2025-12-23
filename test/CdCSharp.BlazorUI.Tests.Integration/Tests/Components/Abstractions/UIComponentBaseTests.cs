using AngleSharp.Dom;
using Bunit;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Templates.Components;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Abstractions;

[Trait("Abstractions", "UIComponentBase")]
public class UIComponentBaseTests : TestContextBase
{
    [Fact(DisplayName = "ComputedCssClasses_ContainsAllClasses")]
    public void UIComponentBase_ComputedCssClasses_ContainsAllClasses()
    {
        // Arrange & Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.IsPrimary, true)
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-class" }
            }));

        // Assert
        TestComponent instance = cut.Instance;
        instance.ComputedCssClasses.Should().Be("test-component test-component--primary user-class");
    }

    [Fact(DisplayName = "ConditionalClasses_AppliedCorrectly")]
    public void UIComponentBase_ConditionalClasses_AppliedCorrectly()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.IsPrimary, true));

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("class").Should().Be("test-component test-component--primary");
    }

    [Fact(DisplayName = "EmptyUserClass_HandledCorrectly")]
    public void UIComponentBase_EmptyUserClass_HandledCorrectly()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "" }
            }));

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("class").Should().Be("test-component");
    }

    [Fact(DisplayName = "InlineStyles_AppliedCorrectly")]
    public void UIComponentBase_InlineStyles_AppliedCorrectly()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.Color, "red")
            .Add(p => p.BackgroundColor, "blue"));

        // Assert
        IElement div = cut.Find("div");
        string? style = div.GetAttribute("style");
        style.Should().Contain("color: red");
        style.Should().Contain("background-color: blue");
    }

    [Fact(DisplayName = "OtherAttributes_PassedThrough")]
    public void UIComponentBase_OtherAttributes_PassedThrough()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "id", "test-id" },
                { "data-test", "value" },
                { "aria-label", "Test component" }
            }));

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("id").Should().Be("test-id");
        div.GetAttribute("data-test").Should().Be("value");
        div.GetAttribute("aria-label").Should().Be("Test component");
    }

    [Fact(DisplayName = "UserStyles_MergedWithComponentStyles")]
    public void UIComponentBase_UserStyles_MergedWithComponentStyles()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.Color, "red")
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "style", "margin: 10px" }
            }));

        // Assert
        IElement div = cut.Find("div");
        string? style = div.GetAttribute("style");
        style.Should().Contain("color: red");
        style.Should().Contain("margin: 10px");
    }

    [Fact(DisplayName = "WithoutUserClasses_RendersOnlyComponentClasses")]
    public void UIComponentBase_WithoutUserClasses_RendersOnlyComponentClasses()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>();

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("class").Should().Be("test-component");
    }

    [Fact(DisplayName = "WithUserClasses_MergesClasses")]
    public void UIComponentBase_WithUserClasses_MergesClasses()
    {
        // Act
        IRenderedComponent<TestComponent> cut = Render<TestComponent>(parameters => parameters
            .Add(p => p.AdditionalAttributes, new Dictionary<string, object>
            {
                { "class", "user-class-1 user-class-2" }
            }));

        // Assert
        IElement div = cut.Find("div");
        div.GetAttribute("class").Should().Be("test-component user-class-1 user-class-2");
    }
}