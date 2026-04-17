using Bunit;
using CdCSharp.BlazorUI.Components;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure;
using CdCSharp.BlazorUI.Tests.Integration.Infrastructure.Contexts;
using FluentAssertions;

namespace CdCSharp.BlazorUI.Tests.Integration.Tests.Components.Loading;

[Trait("Component Accessibility", "BUILoadingIndicator")]
public class BUILoadingIndicatorAccessibilityTests
{
    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Role_Status_On_Spinner(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        // Assert
        cut.Find("bui-component").GetAttribute("role").Should().Be("status");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Custom_Aria_Label(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.AriaLabel, "Processing your request"));

        // Assert
        cut.Find("bui-component").GetAttribute("aria-label").Should().Be("Processing your request");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Role_Progressbar_On_Linear_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Variant, BUILoadingIndicatorVariant.LinearIndeterminate));

        // Assert
        cut.Find("bui-component").GetAttribute("role").Should().Be("progressbar");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_ValueMin_Max_On_Linear_Variant(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>(p => p
            .Add(c => c.Variant, BUILoadingIndicatorVariant.LinearIndeterminate));

        // Assert
        cut.Find("bui-component").GetAttribute("aria-valuemin").Should().Be("0");
        cut.Find("bui-component").GetAttribute("aria-valuemax").Should().Be("100");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Have_Aria_Live_Polite_On_Spinner(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        // Assert
        cut.Find("bui-component").GetAttribute("aria-live").Should().Be("polite");
    }

    [Theory]
    [MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]
    public async Task Should_Svg_Be_Aria_Hidden(BlazorScenario scenario)
    {
        await using BlazorTestContextBase ctx = scenario.CreateContext();

        // Arrange & Act
        IRenderedComponent<BUILoadingIndicator> cut = ctx.Render<BUILoadingIndicator>();

        // Assert — svg is decorative, hidden from screen readers
        cut.Find("svg").GetAttribute("aria-hidden").Should().Be("true");
    }
}
